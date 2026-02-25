/**
 * Location route experience for /Location page.
 * Uses Leaflet for rendering and GraphHopper for route instructions.
 */
(function locationRouteFeature() {
  const card = document.getElementById('location-status-card');
  const mapEl = document.getElementById('location-map');
  if (!card || !mapEl || typeof L === 'undefined') return;

  const panelMeta = document.getElementById('location-directions-meta');
  const panelSteps = document.getElementById('location-directions-steps');

  const parseOptionalNumber = (value) => {
    if (typeof value !== 'string' || value.trim() === '') return null;
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  };

  const destination = {
    name: card.dataset.destinationName || 'Church',
    address: card.dataset.destinationAddress || '',
    lat: parseOptionalNumber(card.dataset.destinationLat),
    lon: parseOptionalNumber(card.dataset.destinationLon)
  };

  const graphHopperKey = card.dataset.graphhopperKey || '';
  const serverLat = parseOptionalNumber(card.dataset.locationLat);
  const serverLon = parseOptionalNumber(card.dataset.locationLon);

  const defaultCenter = destination.lat !== null && destination.lon !== null
    ? [destination.lat, destination.lon]
    : [28.03, -81.73];

  const map = L.map(mapEl, {
    zoomControl: true,
    attributionControl: true
  }).setView(defaultCenter, 12);

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap contributors'
  }).addTo(map);

  if (destination.lat !== null && destination.lon !== null) {
    const churchMarker = L.marker([destination.lat, destination.lon], {
      title: destination.name
    }).addTo(map);
    churchMarker.bindPopup(`<strong>${destination.name}</strong><br>${destination.address || ''}`);
  }

  let routeLayer = null;
  let travelMarker = null;
  let inflight = false;
  let queued = false;
  let lastRequestedAt = 0;

  function updateMeta(message) {
    if (panelMeta) panelMeta.textContent = message;
  }

  function clearSteps() {
    if (panelSteps) panelSteps.innerHTML = '';
  }

  function setSteps(steps) {
    if (!panelSteps) return;
    panelSteps.innerHTML = '';
    if (!steps.length) {
      const li = document.createElement('li');
      li.textContent = 'No step-by-step directions were returned.';
      panelSteps.appendChild(li);
      return;
    }
    steps.forEach((step) => {
      const li = document.createElement('li');
      li.textContent = step;
      panelSteps.appendChild(li);
    });
  }

  function decodePolyline(encoded, is3d) {
    const points = [];
    let index = 0;
    let lat = 0;
    let lng = 0;
    let ele = 0;
    const factor = 1e5;

    while (index < encoded.length) {
      let shift = 0;
      let result = 0;
      let byte;
      do {
        byte = encoded.charCodeAt(index++) - 63;
        result |= (byte & 0x1f) << shift;
        shift += 5;
      } while (byte >= 0x20);
      const latitudeChange = (result & 1) ? ~(result >> 1) : (result >> 1);
      lat += latitudeChange;

      shift = 0;
      result = 0;
      do {
        byte = encoded.charCodeAt(index++) - 63;
        result |= (byte & 0x1f) << shift;
        shift += 5;
      } while (byte >= 0x20);
      const longitudeChange = (result & 1) ? ~(result >> 1) : (result >> 1);
      lng += longitudeChange;

      if (is3d) {
        shift = 0;
        result = 0;
        do {
          byte = encoded.charCodeAt(index++) - 63;
          result |= (byte & 0x1f) << shift;
          shift += 5;
        } while (byte >= 0x20);
        const elevationChange = (result & 1) ? ~(result >> 1) : (result >> 1);
        ele += elevationChange;
      }

      points.push([lat / factor, lng / factor]);
    }
    return points;
  }

  function formatMeters(meters) {
    if (!Number.isFinite(meters)) return '';
    if (meters >= 1000) return `${(meters / 1000).toFixed(1)} km`;
    return `${Math.round(meters)} m`;
  }

  function formatSeconds(seconds) {
    if (!Number.isFinite(seconds)) return '';
    const mins = Math.round(seconds / 60);
    if (mins < 60) return `${mins} min`;
    const hours = Math.floor(mins / 60);
    const rem = mins % 60;
    return rem > 0 ? `${hours} hr ${rem} min` : `${hours} hr`;
  }

  function animateRoutePath() {
    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReduced || !routeLayer) return;

    const path = routeLayer._path;
    if (!path || typeof path.getTotalLength !== 'function') return;

    const totalLength = path.getTotalLength();
    path.classList.add('route-line-animated');
    path.style.strokeDasharray = String(totalLength);
    path.style.strokeDashoffset = String(totalLength);
    path.style.transition = 'stroke-dashoffset 1800ms ease-out';
    requestAnimationFrame(() => {
      path.style.strokeDashoffset = '0';
    });
  }

  function animateTravelMarker(coords) {
    if (!coords || coords.length < 2) return;
    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReduced) return;

    if (travelMarker) {
      map.removeLayer(travelMarker);
      travelMarker = null;
    }

    const icon = L.divIcon({
      className: 'route-travel-dot',
      html: '<div style="width:12px;height:12px;border-radius:50%;background:#2563eb;border:2px solid #fff;"></div>',
      iconSize: [12, 12],
      iconAnchor: [6, 6]
    });

    travelMarker = L.marker(coords[0], { icon }).addTo(map);

    const durationMs = 3500;
    const start = performance.now();

    function tick(now) {
      const progress = Math.min((now - start) / durationMs, 1);
      const idx = Math.min(Math.floor(progress * (coords.length - 1)), coords.length - 1);
      travelMarker.setLatLng(coords[idx]);
      if (progress < 1) {
        requestAnimationFrame(tick);
      }
    }
    requestAnimationFrame(tick);
  }

  async function fetchRoute(originLat, originLon) {
    if (!graphHopperKey) {
      updateMeta('Routing unavailable: GraphHopper API key is missing.');
      clearSteps();
      return null;
    }

    const qp = new URLSearchParams({
      key: graphHopperKey,
      vehicle: 'car',
      locale: 'en',
      points_encoded: 'true',
      instructions: 'true',
      calc_points: 'true'
    });
    if (destination.lat === null || destination.lon === null) {
      throw new Error('Destination coordinates are not configured');
    }

    qp.append('point', `${originLat},${originLon}`);
    qp.append('point', `${destination.lat},${destination.lon}`);

    const url = `https://graphhopper.com/api/1/route?${qp.toString()}`;
    const response = await fetch(url, { method: 'GET' });
    if (!response.ok) {
      throw new Error(`Routing HTTP ${response.status}`);
    }
    const payload = await response.json();
    if (!payload.paths || !payload.paths.length) {
      throw new Error('No route found');
    }
    return payload.paths[0];
  }

  function parseStepText(instruction) {
    const text = (instruction.text || '').trim();
    const distance = formatMeters(instruction.distance);
    if (!text) return distance;
    if (!distance) return text;
    return `${text} (${distance})`;
  }

  async function renderRoute(originLat, originLon) {
    const now = Date.now();
    if (inflight || (now - lastRequestedAt < 900)) {
      queued = true;
      return;
    }

    inflight = true;
    lastRequestedAt = now;
    try {
      updateMeta('Calculating fastest route...');
      const path = await fetchRoute(originLat, originLon);
      const coords = decodePolyline(path.points, false);

      if (routeLayer) map.removeLayer(routeLayer);
      routeLayer = L.polyline(coords, {
        color: '#2563eb',
        weight: 5,
        opacity: 0.9
      }).addTo(map);

      animateRoutePath();
      animateTravelMarker(coords);

      const bounds = L.latLngBounds(coords);
      map.fitBounds(bounds, {
        paddingTopLeft: [30, 30],
        paddingBottomRight: [420, 220]
      });

      const eta = formatSeconds(path.time / 1000);
      const distance = formatMeters(path.distance);
      updateMeta(`Estimated ${eta} • ${distance}`);

      const steps = (path.instructions || []).map(parseStepText).filter(Boolean).slice(0, 12);
      setSteps(steps);
    } catch (_error) {
      updateMeta('Could not calculate route right now. Please try again.');
      clearSteps();
    } finally {
      inflight = false;
      if (queued) {
        queued = false;
        setTimeout(() => {
          renderRoute(originLat, originLon);
        }, 200);
      }
    }
  }

  function tryRouteFromServerCoordinates() {
    if (Number.isFinite(serverLat) && Number.isFinite(serverLon)) {
      renderRoute(serverLat, serverLon);
    } else {
      updateMeta('Waiting for your device location...');
    }
  }

  const refreshBtn = document.getElementById('location-refresh-btn');
  let latestCoords = serverLat !== null && serverLon !== null
    ? { lat: serverLat, lon: serverLon }
    : null;

  if (refreshBtn) {
    refreshBtn.addEventListener('click', () => {
      if (latestCoords) {
        renderRoute(latestCoords.lat, latestCoords.lon);
      } else {
        tryRouteFromServerCoordinates();
      }
    });
  }

  document.addEventListener('church:location-resolved', (event) => {
    const detail = event && event.detail ? event.detail : {};
    const lat = typeof detail.latitude === 'number' && Number.isFinite(detail.latitude) ? detail.latitude : null;
    const lon = typeof detail.longitude === 'number' && Number.isFinite(detail.longitude) ? detail.longitude : null;
    if (lat === null || lon === null) return;
    latestCoords = { lat, lon };
    renderRoute(lat, lon);
  });

  tryRouteFromServerCoordinates();
})();
