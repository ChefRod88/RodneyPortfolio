// Intersection observer for scroll animations
const observer = new IntersectionObserver((entries) => {
    entries.forEach(e => { if (e.isIntersecting) e.target.classList.add('visible'); });
}, { threshold: 0.12 });
document.querySelectorAll('.animate-in, .timeline-item').forEach(el => observer.observe(el));

// Staggered card entry
const cardObserver = new IntersectionObserver((entries) => {
    entries.forEach((e, i) => { if (e.isIntersecting) setTimeout(() => e.target.classList.add('visible'), i * 80); });
}, { threshold: 0.1 });
document.querySelectorAll('.minister-card, .deacon-card, .belief-card').forEach(el => cardObserver.observe(el));

// Nav dot active state
const sections = ['hero','leadership','ministers','music','history','beliefs'];
const dots = document.querySelectorAll('.nav-dot');
const sectionObserver = new IntersectionObserver((entries) => {
    entries.forEach(e => {
        if (e.isIntersecting) {
            dots.forEach(d => d.classList.toggle('active', d.dataset.target === e.target.id));
        }
    });
}, { threshold: 0.4 });
sections.forEach(id => { const el = document.getElementById(id); if (el) sectionObserver.observe(el); });

function scrollToSection(id) {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' });
}

function switchTab(panelId, btn) {
    document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    document.getElementById(panelId).classList.add('active');
    btn.classList.add('active');
}
