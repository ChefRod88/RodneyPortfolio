/**
 * Location route — Google Maps quality dark experience.
 * CartoDB Dark Matter tiles · GraphHopper routing · Uber-style live tracking
 */
(function locationRouteFeature() {
  const card    = document.getElementById('location-status-card');
  const mapEl   = document.getElementById('location-map');
  if (!card || !mapEl || typeof L === 'undefined') return;

  const panelMeta  = document.getElementById('location-directions-meta');
  const panelSteps = document.getElementById('location-directions-steps');
  const dirPanel   = document.getElementById('location-directions-panel');
  const statusText = document.getElementById('location-status-text');
  const statusDot  = document.getElementById('location-status-dot');
  const helpers    = window.LocationRouteHelpers || null;

  /* ── Constants ───────────────────────────────────────────── */
  const DEST = {
    name:    card.dataset.destinationName    || 'New Bethel Missionary Baptist Church',
    address: card.dataset.destinationAddress || '123 Ave Y NE, Winter Haven, FL 33881',
    lat:     28.0497068,
    lon:    -81.7260765
  };
  const GH_KEY = card.dataset.graphhopperKey || '';

  /* ── Parse server-provided coords ───────────────────────── */
  const parseNum = v => { const n = Number(v); return Number.isFinite(n) && v ? n : null; };
  let serverLat = parseNum(card.dataset.locationLat);
  let serverLon = parseNum(card.dataset.locationLon);

  /* ═══════════════════════════════════════════════════════════
     MAP INIT — CartoDB Dark Matter (Google Maps dark quality)
     ═══════════════════════════════════════════════════════════ */
  const map = L.map(mapEl, {
    zoomControl: false,
    attributionControl: true,
    zoomSnap: 0.5,
    zoomDelta: 0.5
  }).setView([DEST.lat, DEST.lon], 13);

  /* CartoDB Dark Matter — professional dark basemap, free, no key */
  L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/attributions">CARTO</a>',
    subdomains: 'abcd',
    maxZoom: 20
  }).addTo(map);

  /* Zoom controls — bottom right */
  L.control.zoom({ position: 'bottomright' }).addTo(map);

  /* Recenter button */
  const RecenterControl = L.Control.extend({
    options: { position: 'bottomright' },
    onAdd() {
      const btn = L.DomUtil.create('button', 'loc-recenter-btn');
      btn.innerHTML = '&#8982;';
      btn.title = 'Re-center map';
      btn.setAttribute('aria-label', 'Re-center map on route');
      L.DomEvent.on(btn, 'click', L.DomEvent.stopPropagation);
      L.DomEvent.on(btn, 'click', () => {
        if (lastRouteCoords && lastRouteCoords.length) {
          map.fitBounds(L.latLngBounds(lastRouteCoords), getFitPadding());
        } else {
          map.setView([DEST.lat, DEST.lon], 14);
        }
      });
      return btn;
    }
  });
  new RecenterControl().addTo(map);

  /* Notify H3 overlay that the map is ready */
  document.dispatchEvent(new CustomEvent('church:map-ready', { detail: { map } }));

  /* ── Church destination marker (gold pulsing pin) ─────── */
  const churchIcon = L.divIcon({
    className: '',
    html: `<div class="loc-church-pin" aria-hidden="true">
             <div class="loc-church-pin-inner">&#9679;</div>
             <div class="loc-church-pin-ring loc-church-pin-ring-1"></div>
             <div class="loc-church-pin-ring loc-church-pin-ring-2"></div>
           </div>`,
    iconSize:   [28, 28],
    iconAnchor: [14, 14],
    popupAnchor:[0, -16]
  });
  const churchMarker = L.marker([DEST.lat, DEST.lon], { icon: churchIcon, zIndexOffset: 1000 })
    .addTo(map)
    .bindPopup(`
      <div style="text-align:center;padding:.2rem 0">
        <strong style="font-size:.95rem;color:#F0D080">${DEST.name}</strong>
        <br><span style="font-size:.78rem;color:rgba(240,237,230,.6)">${DEST.address}</span>
      </div>`);

  /* ── State ─────────────────────────────────────────────── */
  let routeLayer         = null;
  let routeGlowLayer     = null;
  let userMarker         = null;
  let lastRouteCoords    = null;
  let lastRenderedOrigin = null;
  let inflight           = false;
  let queued             = false;
  let queuedCoords       = null;
  let lastRequestedAt    = 0;
  let activeSeq          = 0;
  let watchId            = null;
  let initDone           = false;

  /* ── Helpers ─────────────────────────────────────────── */
  const isSame = (a, b) => a && b &&
    Math.abs(a.lat - b.lat) < 0.00008 &&
    Math.abs(a.lon - b.lon) < 0.00008;

  const updateMeta  = msg => { if (panelMeta)  panelMeta.textContent  = msg; };
  const clearSteps  = ()  => { if (panelSteps) panelSteps.innerHTML   = ''; };

  function setStatus(online, text) {
    if (statusDot) {
      statusDot.className = `location-status-dot ${online ? 'dot-online' : 'dot-offline'}`;
    }
    if (statusText) statusText.textContent = text;
  }

  function setSteps(steps) {
    if (!panelSteps) return;
    panelSteps.innerHTML = '';
    if (!steps.length) {
      const li = document.createElement('li');
      li.textContent = 'No turn-by-turn directions returned.';
      panelSteps.appendChild(li);
      return;
    }
    steps.forEach(s => {
      const li = document.createElement('li');
      li.textContent = s;
      panelSteps.appendChild(li);
    });
  }

  /* ── Polyline decode (GraphHopper encoded) ───────────── */
  function decodePolyline(encoded) {
    const pts = []; let idx = 0, lat = 0, lng = 0;
    while (idx < encoded.length) {
      let s = 0, r = 0, b;
      do { b = encoded.charCodeAt(idx++) - 63; r |= (b & 0x1f) << s; s += 5; } while (b >= 0x20);
      lat += (r & 1) ? ~(r >> 1) : (r >> 1);
      s = 0; r = 0;
      do { b = encoded.charCodeAt(idx++) - 63; r |= (b & 0x1f) << s; s += 5; } while (b >= 0x20);
      lng += (r & 1) ? ~(r >> 1) : (r >> 1);
      pts.push([lat / 1e5, lng / 1e5]);
    }
    return pts;
  }

  /* ── Formatters ─────────────────────────────────────── */
  const fmtDist = m => {
    if (!Number.isFinite(m)) return '';
    const mi = m / 1609.34;
    return mi >= 0.1 ? `${mi.toFixed(1)} mi` : `${Math.round(m * 3.28084)} ft`;
  };
  const fmtTime = s => {
    if (!Number.isFinite(s)) return '';
    const m = Math.round(s / 60);
    if (m < 60) return `${m} min`;
    const h = Math.floor(m / 60), r = m % 60;
    return r ? `${h} hr ${r} min` : `${h} hr`;
  };

  /* ── Fit bounds padding (account for side panel) ────── */
  function getFitPadding() {
    const pad = 40;
    const pr = dirPanel ? dirPanel.getBoundingClientRect() : null;
    if (pr && pr.width > 0 && window.innerWidth > 900) {
      return { paddingTopLeft: [pad, pad], paddingBottomRight: [pr.width + 32, pad] };
    }
    return { paddingTopLeft: [pad, pad], paddingBottomRight: [pad, pad] };
  }

  /* ── User location marker (pulsing green dot) ─────── */
  function upsertUserMarker(lat, lon) {
    const icon = L.divIcon({
      className: '',
      html: '<span class="route-origin-ping-core" aria-hidden="true"></span>',
      iconSize: [18, 18], iconAnchor: [9, 9]
    });
    if (!userMarker) {
      userMarker = L.marker([lat, lon], { icon, zIndexOffset: 800 }).addTo(map);
    } else {
      userMarker.setLatLng([lat, lon]);
    }
  }

