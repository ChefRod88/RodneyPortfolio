// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function toggleMenu() {
  const menu = document.querySelector(".menu-links");
  const icon = document.querySelector(".hamburger-icon");
  if (!menu || !icon) return;

  menu.classList.toggle("open");
  icon.classList.toggle("open");
}

function initAutoScrollCollage() {
  const banner = document.querySelector(".about-collage-banner");
  const track = banner?.querySelector(".collage-track");
  if (!banner || !track) return;

  const prefersReducedMotion = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches;
  if (prefersReducedMotion) return;

  const speed = 20; // pixels per second
  let paused = false;
  let hoverPaused = false;
  let lastTime = null;

  const pause = () => {
    paused = true;
  };

  const resume = () => {
    if (!hoverPaused) paused = false;
  };

  const step = (timestamp) => {
    if (lastTime === null) lastTime = timestamp;
    const deltaSeconds = (timestamp - lastTime) / 1000;
    lastTime = timestamp;

    if (!paused) {
      banner.scrollLeft += speed * deltaSeconds;
      const loopPoint = track.scrollWidth / 2;
      if (banner.scrollLeft >= loopPoint) {
        banner.scrollLeft -= loopPoint;
      }
    }

    requestAnimationFrame(step);
  };

  banner.addEventListener("mouseenter", () => {
    hoverPaused = true;
    pause();
  });

  banner.addEventListener("mouseleave", () => {
    hoverPaused = false;
    resume();
  });

  banner.addEventListener("touchstart", () => {
    hoverPaused = true;
    pause();
  }, { passive: true });

  banner.addEventListener("touchend", () => {
    hoverPaused = false;
    resume();
  });

  let resumeTimer = null;
  const temporarilyPause = () => {
    pause();
    if (resumeTimer) clearTimeout(resumeTimer);
    resumeTimer = setTimeout(resume, 1500);
  };

  banner.addEventListener("wheel", temporarilyPause, { passive: true });

  requestAnimationFrame(step);
}

document.addEventListener("DOMContentLoaded", () => {
  const year = new Date().getFullYear();
  const el = document.getElementById("copyright");
  if (el) el.textContent = `Copyright © ${year} Rodney Chery. All Rights Reserved.`;

  initAutoScrollCollage();
  registerServiceWorker();
});

// PWA: Register the service worker for offline support
function registerServiceWorker() {
  if (!("serviceWorker" in navigator)) return;

  navigator.serviceWorker
    .register("/sw.js")
    .then((reg) => reg.update())
    .catch((err) => console.warn("Service worker registration failed:", err));
}


