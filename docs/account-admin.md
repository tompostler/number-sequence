# Account administration

Tier and role changes used to be direct sql against `Accounts`. They now go through admin-only endpoints on
`AccountsController` (`GET accounts`, `PUT accounts/{name}/tier/{tier}`, `PUT`/`DELETE accounts/{name}/roles/{role}`).
The client wraps them, `ns account list|tier|role add|role remove` in the CLI calls them, and the
`/ui/admin/accounts` page calls them as well. All of them require the `Admin` role.

## Admin is a role, not a flag

`Accounts.Roles` already becomes the principal's roles in `TokenValidationService`, and `RequiresToken(...)` already
gates on them. Admin is one more value in that string, which meant no migration, no `IsAdmin` column, and no second
authorization check next to `RequiresToken`. A flag would have bought nothing a role doesn't already do.

Bootstrapping still needs one manual sql update, to give the first account `Admin`. After that, everything goes
through the api.

## Roles are validated against the constants

`AccountRoles.All` comes from reflection over the `const string` fields on `AccountRoles`, so adding a role constant is
the only step needed for it to become grantable and show up in the UI. There is deliberately no hand-kept list.

- **Granting** only accepts a defined role (case-insensitive) and stores it in the constant's casing. Before this, a
  typo in the sql went in silently and the feature just stayed hidden.
- **Removing** accepts any string, so values written by hand before validation existed can still be cleaned up. The UI
  shows those stale values struck through, so they can be unchecked.
- The stored string is rewritten sorted, de-duplicated, and `;`-joined on every change, and set to null when empty.
  `AccountRoles.Parse` is the one reader of that format, used by both the controller and the page.

An admin cannot remove `Admin` from their own account; the api refuses it. With a single admin, doing so would mean
going back to sql to undo it. The UI disables that checkbox on the viewer's own row and posts a hidden input in its
place, because a disabled checkbox doesn't post and the diff would otherwise read it as a removal.

## Changes take effect on the next request

`TokenValidationService` caches the principal (roles included) in `IMemoryCache`, keyed by the raw token string. The
cache has a 5-minute sliding expiration, but a token in active use never goes idle that long, so without help a role
change wouldn't reach that token until it expired. After saving, the update looks up the account's `Tokens.Value`s and
evicts each one from the cache.

The eviction only reaches the cache on the instance that served the admin request. If the plan is ever scaled out to
several instances, the others keep serving stale roles until the token sits idle for 5 minutes or expires.

Tier limits are read from `Accounts.Tier` at enforcement time, so a tier change applies to the next create right away.
The token's own `AccountTier` is a copy taken when the token was minted. It is baked into the signed token string, so it
can't be updated, and it only affects cache eviction priority. Existing tokens keep the old priority until they're
replaced. That is harmless.

## UI shape

The api changes one thing per call. The page posts the whole desired state of a row (tier select plus role
checkboxes, joined to a per-row `<form>` through the html `form` attribute, since a `<form>` can't wrap a `<tr>`). The
handler diffs that state against the current account and sends only what changed: tier first, then adds, then removes.
If a call fails partway, the changes made before it stay, and the error is shown. Saving the row again finishes the
rest.