/* ── Route line draw-on animation ──────────────────── */
  function animateRouteLine() {
    if (!routeLayer || window.matchMedia('(prefers-reduced-motion:reduce)').matches) return;
    const path = routeLayer._path;
    if (!path || typeof path.getTotalLength !== 'function') return;
    const len = path.getTotalLength();
    path.style.strokeDasharray  = len;
    path.style.strokeDashoffset = len;
    path.style.transition       = 'none';
    requestAnimationFrame(() => {
      path.style.transition       = 'stroke-dashoffset 1.6s cubic-bezier(.4,0,.2,1)';
      path.style.strokeDashoffset = '0';
    });
    const cleanup = () => {
      path.style.strokeDasharray = '';
      path.style.strokeDashoffset = '';
      path.style.transition = '';
      path.removeEventListener('transitionend', cleanup);
    };
    path.addEventListener('transitionend', cleanup);
    setTimeout(cleanup, 2200);
  }

  /* ── Fetch route from GraphHopper ─────────────────── */
  async function fetchRoute(oLat, oLon) {
    if (!GH_KEY) return null;
    const qp = new URLSearchParams({
      key: GH_KEY, vehicle: 'car', locale: 'en',
      points_encoded: 'true', instructions: 'true', calc_points: 'true'
    });
    qp.append('point', `${oLat},${oLon}`);
    qp.append('point', `${DEST.lat},${DEST.lon}`);
    const res = await fetch(`https://graphhopper.com/api/1/route?${qp}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();
    if (!data.paths?.length) throw new Error('No route found');
    return data.paths[0];
  }

  /* ── Main render ─────────────────────────────────── */
  async function renderRoute(oLat, oLon) {
    const origin = { lat: oLat, lon: oLon };
    if (routeLayer && isSame(lastRenderedOrigin, origin)) return;

    const now = Date.now();
    if (inflight || now - lastRequestedAt < 800) {
      if (inflight && isSame(inflight === origin, origin)) return;
      queued = true;
      queuedCoords = origin;
      return;
    }

    inflight = true;
    lastRequestedAt = now;
    activeSeq++;
    const seq = activeSeq;

    try {
      updateMeta('Calculating fastest route…');
      upsertUserMarker(oLat, oLon);
      if (window.LocationH3) window.LocationH3.updateUser(oLat, oLon);

      /* Try GraphHopper; fall back to straight line if key missing */
      let coords;
      if (GH_KEY) {
        const path = await fetchRoute(oLat, oLon);
        if (!path || seq !== activeSeq) return;
        coords = decodePolyline(path.points);
        if (!coords.length || seq !== activeSeq) return;

        /* Glow layer (thick blurred copy behind the main line) */
        if (routeGlowLayer) map.removeLayer(routeGlowLayer);
        routeGlowLayer = L.polyline(coords, {
          color: 'rgba(37,99,235,0.25)', weight: 14, opacity: 1, lineCap: 'round'
        }).addTo(map);

        /* Main route line */
        if (routeLayer) map.removeLayer(routeLayer);
        routeLayer = L.polyline(coords, {
          color: '#3b82f6', weight: 5, opacity: 0.95, lineCap: 'round', lineJoin: 'round'
        }).addTo(map);
        routeLayer.bringToFront();

        /* ETA + steps */
        const eta  = fmtTime(path.time / 1000);
        const dist = fmtDist(path.distance);
        updateMeta(eta && dist ? `${eta} • ${dist}` : 'Route calculated');
        const steps = (path.instructions || [])
          .map(i => { const t = (i.text||'').trim(); const d = fmtDist(i.distance); return t && d ? `${t}  (${d})` : t || d; })
          .filter(Boolean).slice(0, 14);
        setSteps(steps);
      } else {
        /* No API key — draw straight fallback line */
        coords = [[oLat, oLon], [DEST.lat, DEST.lon]];
        if (routeLayer) map.removeLayer(routeLayer);
        routeLayer = L.polyline(coords, {
          color: '#3b82f6', weight: 4, opacity: 0.7, dashArray: '8 6'
        }).addTo(map);
        updateMeta('Straight-line preview (routing key not configured)');
        clearSteps();
      }

      lastRouteCoords    = coords;
      lastRenderedOrigin = origin;

      /* Fit map to show full route — fire animations exactly once */
      const bounds = L.latLngBounds(coords);
      let animFired = false;
      const fireAnim = () => {
        if (animFired) return;
        animFired = true;
        animateRouteLine();
      };
      map.once('moveend', fireAnim);
      map.fitBounds(bounds, getFitPadding());
      /* Safety net: if fitBounds produces no moveend (already in view), fire after 500ms */
      setTimeout(fireAnim, 500);

      initDone = true;

    } catch {
      updateMeta('Could not calculate route. Please try again.');
      clearSteps();
    } finally {
      inflight = false;
      if (queued) {
        queued = false;
        const next = queuedCoords ? { ...queuedCoords } : null;
        queuedCoords = null;
        setTimeout(() => next && renderRoute(next.lat, next.lon), 300);
      }
    }
  }

  /* ═══════════════════════════════════════════════════
     LIVE GPS — auto-start on page load like Uber
     ═══════════════════════════════════════════════════ */
  function startGPS() {
    if (!navigator.geolocation) {
      tryServerCoords();
      return;
    }

    const refreshBtn = document.getElementById('location-refresh-btn');
    if (refreshBtn) { refreshBtn.disabled = true; refreshBtn.textContent = '📡 Locating…'; }

    navigator.geolocation.getCurrentPosition(
      pos => {
        const { latitude: lat, longitude: lon, accuracy } = pos.coords;
        setStatus(true, `GPS lock (±${Math.round(accuracy)}m)`);
        if (refreshBtn) { refreshBtn.disabled = false; refreshBtn.textContent = '🔄 Recalculate'; }
        renderRoute(lat, lon);
        sendLocationOverWs(lat, lon, accuracy);

        /* Live watch — re-route if user moves >30 m (Uber-style) */
        if (watchId !== null) navigator.geolocation.clearWatch(watchId);
        watchId = navigator.geolocation.watchPosition(
          wp => {
            const wLat = wp.coords.latitude, wLon = wp.coords.longitude;
            const d = Math.hypot(wLat - lat, wLon - lon) * 111320;
            if (d > 30) {
              renderRoute(wLat, wLon);
              setStatus(true, `Live tracking (±${Math.round(wp.coords.accuracy)}m)`);
              sendLocationOverWs(wLat, wLon, wp.coords.accuracy);
            }
          },
          null,
          { enableHighAccuracy: true, maximumAge: 10000, timeout: 30000 }
        );
      },
      err => {
        if (refreshBtn) { refreshBtn.disabled = false; refreshBtn.textContent = '📍 Use Device GPS'; }
        const msg = err.code === 1 ? 'GPS permission denied' :
                    err.code === 2 ? 'GPS unavailable' : 'GPS timeout';
        setStatus(false, msg);
        tryServerCoords();
      },
      { enableHighAccuracy: true, timeout: 12000, maximumAge: 30000 }
    );
  }

  function tryServerCoords() {
    if (Number.isFinite(serverLat) && Number.isFinite(serverLon)) {
      setStatus(true, 'Location from server IP');
      renderRoute(serverLat, serverLon);
    } else {
      updateMeta('Could not determine your location. Open in Google Maps for directions.');
      map.setView([DEST.lat, DEST.lon], 15);
      churchMarker.openPopup();
    }
  }

  /* Manual refresh button */
  const refreshBtn = document.getElementById('location-refresh-btn');
  if (refreshBtn) {
    refreshBtn.addEventListener('click', () => {
      if (watchId !== null) { navigator.geolocation.clearWatch(watchId); watchId = null; }
      startGPS();
    });
  }

  /* External location-resolved event (server push) */
  document.addEventListener('church:location-resolved', e => {
    const { latitude: lat, longitude: lon } = e?.detail || {};
    if (typeof lat === 'number' && typeof lon === 'number') {
      serverLat = lat; serverLon = lon;
      if (!initDone) renderRoute(lat, lon);
    }
  });

  /* ═══════════════════════════════════════════════════
     SIGNALR — Uber-style two-way WebSocket channel.
     Streams GPS position to server; server echoes back
     a timestamp so we can display round-trip latency.
     ═══════════════════════════════════════════════════ */
  let wsConn = null;

  function updateWsIndicator(connected, serverTs) {
    const dot   = document.getElementById('h3-ws-dot');
    const label = document.getElementById('h3-ws-label');
    if (dot) {
      dot.classList.toggle('dot-online',  connected);
      dot.classList.toggle('dot-offline', !connected);
    }
    if (label) {
      if (connected && serverTs) {
        const latency = Math.max(0, Date.now() - serverTs);
        label.textContent = `WS \u00b7 ${latency}ms`;
      } else {
        label.textContent = connected ? 'WS \u00b7 connected' : 'WS \u00b7 offline';
      }
    }
  }

  function sendLocationOverWs(lat, lon, accuracy) {
    if (wsConn && wsConn.state === signalR.HubConnectionState.Connected) {
      wsConn.invoke('SendLocation', lat, lon, accuracy || 0).catch(console.warn);
    }
  }

  function setupSignalR() {
    if (typeof signalR === 'undefined') return;

    wsConn = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/location')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    wsConn.on('LocationConfirmed', data => {
      updateWsIndicator(true, data.serverTimestamp);
    });
    wsConn.onreconnecting(() => updateWsIndicator(false, null));
    wsConn.onreconnected(()  => updateWsIndicator(true, null));
    wsConn.onclose(()        => updateWsIndicator(false, null));

    wsConn.start()
      .then(() => updateWsIndicator(true, null))
      .catch(err => console.warn('[LocationHub] SignalR connect failed:', err));
  }

  setupSignalR();

  /* ── Boot ─────────────────────────────────────────── */
  /* Auto-start GPS immediately on page load */
  startGPS();

})();
