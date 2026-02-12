/**
 * Hero Banner - Scrolling photo strip
 * Pause on hover, respect prefers-reduced-motion
 */
(function () {
  const banner = document.getElementById('hero-banner');
  if (!banner) return;

  const track = banner.querySelector('.hero-banner-track');
  if (!track) return;

  const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  if (prefersReducedMotion) {
    banner.classList.add('hero-banner-reduced-motion');
    return;
  }

  function pause() {
    banner.classList.add('hero-banner-paused');
  }

  function resume() {
    banner.classList.remove('hero-banner-paused');
  }

  banner.addEventListener('mouseenter', pause);
  banner.addEventListener('mouseleave', resume);
  banner.addEventListener('touchstart', pause, { passive: true });
  banner.addEventListener('touchend', resume, { passive: true });
})();
