// Hero word blur-in
window.addEventListener('load', () => {
  const words = document.querySelectorAll('.word');
  words.forEach((w, i) => setTimeout(() => w.classList.add('show'), 300 + i * 150));
});

// Matrix collage: dual-layer opacity crossfade + staggered swap cadence (home only)
(function initHeroMatrixCollage() {
  const matrix = document.getElementById('heroMatrix');
  const jsonEl = document.getElementById('hero-matrix-slides');
  if (!matrix || !jsonEl) return;

  let slides;
  try {
    slides = JSON.parse(jsonEl.textContent || '[]');
  } catch {
    return;
  }
  if (!Array.isArray(slides) || slides.length === 0) return;
  if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
  if (slides.length < 2) return;

  /** ~1.35s CSS crossfade — intervals long enough for fade + a short beat on each image */
  const BASE_INTERVAL_MS = 2650;
  const STAGGER_MS = 380;
  const LOAD_GUARD_MS = 2600;

  const cells = Array.from(matrix.querySelectorAll('.hero-matrix-cell'));
  const innerEls = cells.map((c) => c.querySelector('.hero-matrix-cell-inner')).filter(Boolean);
  const cellImgs = cells.map((c) => Array.from(c.querySelectorAll('.hero-matrix-img')));
  if (innerEls.length !== cells.length || cellImgs.some((im) => im.length < 2)) return;

  const pauseBtn = document.getElementById('heroMatrixPause');
  const labelEl = pauseBtn && pauseBtn.querySelector('.hero-matrix-pause-label');
  let playing = true;
  const timers = [];

  function slideAt(i) {
    return slides[((i % slides.length) + slides.length) % slides.length];
  }

  function swapCell(cellIdx) {
    const cell = cells[cellIdx];
    const imgs = cellImgs[cellIdx];
    if (!cell || imgs.length < 2) return;

    const a = imgs[0];
    const b = imgs[1];
    const front = a.classList.contains('is-front') ? a : b;
    const back = front === a ? b : a;

    let idx = parseInt(cell.dataset.slideIndex || '0', 10);
    if (Number.isNaN(idx)) idx = 0;
    const nextIdx = (idx + 1) % slides.length;
    const next = slideAt(nextIdx);

    let settled = false;
    const crossfade = () => {
      if (settled) return;
      settled = true;
      front.classList.remove('is-front');
      back.classList.add('is-front');
      cell.dataset.slideIndex = String(nextIdx);
      const preloadIdx = (nextIdx + 1) % slides.length;
      const preload = slideAt(preloadIdx);
      front.src = preload.url;
      front.alt = '';
    };

    back.alt = '';
    back.src = next.url;
    if (back.complete) crossfade();
    else back.addEventListener('load', crossfade, { once: true });
    window.setTimeout(crossfade, LOAD_GUARD_MS);
  }

  function scheduleCells() {
    cells.forEach((_, c) => {
      const id = window.setInterval(() => {
        if (playing) swapCell(c);
      }, BASE_INTERVAL_MS + c * STAGGER_MS);
      timers.push(id);
    });
  }

  scheduleCells();

  function pauseAll() {
    playing = false;
    timers.forEach((id) => clearInterval(id));
    timers.length = 0;
    if (pauseBtn) pauseBtn.setAttribute('aria-pressed', 'true');
    if (labelEl) labelEl.textContent = 'Resume matrix';
  }

  function resumeAll() {
    playing = true;
    scheduleCells();
    if (pauseBtn) pauseBtn.setAttribute('aria-pressed', 'false');
    if (labelEl) labelEl.textContent = 'Pause matrix';
  }

  if (pauseBtn && labelEl) {
    pauseBtn.addEventListener('click', () => {
      if (playing) pauseAll();
      else resumeAll();
    });
  }
})();

// Parallax: matrix stack
window.addEventListener('scroll', () => {
  const y = window.scrollY;
  const matrix = document.getElementById('heroMatrix');
  const ph = document.querySelector('.hero-bg-placeholder');
  if (matrix) {
    matrix.style.transform = `translateY(${y * 0.3}px)`;
  } else if (ph) {
    ph.style.transform = `translateY(${y * 0.3}px)`;
  }
  const txt = document.querySelector('.hero-text-wrap');
  if (txt) txt.style.transform = `translateY(${y * 0.15}px)`;
}, { passive: true });

// Scroll reveal
const obs = new IntersectionObserver(entries => {
  entries.forEach(e => { if (e.isIntersecting) e.target.classList.add('visible'); });
}, { threshold: 0.1 });
document.querySelectorAll('.reveal').forEach(el => obs.observe(el));
