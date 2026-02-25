/**
 * Church Website - Main site JavaScript
 * USE CASE: Smooth scroll, transparent nav on home page
 * LOCATION: Loaded on every page via _Layout.cshtml
 */
document.addEventListener('DOMContentLoaded', function () {
  // MOBILE MENU: Close collapse when a nav link is clicked (not when clicking dropdown toggles like About/Next Steps)
  const collapse = document.getElementById('navbarMain');
  if (collapse && window.matchMedia('(max-width: 991.98px)').matches) {
    collapse.querySelectorAll('.dropdown-item, .nav-link:not(.dropdown-toggle)').forEach(function (link) {
      link.addEventListener('click', function () {
        const instance = bootstrap.Collapse.getInstance(collapse);
        if (instance) instance.hide();
      });
    });
  }

  // SMOOTH SCROLL: Same-page anchor links (e.g. #three-cards-section)
  document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
    anchor.addEventListener('click', function (e) {
      const targetId = this.getAttribute('href');
      if (targetId === '#') return;  // Ignore plain # links
      const target = document.querySelector(targetId);
      if (target) {
        e.preventDefault();  // Stop default jump
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });  // Animate scroll
      }
    });
  });

  // SMOOTH SCROLL: When landing on a page with hash (e.g. /About#beliefs) - scroll smoothly to section
  const hash = window.location.hash;
  if (hash) {
    const target = document.querySelector(hash);
    if (target) {
      requestAnimationFrame(function () {
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      });
    }
  }

  // TRANSPARENT NAV: On home page only - nav is transparent when hero visible, solid when scrolled past
  const nav = document.getElementById('mainNav');
  const isHome = document.body.classList.contains('home-page');  // Set in _Layout when path is /
  const hero = document.querySelector('.hero-calvary');

  if (isHome && nav && hero) {
    function updateNav() {
      const heroBottom = hero.getBoundingClientRect().bottom;  // Distance from top of viewport to hero bottom
      if (heroBottom > 80) {
        // Hero still visible - make nav transparent (white text over hero)
        nav.classList.add('navbar-transparent');
        nav.classList.remove('bg-white');
      } else {
        // Scrolled past hero - solid white nav
        nav.classList.remove('navbar-transparent');
        nav.classList.add('bg-white');
      }
    }

    updateNav();  // Run once on load
    window.addEventListener('scroll', updateNav, { passive: true });  // Run on scroll (passive = better perf)
  } else if (nav) {
    // Non-home pages: always solid white nav
    nav.classList.add('bg-white');
  }

  // LOCATION PAGE: server-first location with browser-geolocation fallback.
  (function locationFallback() {
    const card = document.getElementById('location-status-card');
    if (!card) return;

    const dot = document.getElementById('location-status-dot');
    const text = document.getElementById('location-status-text');
    const meta = document.getElementById('location-status-meta');
    const refreshBtn = document.getElementById('location-refresh-btn');

    const serverSuccess = card.dataset.locationSuccess === 'true';
    const parseOptionalNumber = (value) => {
      if (typeof value !== 'string' || value.trim() === '') return null;
      const parsed = Number(value);
      return Number.isFinite(parsed) ? parsed : null;
    };
    const serverLat = parseOptionalNumber(card.dataset.locationLat);
    const serverLon = parseOptionalNumber(card.dataset.locationLon);
    const storageKey = 'church.currentLocation.cache.v2';
    const cacheTtlMs = 10 * 60 * 1000; // 10 minutes

    function publishResolvedLocation(payload) {
      document.dispatchEvent(new CustomEvent('church:location-resolved', { detail: payload }));
    }

    function setOnline(displayText, sourceLabel) {
      if (dot) {
        dot.classList.remove('dot-offline');
        dot.classList.add('dot-online');
      }
      if (text) text.textContent = displayText;
      if (meta) meta.textContent = `Source: ${sourceLabel}`;
    }

    function setOffline(message) {
      if (dot) {
        dot.classList.remove('dot-online');
        dot.classList.add('dot-offline');
      }
      if (text) text.textContent = message;
      if (meta) meta.textContent = 'Source: fallback failed';
    }

    function getFreshCachedLocation() {
      const raw = sessionStorage.getItem(storageKey);
      if (!raw) return null;

      try {
        const parsed = JSON.parse(raw);
        const hasRequiredFields = parsed && typeof parsed.displayText === 'string' && typeof parsed.timestamp === 'number';
        if (!hasRequiredFields) return null;
        const isFresh = Date.now() - parsed.timestamp <= cacheTtlMs;
        return isFresh ? parsed : null;
      } catch (_error) {
        return null;
      }
    }

    function saveCachedLocation(displayText, source, latitude, longitude, accuracy) {
      sessionStorage.setItem(storageKey, JSON.stringify({
        displayText,
        source,
        latitude,
        longitude,
        accuracy,
        timestamp: Date.now()
      }));
    }

    function getCurrentPositionAsync(options) {
      return new Promise((resolve, reject) => {
        navigator.geolocation.getCurrentPosition(resolve, reject, options);
      });
    }

    async function sampleBestPosition(maxAttempts) {
      let best = null;
      for (let i = 0; i < maxAttempts; i += 1) {
        try {
          const position = await getCurrentPositionAsync({
            enableHighAccuracy: true,
            timeout: 9000,
            maximumAge: 0
          });

          if (!best || position.coords.accuracy < best.coords.accuracy) {
            best = position;
          }

          // Stop early if very accurate.
          if (position.coords.accuracy <= 50) {
            break;
          }
        } catch (_error) {
          // Keep sampling attempts; fallback is handled after loop.
        }
      }

      return best;
    }

    async function reverseGeocodeDisplay(lat, lon) {
      const response = await fetch(
        `https://api.bigdatacloud.net/data/reverse-geocode-client?latitude=${encodeURIComponent(lat)}&longitude=${encodeURIComponent(lon)}&localityLanguage=en`,
        { method: 'GET' }
      );
      if (!response.ok) {
        return null;
      }

      const payload = await response.json();
      const city = payload.city || payload.locality || '';
      const region = payload.principalSubdivision || '';
      const countryCode = payload.countryCode || '';
      const countryName = payload.countryName || '';
      const country = countryCode === 'US' || /united states/i.test(countryName) ? 'USA' : countryName;
      const parts = [city, region, country].filter(Boolean);
      return parts.length > 0 ? parts.join(', ') : null;
    }

    async function resolveWithDeviceGps(forceLive) {
      // Keep fallback values handy, but still attempt live device geolocation first.
      const cached = forceLive ? null : getFreshCachedLocation();
      const serverDisplay = text && text.textContent ? text.textContent.trim() : '';

      if (cached) {
        setOnline(cached.displayText, 'session cache');
        publishResolvedLocation({
          source: 'session-cache',
          displayText: cached.displayText,
          latitude: cached.latitude,
          longitude: cached.longitude,
          accuracy: cached.accuracy
        });
        return;
      }

      if (!navigator.geolocation) {
        if (serverSuccess && serverDisplay) {
          setOnline(serverDisplay, 'server detection');
          publishResolvedLocation({
            source: 'server-ip',
            displayText: serverDisplay,
            latitude: serverLat,
            longitude: serverLon
          });
        } else {
          setOffline('Location unavailable');
        }
        return;
      }

      if (refreshBtn) {
        refreshBtn.disabled = true;
        refreshBtn.textContent = 'Refining location...';
      }

      try {
        const bestPosition = await sampleBestPosition(3);
        if (!bestPosition) {
          if (serverSuccess && serverDisplay) {
            setOnline(serverDisplay, 'server detection');
            publishResolvedLocation({
              source: 'server-ip',
              displayText: serverDisplay,
              latitude: serverLat,
              longitude: serverLon
            });
          } else {
            setOffline('Location unavailable');
          }
          return;
        }

        const displayText = await reverseGeocodeDisplay(
          bestPosition.coords.latitude,
          bestPosition.coords.longitude
        );

        if (displayText) {
          saveCachedLocation(
            displayText,
            'browser geolocation',
            bestPosition.coords.latitude,
            bestPosition.coords.longitude,
            bestPosition.coords.accuracy
          );
          setOnline(displayText, `browser geolocation (${Math.round(bestPosition.coords.accuracy)}m)`);
          publishResolvedLocation({
            source: 'browser-geolocation',
            displayText,
            latitude: bestPosition.coords.latitude,
            longitude: bestPosition.coords.longitude,
            accuracy: bestPosition.coords.accuracy
          });
          return;
        }

        if (serverSuccess && serverDisplay) {
          setOnline(serverDisplay, 'server detection');
          publishResolvedLocation({
            source: 'server-ip',
            displayText: serverDisplay,
            latitude: serverLat,
            longitude: serverLon
          });
        } else {
          setOffline('Location unavailable');
        }
      } catch (_error) {
        if (serverSuccess && serverDisplay) {
          setOnline(serverDisplay, 'server detection');
          publishResolvedLocation({
            source: 'server-ip',
            displayText: serverDisplay,
            latitude: serverLat,
            longitude: serverLon
          });
        } else {
          setOffline('Location unavailable');
        }
      } finally {
        if (refreshBtn) {
          refreshBtn.disabled = false;
          refreshBtn.textContent = 'Use device GPS';
        }
      }
    }

    // Automatic attempt on load.
    resolveWithDeviceGps(false);

    // Manual re-trigger button.
    if (refreshBtn) {
      refreshBtn.addEventListener('click', function () {
        resolveWithDeviceGps(true);
      });
    }
  })();
});
