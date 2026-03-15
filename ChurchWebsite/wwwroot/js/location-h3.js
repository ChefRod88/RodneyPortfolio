/**
 * H3 Geospatial Index overlay — Uber open-source technology.
 *
 * Renders hexagonal grid cells on the Leaflet map:
 *   - Animated ring-search expanding outward from the church destination
 *   - Church hex cell highlighted in gold (ring 0)
 *   - Proximity rings 1–3 fading outward
 *   - User's current hex cell in blue
 *   - Ring distance + cell IDs displayed in the HUD panel
 *
 * Resolution 8 ≈ 0.74 km² per cell — street-block granularity.
 *
 * Requires: h3-js (window.h3), Leaflet (window.L)
 * Listens for: church:map-ready, church:location-resolved
 */
(function H3OverlayFeature() {
  if (typeof h3 === 'undefined' || typeof L === 'undefined') return;

  const RESOLUTION  = 8;
  const CHURCH_LAT  = 28.0497068;
  const CHURCH_LON  = -81.7260765;
  const CHURCH_CELL = h3.latLngToCell(CHURCH_LAT, CHURCH_LON, RESOLUTION);
  const MAX_RINGS   = 3;

  /* Ring styles — gold fading outward, matching existing theme */
  const RING_STYLES = [
    { color: 'rgba(201,168,76,0.85)',  fillColor: 'rgba(201,168,76,0.20)', weight: 2   }, // r=0 church cell
    { color: 'rgba(201,168,76,0.50)',  fillColor: 'rgba(201,168,76,0.09)', weight: 1.5 }, // r=1
    { color: 'rgba(201,168,76,0.28)',  fillColor: 'rgba(201,168,76,0.04)', weight: 1   }, // r=2
    { color: 'rgba(201,168,76,0.13)',  fillColor: 'rgba(201,168,76,0.02)', weight: 1   }, // r=3
  ];
  const USER_STYLE = {
    color: 'rgba(59,130,246,0.80)', fillColor: 'rgba(59,130,246,0.15)', weight: 2
  };

  let mapInstance  = null;
  let ringLayers   = [];
  let userHexLayer = null;
  let animTimer    = null;
  let initialized  = false;

  /* ── Helpers ─────────────────────────────────────────────── */

  function makeHexPolygon(cell, style) {
    return L.polygon(h3.cellToBoundary(cell), {
      color:       style.color,
      weight:      style.weight,
      fillColor:   style.fillColor,
      fillOpacity: 1,
      opacity:     1,
      interactive: false,
      className:   'h3-hex-cell'
    });
  }

  function setEl(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = val;
  }

  /* ── Ring-search animation ─────────────────────────────── */

  function clearRings() {
    if (animTimer) { clearTimeout(animTimer); animTimer = null; }
    ringLayers.forEach(l => { try { mapInstance.removeLayer(l); } catch (_) {} });
    ringLayers = [];
  }

  function addRing(k) {
    const cells  = h3.gridRing(CHURCH_CELL, k);   // gridRing(origin, 0) returns [origin]
    const style  = RING_STYLES[Math.min(k, RING_STYLES.length - 1)];
    const reduced = window.matchMedia('(prefers-reduced-motion:reduce)').matches;

    cells.forEach(cell => {
      const layer = makeHexPolygon(cell, style).addTo(mapInstance);
      if (!reduced && layer._path) {
        layer._path.style.opacity   = '0';
        layer._path.style.transition = 'opacity 0.45s ease';
        /* Kick the transition on next paint */
        requestAnimationFrame(() => {
          if (layer._path) layer._path.style.opacity = '1';
        });
      }
      ringLayers.push(layer);
    });
  }

  function startRingAnimation() {
    if (!mapInstance) return;
    clearRings();

    const reduced = window.matchMedia('(prefers-reduced-motion:reduce)').matches;

    if (reduced) {
      for (let k = 0; k <= MAX_RINGS; k++) addRing(k);
      return;
    }

    let k = 0;
    function step() {
      if (k > MAX_RINGS) return;
      addRing(k++);
      if (k <= MAX_RINGS) animTimer = setTimeout(step, 320);
    }
    step();
  }

  /* ── User hex ──────────────────────────────────────────── */

  function updateUserHex(lat, lon) {
    if (!mapInstance) return;
    const cell = h3.latLngToCell(lat, lon, RESOLUTION);
    if (userHexLayer) { try { mapInstance.removeLayer(userHexLayer); } catch (_) {} }
    userHexLayer = makeHexPolygon(cell, USER_STYLE).addTo(mapInstance);
    return cell;
  }

  /* ── HUD update ────────────────────────────────────────── */

  function updateHUD(userLat, userLon) {
    const userCell = h3.latLngToCell(userLat, userLon, RESOLUTION);
    let dist = null;
    try { dist = h3.gridDistance(userCell, CHURCH_CELL); } catch (_) {}

    setEl('h3-user-cell',   userCell.slice(0, 13) + '\u2026');
    setEl('h3-church-cell', CHURCH_CELL.slice(0, 13) + '\u2026');
    setEl('h3-ring-dist',   dist !== null ? `${dist} ring${dist !== 1 ? 's' : ''}` : '\u2014');
  }

  /* ── Init ──────────────────────────────────────────────── */

  function init(map) {
    mapInstance = map;
    initialized = true;
    setEl('h3-church-cell', CHURCH_CELL.slice(0, 13) + '\u2026');
    startRingAnimation();
  }

  /* ── Public API (called by location-route.js) ─────────── */
  window.LocationH3 = {
    updateUser(lat, lon) {
      updateUserHex(lat, lon);
      updateHUD(lat, lon);
    }
  };

  /* Receive the Leaflet map instance once it's ready */
  document.addEventListener('church:map-ready', e => {
    const map = e?.detail?.map;
    if (map && !initialized) init(map);
  });

  /* Also respond to location-resolved events (server IP / session cache path) */
  document.addEventListener('church:location-resolved', e => {
    const { latitude: lat, longitude: lon } = e?.detail || {};
    if (typeof lat === 'number' && typeof lon === 'number' && mapInstance) {
      updateUserHex(lat, lon);
      updateHUD(lat, lon);
    }
  });

})();
