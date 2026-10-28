# Static assets

Bootstrap, the favicon and the library's barcode scanner are served from `src/number-sequence/wwwroot` by
`MapStaticAssets` in `Startup`, and pages reference them with `~/` paths and `asp-append-version`. Bootstrap used to
come from jsdelivr.

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

## Files fetched by script

A tag helper only fingerprints the url on its own tag. A file that script fetches by itself, such as the scanner's
`.wasm`, gets its url from `Utilities/StaticAssetUrls.For`, which reads the same manifest from the page's endpoint
metadata (`ResourceAssetCollection`, attached by `WithStaticAssets()`). The page passes that url to the script, here as a
`data-` attribute on the script tag in `_LibraryScanner.cshtml`. A hard-coded `/lib/...` url in script works but gets
the `no-cache` plain route.

## Updating the barcode scanner

`lib/barcode-detector/ponyfill.js` is the `dist/iife/ponyfill.js` bundle of the `barcode-detector` npm package, and
`lib/zxing-wasm/zxing_reader.wasm` is `dist/reader/zxing_reader.wasm` of `zxing-wasm`. Update them together, taking
zxing-wasm at exactly the version barcode-detector depends on: the bundle has the zxing-wasm glue code built in, and
glue and `.wasm` from different versions fail at runtime, not at build.

```sh
bd=<barcode-detector version>
zx=$(curl -s https://registry.npmjs.org/barcode-detector/$bd | jq -r '.dependencies["zxing-wasm"]')   # exact, e.g. 3.1.6
for p in barcode-detector@$bd zxing-wasm@$zx; do
  meta=$(curl -s https://registry.npmjs.org/${p%@*}/${p#*@})
  curl -sL $(echo "$meta" | jq -r .dist.tarball) -o ${p%@*}.tgz
  echo "$(echo "$meta" | jq -r .dist.integrity)"                                      # these two lines
  echo "sha512-$(openssl dgst -sha512 -binary ${p%@*}.tgz | base64 -w0)"               # must be equal
  mkdir -p ${p%@*} && tar xzf ${p%@*}.tgz -C ${p%@*}
done

w=src/number-sequence/wwwroot/lib
cp barcode-detector/package/dist/iife/ponyfill.js barcode-detector/package/LICENSE  $w/barcode-detector/
cp zxing-wasm/package/dist/reader/zxing_reader.wasm zxing-wasm/package/LICENSE      $w/zxing-wasm/

# The bundle names the .wasm it expects and the zxing-cpp commit it was built from.
grep -o 'ZXING_WASM_SHA256=[a-z]*' $w/barcode-detector/ponyfill.js   # gives the variable holding it, e.g. pe
grep -o 'pe=`[0-9a-f]*`' $w/barcode-detector/ponyfill.js             # must equal the next line
sha256sum $w/zxing-wasm/zxing_reader.wasm
grep -o 'ZXING_CPP_COMMIT=[a-z]*' $w/barcode-detector/ponyfill.js    # e.g. re
grep -o 're=`[0-9a-f]*`' $w/barcode-detector/ponyfill.js             # the commit
curl -sfL https://raw.githubusercontent.com/zxing-cpp/zxing-cpp/<commit>/LICENSE -o $w/zxing-wasm/LICENSE-zxing-cpp
```

The minified variable names (`pe`, `re` above) change between releases, hence the first grep of each pair. Keep the
file names as they are, so `_LibraryScanner.cshtml` doesn't change. The `.wasm` url is also in the bundle as a
jsdelivr default, but it is overridden by `locateFile` in `library-scanner.js`; confirm in devtools after updating that
the `.wasm` still loads from this site, on a browser without a native `BarcodeDetector` (Firefox, or desktop Chrome on
Windows or Linux).

Licenses: both packages are MIT (`LICENSE` in each folder). The `.wasm` is zxing-cpp, which is Apache-2.0, so its license
is vendored as `lib/zxing-wasm/LICENSE-zxing-cpp`, from the commit the bundle names.

The vendored versions are barcode-detector 3.2.3 and zxing-wasm 3.1.6, with zxing-cpp at `2c3dcfef`.

libman and an npm build step were considered and not used. Two files that change once or twice a year don't
justify another tool in the build.
