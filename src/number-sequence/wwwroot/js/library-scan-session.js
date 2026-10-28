// The scan session page (Pages/UI/Libraries/Scans/Scan.cshtml); see docs/library.md. Every code, from the camera or the
// text box (which a handheld scanner types into), is posted to the api in order, one at a time, and the list shows what
// each matched. The api calls go straight to /library/... with the ns-token cookie, which is SameSite=Strict.
//
// Codes not yet saved wait in a queue, kept in localStorage per session, and are retried until the api takes them, so a
// dropped connection in a basement or a reload loses nothing. A refusal (a bad check digit, a session already applied)
// drops the code and says so; only network failures and 5xx are retried.
(function () {
  'use strict';

  var root = document.getElementById('scan-session');
  var libraryId = root.dataset.libraryId;
  var sessionId = root.dataset.sessionId;
  var here = Number(root.dataset.locationId);
  var paths = JSON.parse(root.dataset.paths || '{}');
  var canEdit = root.dataset.canEdit === 'true';
  var api = '/library/libraries/' + libraryId + '/scans/' + sessionId;

  var list = root.querySelector('[data-entries]');
  var summary = root.querySelector('[data-summary]');
  var form = root.querySelector('[data-code-form]');
  var input = form.querySelector('input');
  var cameraButton = root.querySelector('[data-camera]');

  var queueKey = 'ns-scan-queue-' + sessionId;
  var entries = [];
  var failures = [];
  var queue = readQueue();
  var sending = false;
  var retryTimer = null;
  var camera = null;

  function readQueue() {
    try {
      var stored = JSON.parse(localStorage.getItem(queueKey));
      return Array.isArray(stored) ? stored : [];
    } catch (e) {
      return [];
    }
  }

  function writeQueue() {
    try {
      if (queue.length > 0) {
        localStorage.setItem(queueKey, JSON.stringify(queue));
      } else {
        localStorage.removeItem(queueKey);
      }
    } catch (e) { }
  }

  // What a saved entry means, as a badge, a title and a line of detail. The copy is as it is now, which is also how
  // applying the session will treat it.
  function describe(entry) {
    var copy = entry.resolvedCopy;
    var title = copy && copy.item ? copy.item.title : entry.barcode;
    var notes = [];
    if (copy && copy.status === 'Missing') {
      notes.push('was missing, found');
    }
    if (copy && copy.loans && copy.loans.length > 0) {
      notes.push('on loan to ' + copy.loans[0].borrowerName);
    }
    switch (entry.resolution) {
      case 'Expected':
        return { badge: 'Here', cls: 'text-bg-success', title: title, detail: notes.join('; '), tone: 'ok' };
      case 'Elsewhere':
        notes.unshift('recorded at ' + (copy && copy.locationId ? (paths[copy.locationId] || 'another location') : 'no location'));
        return { badge: 'Elsewhere', cls: 'text-bg-warning', title: title, detail: notes.join('; '), tone: 'move' };
      case 'Added':
        return { badge: 'Added', cls: 'text-bg-primary', title: title, detail: '', tone: 'ok' };
      default:
        return { badge: 'New', cls: 'text-bg-info', title: entry.barcode, detail: 'not in this library, or every copy already scanned', tone: 'new' };
    }
  }

  function row(badge, cls, title, detail, entryId) {
    var li = document.createElement('li');
    li.className = 'list-group-item d-flex align-items-center gap-2 py-2';
    var tag = document.createElement('span');
    tag.className = 'badge ' + cls + ' flex-shrink-0';
    tag.textContent = badge;
    var text = document.createElement('div');
    text.className = 'flex-grow-1 text-break';
    var main = document.createElement('div');
    main.textContent = title;
    text.appendChild(main);
    if (detail) {
      var sub = document.createElement('div');
      sub.className = 'small text-muted';
      sub.textContent = detail;
      text.appendChild(sub);
    }
    li.appendChild(tag);
    li.appendChild(text);
    if (entryId && canEdit) {
      var remove = document.createElement('button');
      remove.type = 'button';
      remove.className = 'btn btn-sm btn-outline-secondary flex-shrink-0';
      remove.setAttribute('aria-label', 'Remove this scan');
      remove.textContent = '×';
      remove.addEventListener('click', function () { removeEntry(entryId, remove); });
      li.appendChild(remove);
    }
    return li;
  }

  function render() {
    var rows = [];
    queue.slice().reverse().forEach(function (item) {
      rows.push(row('Saving', 'text-bg-secondary', item.code, 'waiting for the server', null));
    });
    failures.forEach(function (failure) {
      rows.push(row('Refused', 'text-bg-danger', failure.code, failure.message, null));
    });
    entries.slice().reverse().forEach(function (entry) {
      var d = describe(entry);
      rows.push(row(d.badge, d.cls, d.title, d.detail, entry.id));
    });
    list.replaceChildren.apply(list, rows);

    var counts = { Expected: 0, Elsewhere: 0, NotInLibrary: 0, Added: 0 };
    entries.forEach(function (entry) { counts[entry.resolution] = (counts[entry.resolution] || 0) + 1; });
    var parts = [entries.length + ' scanned', counts.Expected + ' here', counts.Elsewhere + ' elsewhere', (counts.NotInLibrary + counts.Added) + ' new'];
    if (queue.length > 0) {
      parts.push(queue.length + ' waiting to save');
    }
    summary.textContent = parts.join(' · ');
  }

  function scanned(code) {
    code = (code || '').trim();
    if (!code) {
      return;
    }
    queue.push({ code: code });
    writeQueue();
    render();
    send();
  }

  // One request at a time, in scan order: each match depends on which copies earlier scans claimed.
  async function send() {
    if (sending || queue.length === 0) {
      return;
    }
    sending = true;
    clearTimeout(retryTimer);
    var item = queue[0];
    try {
      var response = await fetch(api + '/entries', {
        method: 'POST',
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ barcode: item.code }),
      });
      if (response.ok) {
        var entry = await response.json();
        queue.shift();
        entries.push(entry);
        var d = describe(entry);
        report(d.badge + ': ' + d.title + (d.detail ? ' (' + d.detail + ')' : ''), d.tone);
      } else if (response.status >= 400 && response.status < 500) {
        queue.shift();
        var message = await errorText(response);
        failures.unshift({ code: item.code, message: message });
        report('Refused: ' + item.code + '. ' + message, 'error');
      } else {
        throw new Error('The server returned ' + response.status + '.');
      }
      writeQueue();
      sending = false;
      render();
      send();
    } catch (e) {
      sending = false;
      render();
      report(queue.length + ' waiting to save; retrying.', null);
      retryTimer = setTimeout(send, 3000);
    }
  }

  // The camera view's status line and tone while it's open; the list and summary say the same on the page.
  function report(text, kind) {
    if (camera && camera.isOpen()) {
      camera.status(text);
      if (kind) {
        camera.tone(kind);
      }
    }
  }

  async function errorText(response) {
    try {
      var text = await response.text();
      try {
        var parsed = JSON.parse(text);
        return typeof parsed === 'string' ? parsed : (parsed.title || text);
      } catch (e) {
        return text;
      }
    } catch (e) {
      return 'The server refused it.';
    }
  }

  // Removing a scan can change what other scans of the same code matched, so the list is loaded again after.
  async function removeEntry(entryId, button) {
    button.disabled = true;
    try {
      var response = await fetch(api + '/entries/' + entryId, { method: 'DELETE', credentials: 'same-origin' });
      if (!response.ok) {
        throw new Error(await errorText(response));
      }
      await load();
    } catch (e) {
      button.disabled = false;
      alert(e.message || 'That scan could not be removed.');
    }
  }

  async function load() {
    var response = await fetch(api, { credentials: 'same-origin' });
    if (!response.ok) {
      summary.textContent = 'The scan could not be loaded: ' + await errorText(response);
      return;
    }
    var session = await response.json();
    if (session.completedDate) {
      location.href = '/ui/library/' + libraryId + '/scans/' + sessionId + '/reconcile';
      return;
    }
    entries = session.entries || [];
    render();
  }

  form.addEventListener('submit', function (e) {
    e.preventDefault();
    scanned(input.value);
    input.value = '';
    input.focus();
  });

  if (cameraButton && window.LibraryScanner && window.LibraryScanner.available) {
    cameraButton.hidden = false;
    cameraButton.addEventListener('click', function () {
      window.LibraryScanner.open(function (code, handle) {
        camera = handle;
        handle.status('Saving ' + code + '…');
        scanned(code);
      }, { continuous: true });
      // The handle only arrives with the first code; until then there's nothing to report to.
      camera = null;
    });
  }

  window.addEventListener('online', send);
  load()
    .catch(function () { summary.textContent = 'The scan could not be loaded. Reload to try again; anything scanned meanwhile is kept.'; })
    .then(send);
})();
