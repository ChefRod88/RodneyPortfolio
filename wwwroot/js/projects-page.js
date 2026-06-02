// Projects page: terminal boot, card stagger, preview lightbox (Pages/Projects)
(function () {
  const terminalBoot = document.getElementById("terminalBoot");
  if (terminalBoot) {
    const count = terminalBoot.getAttribute("data-project-count") ?? "0";
    const lines = [
      "> Initializing project database...",
      `> Loading ${count} nodes...`,
      "> All systems nominal. Ready.",
    ];
    let lineIdx = 0;
    let charIdx = 0;
    let currentLine = document.createElement("div");
    currentLine.className = "terminal-line";
    terminalBoot.appendChild(currentLine);

    const timer = setInterval(() => {
      if (lineIdx >= lines.length) {
        clearInterval(timer);
        return;
      }
      const line = lines[lineIdx];
      if (charIdx < line.length) {
        currentLine.textContent += line[charIdx++];
      } else {
        lineIdx++;
        charIdx = 0;
        if (lineIdx < lines.length) {
          currentLine = document.createElement("div");
          currentLine.className = "terminal-line";
          terminalBoot.appendChild(currentLine);
        }
      }
    }, 28);
  }

  const beam = document.getElementById("projScanBeam");
  if (beam) {
    beam.classList.add("proj-scan-beam--active");
    beam.addEventListener("animationend", () => beam.remove());
  }

  function staggerCards() {
    document.querySelectorAll(".proj-card").forEach((card, i) => {
      card.style.transitionDelay = i * 80 + "ms";
    });
  }

  function initPreviewLightbox() {
    const lightbox = document.getElementById("projPreviewLightbox");
    const img = document.getElementById("projPreviewLightboxImg");
    const caption = document.getElementById("projPreviewLightboxCaption");
    if (!lightbox || !img || !caption) return;

    let lastTrigger = null;

    function openLightbox(src, name, trigger) {
      lastTrigger = trigger;
      img.src = src;
      img.alt = name ? `${name} preview` : "Project preview";
      caption.textContent = name || "";
      caption.hidden = !name;
      lightbox.hidden = false;
      lightbox.setAttribute("aria-hidden", "false");
      lightbox.classList.add("proj-lightbox--open");
      document.body.style.overflow = "hidden";
      const closeBtn = lightbox.querySelector(".proj-lightbox-close");
      closeBtn?.focus();
    }

    function closeLightbox() {
      if (lightbox.hidden) return;
      lightbox.hidden = true;
      lightbox.setAttribute("aria-hidden", "true");
      lightbox.classList.remove("proj-lightbox--open");
      document.body.style.overflow = "";
      img.removeAttribute("src");
      if (lastTrigger) {
        lastTrigger.focus();
        lastTrigger = null;
      }
    }

    document.querySelectorAll(".proj-preview-trigger").forEach((trigger) => {
      trigger.addEventListener("click", () => {
        const src = trigger.getAttribute("data-full-src");
        if (!src) return;
        const name = trigger.getAttribute("data-caption") || "";
        openLightbox(src, name, trigger);
      });
    });

    lightbox.querySelectorAll("[data-proj-lightbox-close]").forEach((el) => {
      el.addEventListener("click", closeLightbox);
    });

    lightbox.querySelector(".proj-lightbox-close")?.addEventListener("click", closeLightbox);

    document.addEventListener("keydown", (e) => {
      if (e.key === "Escape" && lightbox.classList.contains("proj-lightbox--open")) {
        e.preventDefault();
        closeLightbox();
      }
    });
  }

  function onReady() {
    staggerCards();
    initPreviewLightbox();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", onReady);
  } else {
    onReady();
  }
})();
