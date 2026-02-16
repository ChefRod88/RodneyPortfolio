/**
 * Church Website - Main site JavaScript
 * USE CASE: Smooth scroll, transparent nav on home page
 * LOCATION: Loaded on every page via _Layout.cshtml
 */
document.addEventListener('DOMContentLoaded', function () {
  // SMOOTH SCROLL: Find all links that start with # (e.g. #three-cards-section)
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
});
