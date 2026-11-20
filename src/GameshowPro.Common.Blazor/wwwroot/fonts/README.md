# GameshowPro house fonts

Plus Jakarta Sans (UI and prose) and Cascadia Code (data and code), self-hosted as WOFF2 so no web
font is ever fetched from a CDN. Every file in this directory is copied unmodified from the `fonts/`
directory of the Suede repository (`GameshowPro/Suede`); `sources.json` there and here records the
upstream download location and SHA-256 hash of each file. Both families are licensed under the SIL
Open Font License; the license texts sit next to the fonts and ship in the NuGet package.

Link `fonts.css` from the page (see the package README); its `@font-face` URLs are relative to the
CSS file, so it works under any path prefix.

`fonts-upright.css` declares only the two upright faces, for products served from a small device (the browser then never
requests the italic files, about 225 KB). Use `fonts.css` otherwise. `gsp-house.css` links `fonts.css` and
`gsp-house-upright.css` links `fonts-upright.css`, so most products never link either directly.
