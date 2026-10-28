// Camera barcode scanning for the library pages; see docs/library.md. Loaded by _LibraryScanner.cshtml, which passes the
// fingerprinted urls of the ponyfill and its .wasm on this script tag.
//
// A button with data-library-scan="<selector of a text input>" opens the camera. The first barcode read goes into that
// input and its form is submitted, so the page does exactly what it does when the code is typed. Buttons start hidden and
// are shown only where there is a camera api (https or localhost), so the text box is always the fallback. Adding
// data-library-scan-quiet skips the beep (a read still vibrates), for scanning in a shop.
(function () {
  'use strict';

  var script = document.currentScript;
  var ponyfillSrc = script.dataset.ponyfill;
  var wasmSrc = script.dataset.wasm;

  // What's printed on books, movies and games. EAN-13 covers ISBNs; the server normalizes all three.
  var formats = ['ean_13', 'upc_a', 'upc_e'];
  var detectorPromise = null;
  var audio = null;

  function loadScript(src) {
    return new Promise(function (resolve, reject) {
      var tag = document.createElement('script');
      tag.src = src;
      tag.onload = resolve;
      tag.onerror = function () { reject(new Error('Could not load the barcode reader.')); };
      document.head.appendChild(tag);
    });
  }

  // The browser's own BarcodeDetector where it has one with these formats (Android Chrome, macOS), else the zxing-wasm
  // ponyfill, which is only downloaded then. iOS Safari has no BarcodeDetector at all.
  function getDetector() {
    if (!detectorPromise) {
      detectorPromise = (async function () {
        if ('BarcodeDetector' in window) {
          try {
            var supported = await window.BarcodeDetector.getSupportedFormats();
            if (formats.every(function (f) { return supported.indexOf(f) >= 0; })) {
              return new window.BarcodeDetector({ formats: formats });
            }
          } catch (e) { }
        }
        await loadScript(ponyfillSrc);
        var api = window.BarcodeDetectionAPI;
        // Without this the ponyfill fetches its .wasm from jsdelivr.
        await api.prepareZXingModule({
          overrides: {
            locateFile: function (path, prefix) { return path.endsWith('.wasm') ? wasmSrc : prefix + path; },
          },
          fireImmediately: true,
        });
        return new api.BarcodeDetector({ formats: formats });
      })();
      // A failed load can be retried by tapping Scan again.
      detectorPromise.catch(function () { detectorPromise = null; });
    }
    return detectorPromise;
  }

  // Started from the tap that opened the scanner: iOS only lets audio play from a context created in a user gesture.
  function unlockAudio() {
    try {
      audio = audio || new (window.AudioContext || window.webkitAudioContext)();
      if (audio.state === 'suspended') {
        audio.resume();
      }
    } catch (e) { }
  }

  function feedback(quiet) {
    if (navigator.vibrate) {
      navigator.vibrate(60);
    }
    if (quiet) {
      return;
    }
    try {
      var osc = audio.createOscillator();
      var gain = audio.createGain();
      osc.frequency.value = 1760;
      gain.gain.value = 0.08;
      osc.connect(gain).connect(audio.destination);
      osc.start();
      osc.stop(audio.currentTime + 0.08);
    } catch (e) { }
  }

  function describe(error) {
    switch (error && error.name) {
      case 'NotAllowedError': return 'Camera access was refused. Allow it in the browser\'s settings for this site, or type the code.';
      case 'NotFoundError':
      case 'OverconstrainedError': return 'No camera found. Type the code instead.';
      case 'NotReadableError': return 'The camera is in use by something else.';
      default: return (error && error.message) || 'The camera could not be started.';
    }
  }

  // A full-screen camera view. Calls onCode with the first barcode read, after closing itself.
  function open(onCode, quiet) {
    if (!quiet) {
      unlockAudio();
    }

    var overlay = document.createElement('div');
    overlay.className = 'position-fixed top-0 start-0 w-100 h-100 bg-black d-flex flex-column';
    overlay.style.zIndex = '1090';
    overlay.innerHTML =
      '<div class="position-relative flex-grow-1 overflow-hidden">' +
      '  <video playsinline muted class="w-100 h-100" style="object-fit: cover"></video>' +
      '  <div class="position-absolute top-50 start-50 translate-middle border border-2 border-light rounded" style="width: 80%; max-width: 480px; aspect-ratio: 2 / 1; box-shadow: 0 0 0 100vmax rgba(0, 0, 0, .35)"></div>' +
      '</div>' +
      '<div class="d-flex align-items-center gap-2 p-3 text-white">' +
      '  <div class="flex-grow-1 small" data-status>Starting the camera…</div>' +
      '  <button type="button" class="btn btn-light">Cancel</button>' +
      '</div>';
    document.body.appendChild(overlay);

    var video = overlay.querySelector('video');
    var status = overlay.querySelector('[data-status]');
    var stream = null;
    var timer = null;
    var closed = false;

    function close() {
      if (closed) {
        return;
      }
      closed = true;
      clearTimeout(timer);
      if (stream) {
        stream.getTracks().forEach(function (t) { t.stop(); });
      }
      overlay.remove();
      document.removeEventListener('keydown', onKey);
      document.removeEventListener('visibilitychange', onHidden);
    }
    function onKey(e) {
      if (e.key === 'Escape') {
        close();
      }
    }
    // A locked phone or switched tab suspends the camera, and it doesn't always come back.
    function onHidden() {
      if (document.visibilityState === 'hidden') {
        close();
      }
    }
    overlay.querySelector('button').addEventListener('click', close);
    document.addEventListener('keydown', onKey);
    document.addEventListener('visibilitychange', onHidden);

    function fail(e) {
      if (!closed) {
        clearTimeout(timer);
        if (stream) {
          stream.getTracks().forEach(function (t) { t.stop(); });
        }
        status.textContent = describe(e);
      }
    }

    (async function () {
      try {
        var detectorTask = getDetector();
        stream = await navigator.mediaDevices.getUserMedia({
          audio: false,
          video: { facingMode: { ideal: 'environment' }, width: { ideal: 1280 }, height: { ideal: 720 } },
        });
        if (closed) {
          stream.getTracks().forEach(function (t) { t.stop(); });
          return;
        }
        video.srcObject = stream;
        await video.play();
        status.textContent = 'Loading the barcode reader…';
        var detector = await detectorTask;
        status.textContent = 'Hold the barcode inside the box.';

        // Each pass schedules the next, so a slow wasm read never piles up behind itself.
        var tick = async function () {
          try {
            if (closed) {
              return;
            }
            if (video.readyState >= 2) {
              var codes = await detector.detect(video);
              var code = codes.length > 0 ? codes[0].rawValue : null;
              if (code && !closed) {
                feedback(quiet);
                close();
                onCode(code);
                return;
              }
            }
            if (!closed) {
              timer = setTimeout(tick, 120);
            }
          } catch (e) {
            fail(e);
          }
        };
        tick();
      } catch (e) {
        fail(e);
      }
    })();
  }

  if (!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia)) {
    return;
  }
  document.querySelectorAll('[data-library-scan]').forEach(function (button) {
    var input = document.querySelector(button.getAttribute('data-library-scan'));
    if (!input || !input.form) {
      return;
    }
    button.hidden = false;
    var quiet = button.hasAttribute('data-library-scan-quiet');
    button.addEventListener('click', function () {
      open(function (code) {
        input.value = code;
        if (input.form.requestSubmit) {
          input.form.requestSubmit();
        } else {
          input.form.submit();
        }
      }, quiet);
    });
  });
})();
