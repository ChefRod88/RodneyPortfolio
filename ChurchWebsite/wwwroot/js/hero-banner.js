/**
 * Hero Banner - Scrolling photo strip (optional; not used on current home)
 * Runs only if #hero-banner exists. Pauses on hover/touch; respects prefers-reduced-motion.
 */
(function () {
  const banner = document.getElementById('hero-banner');
  if (!banner) return;

  const track = banner.querySelector('.hero-banner-track');
  if (!track) return;

  /* Check if user prefers reduced motion (accessibility) - stop animation if so */
  const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  if (prefersReducedMotion) {
    banner.classList.add('hero-banner-reduced-motion');
    return;
  }

  /* Pause animation when user hovers or touches - lets them read content */
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
