# Static assets

Bootstrap and the favicon are served from `src/number-sequence/wwwroot` by `MapStaticAssets` in `Startup`, and
`_Layout.cshtml` references them with `~/` paths and `asp-append-version`. Bootstrap used to come from jsdelivr.

## Why not jsdelivr

The usual reason for a public CDN is that a visitor already has the file cached from another site. Browsers have
partitioned their http caches by top-level site since about 2020, so that hit never happens any more: a visitor
downloads jsdelivr's bootstrap once for ns.tcp.wtf either way. What the CDN still cost:

- A third party in the critical path. If jsdelivr has an outage or is blocked on a network, every page loses its
  styling and the navbar dropdowns stop working.
- An extra DNS lookup and TLS handshake to a second origin on first load.
- Script from a third party running on authenticated pages (chiro, ledger) with no `integrity` check.
- A `script-src` entry to carry if a Content-Security-Policy is ever added.

The domain is proxied through Cloudflare, so serving from the origin doesn't give up edge caching. Cloudflare caches
`.css`/`.js` by default and honours the origin's `Cache-Control`.

## How caching works

At build time, `MapStaticAssets` writes a manifest (`*.staticwebassets.endpoints.json`) with two routes for each
file:

- A fingerprinted route, such as `lib/bootstrap/css/bootstrap.min.<hash>.css`, sent with
  `Cache-Control: max-age=31536000, immutable`.
- The plain route, sent with `no-cache` and an ETag. Every use of the plain route costs a revalidation.

`MapRazorPages().WithStaticAssets()` gives the tag helpers that manifest. `asp-append-version="true"` on a `~/` path
then makes them emit the fingerprinted route. A changed file gets a new hash and therefore a new url, so neither
Cloudflare nor browsers ever need a purge. Publish also pre-compresses each file (gzip and brotli) and serves the
right variant by `Accept-Encoding`.

Locally none of this caching shows up. Both launch profiles set `ASPNETCORE_ENVIRONMENT=Development`. In
Development, `MapStaticAssets` (`StaticAssetDevelopmentRuntimeHandler`) replaces the caching headers with `no-cache`,
including on fingerprinted urls, so that edits under `wwwroot` show on the next reload. It also drops integrity in
Development. The `EnableStaticAssetsDevelopmentCaching` and `EnableStaticAssetsDevelopmentIntegrity` settings turn
those back on. The deployed app sets no environment, so it runs as Production and sends the manifest's headers. To
check caching, look at the response headers on ns.tcp.wtf (`cf-cache-status` shows whether Cloudflare's edge served
it), not on localhost.

## Favicon

`wwwroot/favicon.ico` replaced an embedded resource that a `RootLevelController` action returned with no cache
headers. Static-asset serving brings an ETag, compression and caching that the action never had. It is served two
ways:

- `_Layout.cshtml` links it with `<link rel="icon" href="~/favicon.ico" asp-append-version="true" />`, so UI pages
  get the fingerprinted, immutable url.
- Browsers and other clients still ask for `/favicon.ico` unprompted, for example on api responses. The static-asset
  SDK gives the plain `favicon.ico` route `max-age=3600, must-revalidate` rather than the `no-cache` other plain
  routes get, so those requests are cached for an hour as well.

`WEBSITE_RUN_FROM_PACKAGE` doesn't interfere. `wwwroot` is part of `dotnet publish` output, so it ships inside the
mounted zip.

## Updating bootstrap

Copy the files out of the npm tarball and check it against the registry's integrity hash. Using the tarball rather
than the GitHub release means the files are byte-for-byte what jsdelivr served.

```sh
meta=$(curl -s https://registry.npmjs.org/bootstrap/<version>)
# dist.tarball and dist.integrity (sha512) are in $meta
curl -sL <tarball> -o bootstrap.tgz
echo "sha512-$(openssl dgst -sha512 -binary bootstrap.tgz | base64 -w0)"   # must equal dist.integrity
tar xzf bootstrap.tgz
cp package/dist/css/bootstrap.min.css{,.map}       src/number-sequence/wwwroot/lib/bootstrap/css/
cp package/dist/js/bootstrap.bundle.min.js{,.map}  src/number-sequence/wwwroot/lib/bootstrap/js/
cp package/LICENSE                                 src/number-sequence/wwwroot/lib/bootstrap/
```

Keep the file names and layout as they are, so `_Layout.cshtml` doesn't change. The `.map` files are vendored
because the minified files end in a `sourceMappingURL` comment, and devtools would otherwise log a 404 for each.
Bootstrap is MIT licensed, and `LICENSE` sits next to the files to satisfy the notice requirement.

The vendored version is 5.3.8. The only newer release is 6.0.0-alpha.1 (2026-10-08). Version 6 is a major break, so
don't adopt it until it is stable and the pages have been checked against it.

libman and an npm build step were considered and not used. Two files that change once or twice a year don't
justify another tool in the build.
