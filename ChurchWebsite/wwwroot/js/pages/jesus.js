// Parallax hero
window.addEventListener('scroll', () => {
  const y = window.scrollY;
  const bg = document.querySelector('.jesus-hero-ph, .jesus-hero-img');
  if (bg) bg.style.transform = `translateY(${y * 0.3}px)`;
  const txt = document.querySelector('.jesus-hero-text');
  if (txt) txt.style.transform = `translateY(${y * 0.15}px)`;
}, { passive: true });

// Accordion
document.querySelectorAll('.faq-trigger').forEach(trigger => {
  trigger.addEventListener('click', () => {
    const item = trigger.closest('.faq-item');
    const body = item.querySelector('.faq-body');
    const isOpen = item.classList.contains('open');

    // Close all
    document.querySelectorAll('.faq-item.open').forEach(openItem => {
      openItem.classList.remove('open');
      openItem.querySelector('.faq-trigger').setAttribute('aria-expanded', 'false');
      openItem.querySelector('.faq-body').style.maxHeight = '0';
    });

    // Open clicked (if it was closed)
    if (!isOpen) {
      item.classList.add('open');
      trigger.setAttribute('aria-expanded', 'true');
      body.style.maxHeight = body.scrollHeight + 'px';
    }
  });
});

// Scroll reveal
const obs = new IntersectionObserver(entries => {
  entries.forEach(e => { if (e.isIntersecting) e.target.classList.add('visible'); });
}, { threshold: 0.1 });
document.querySelectorAll('.reveal').forEach(el => obs.observe(el));
