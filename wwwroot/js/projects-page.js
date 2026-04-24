// Projects page terminal boot + scan (Pages/Projects)
(function () {
  const el = document.getElementById("terminalBoot");
  if (!el) return;
  const count = el.getAttribute("data-project-count") ?? "0";
  const lines = [
    "> Initializing project database...",
    `> Loading ${count} nodes...`,
    "> All systems nominal. Ready.",
  ];
  let lineIdx = 0;
  let charIdx = 0;
  let currentLine = document.createElement("div");
  currentLine.className = "terminal-line";
  el.appendChild(currentLine);

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
        el.appendChild(currentLine);
      }
    }
  }, 28);

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
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", staggerCards);
  } else {
    staggerCards();
  }
})();
