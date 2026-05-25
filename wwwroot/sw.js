// Service Worker for Rodney Portfolio PWA
// Network-first for CSS/JS (fingerprinted URLs change on deploy).
// Only caches the offline shell (/) — not unversioned /css/site.css paths.

const CACHE_NAME = 'rodney-portfolio-v2';

// Fingerprinted static assets (asp-append-version / MapStaticAssets) — do not intercept.
function isFingerprintedAsset(pathname) {
  return /\.[a-z0-9]{8,}\.(css|js|svg|png|jpg|webp)$/i.test(pathname);
}

// Install: cache offline shell only (avoid stale unversioned CSS/JS)
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.add('/')).then(() => self.skipWaiting())
  );
});

// Activate: remove old caches (including rodney-portfolio-v1)
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((cacheNames) =>
      Promise.all(
        cacheNames.filter((name) => name !== CACHE_NAME).map((name) => caches.delete(name))
      )
    ).then(() => self.clients.claim())
  );
});

// Fetch: network-first for HTML and static assets; fingerprinted assets pass through
self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  if (url.origin !== location.origin) return;

  // Let the browser handle fingerprinted bundles (immutable cache headers from server)
  if (isFingerprintedAsset(url.pathname)) return;

  // HTML: network first, fall back to cached shell
  if (request.mode === 'navigate' || request.headers.get('accept')?.includes('text/html')) {
    event.respondWith(
      fetch(request)
        .then((response) => {
          if (response.ok) {
            const clone = response.clone();
            caches.open(CACHE_NAME).then((cache) => cache.put(request, clone));
          }
          return response;
        })
        .catch(() => caches.match(request).then((cached) => cached || caches.match('/')))
    );
    return;
  }

  // CSS, JS, images, fonts (unversioned paths only): network first, then cache
  if (
    request.destination === 'style' ||
    request.destination === 'script' ||
    request.destination === 'image' ||
    request.destination === 'font'
  ) {
    event.respondWith(
      fetch(request)
        .then((response) => {
          if (response.ok) {
            const clone = response.clone();
            caches.open(CACHE_NAME).then((cache) => cache.put(request, clone));
          }
          return response;
        })
        .catch(() => caches.match(request))
    );
  }
});
