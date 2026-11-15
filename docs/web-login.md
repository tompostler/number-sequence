# Web login

The `/ui` pages authenticate with the `ns-token` cookie, which `/ui/login` sets to the account's `web` token.
`TokenCookieMiddleware` turns the cookie into `HttpContext.User` on every request, and `RequiresTokenFilter` redirects an unauthenticated `/ui` request to the login page with a `returnUrl`.
The api controllers accept the same cookie, which is how page scripts such as the library scan page call the api with `fetch`.

## The cookie is `SameSite=Lax`

It used to be `Strict`, and that broke deep links opened from outside the browser.
A phone home screen shortcut, or a link tapped in another app, is a cross-site navigation, and `Strict` withholds the cookie on it.
So a signed-in user was redirected to the login page, which rendered an antiforgery token for an anonymous user.
Submitting it was a same-site post, so the cookie came along, the middleware made the user authenticated, and antiforgery rejected the post with a 400 ("meant for a different claims-based user than the current user").

`Lax` sends the cookie on a cross-site top-level `GET` navigation and nowhere else cross-site: not on posts, puts, deletes, `fetch` or iframes.
That is the property the api relies on, since its controllers have no antiforgery validation.
It holds only while no cookie-authenticated `GET` changes state.

- **Rejected: `[IgnoreAntiforgeryToken]` on the login page.**
  It would have stopped the 400, but the deep link would still have landed on the login page every time, for a user who was already signed in.

An existing `Strict` cookie stays `Strict` until the next login reissues it, so the fix needs one log out and log in to take effect on a device.
