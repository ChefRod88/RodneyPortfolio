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

  // NAV BACKGROUND: Remove any leftover transparent/white classes; CSS handles the cream bg.
  const nav = document.getElementById('mainNav');
  if (nav) {
    nav.classList.remove('navbar-transparent');
    nav.classList.remove('bg-white');
  }

  // MARQUEE: Duplicate content for seamless infinite loop
  const marqueeTrack = document.querySelector('.marquee-track');
  if (marqueeTrack) {
    marqueeTrack.innerHTML += marqueeTrack.innerHTML;
  }

  // SCROLL REVEAL
  const revealEls = document.querySelectorAll('.reveal');
  if (revealEls.length) {
    const revealObs = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible');
          revealObs.unobserve(entry.target);
        }
      });
    }, { threshold: 0.1 });
    revealEls.forEach(function (el) { revealObs.observe(el); });
  }

});
