// Matrix background, global nav + FAQ chrome (Razor _Layout)
(function () {
  const canvas = document.getElementById("rc-matrix");
  if (!canvas) return;
  const ctx = canvas.getContext("2d");
  const chars =
    "アイウエオカキクケコサシスセソタチツテトナニヌネノ0123456789ABCDEFabcdef{}[]<>/\\|=+-*{}#@@";
  const fontSize = 13;
  let cols;
  let drops;

  function resize() {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
  }
  function initDrops() {
    cols = Math.floor(canvas.width / fontSize);
    drops = Array.from({ length: cols }, () => Math.random() * -80);
  }
  resize();
  initDrops();
  window.addEventListener("resize", () => {
    resize();
    initDrops();
  });

  function draw() {
    ctx.fillStyle = "rgba(2,12,20,0.052)";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.font = fontSize + 'px "Share Tech Mono", monospace';
    for (let i = 0; i < cols; i++) {
      const y = drops[i] * fontSize;
      ctx.fillStyle = "rgba(0,220,255,0.95)";
      ctx.fillText(chars[Math.floor(Math.random() * chars.length)], i * fontSize, y);
      ctx.fillStyle = "rgba(0,160,210,0.6)";
      ctx.fillText(
        chars[Math.floor(Math.random() * chars.length)],
        i * fontSize,
        y - fontSize
      );
      ctx.fillStyle = "rgba(0,80,130,0.3)";
      ctx.fillText(
        chars[Math.floor(Math.random() * chars.length)],
        i * fontSize,
        y - fontSize * 2
      );
      if (y > canvas.height && Math.random() > 0.975) drops[i] = Math.random() * -40;
      drops[i] += 0.48 + Math.random() * 0.38;
    }
  }
  setInterval(draw, 46);
})();

document.addEventListener("click", function (e) {
  if (!e.target.closest("#hamburger-nav")) {
    document.querySelector(".menu-links")?.classList.remove("open");
    document.querySelector(".hamburger-icon")?.classList.remove("open");
  }
});

const rcObserver = new IntersectionObserver(
  (entries) => {
    entries.forEach((e) => {
      if (e.isIntersecting) {
        e.target.classList.add("rc-visible");
      }
    });
  },
  { threshold: 0.07 }
);

document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll(".rc-fade").forEach((el) => rcObserver.observe(el));
  document.querySelectorAll(".faq-q").forEach((btn) => {
    btn.addEventListener("click", () => {
      const isOpen = btn.getAttribute("aria-expanded") === "true";
      document.querySelectorAll(".faq-q").forEach((b) => {
        b.setAttribute("aria-expanded", "false");
        b.nextElementSibling?.classList.remove("open");
      });
      if (!isOpen) {
        btn.setAttribute("aria-expanded", "true");
        btn.nextElementSibling?.classList.add("open");
      }
    });
  });
});
