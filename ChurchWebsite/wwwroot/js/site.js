// Church Website - Calvary-style JavaScript
document.addEventListener('DOMContentLoaded', function () {
  // Smooth scroll for anchor links
  document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
    anchor.addEventListener('click', function (e) {
      const targetId = this.getAttribute('href');
      if (targetId === '#') return;
      const target = document.querySelector(targetId);
      if (target) {
        e.preventDefault();
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    });
  });

  // Transparent nav over hero - becomes solid on scroll (home page only)
  const nav = document.getElementById('mainNav');
  const isHome = document.body.classList.contains('home-page');
  const hero = document.querySelector('.hero-calvary');

  if (isHome && nav && hero) {
    function updateNav() {
      const heroBottom = hero.getBoundingClientRect().bottom;
      if (heroBottom > 80) {
        nav.classList.add('navbar-transparent');
        nav.classList.remove('bg-white');
      } else {
        nav.classList.remove('navbar-transparent');
        nav.classList.add('bg-white');
      }
    }

    updateNav();
    window.addEventListener('scroll', updateNav, { passive: true });
  } else if (nav) {
    nav.classList.add('bg-white');
  }
});
