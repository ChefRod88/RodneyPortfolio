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
  initChatBot();
});

// ================================
// ASK RODNEY - CHATBOT
// ================================
const CHAT_MODE_KEY = "askRodneyMode";

function initChatBot() {
  const messagesEl = document.getElementById("chat-messages");
  const inputEl = document.getElementById("chat-input");
  const sendBtn = document.getElementById("chat-send");
  const charRemainingEl = document.getElementById("chat-char-remaining");
  const maxLen = 500;

  if (!messagesEl || !inputEl || !sendBtn) return;

  function getMode() {
    const active = document.querySelector(".chat-mode-btn.active");
    return active?.getAttribute("data-mode") || "recruiter";
  }

  function setMode(mode) {
    document.querySelectorAll(".chat-mode-btn").forEach((btn) => {
      const isActive = btn.getAttribute("data-mode") === mode;
      btn.classList.toggle("active", isActive);
      btn.setAttribute("aria-selected", isActive);
    });
    try {
      sessionStorage.setItem(CHAT_MODE_KEY, mode);
    } catch (_) {}
  }

  const savedMode = (() => {
    try {
      return sessionStorage.getItem(CHAT_MODE_KEY) || "recruiter";
    } catch (_) {
      return "recruiter";
    }
  })();
  setMode(savedMode);

  document.querySelectorAll(".chat-mode-btn").forEach((btn) => {
    btn.addEventListener("click", () => setMode(btn.getAttribute("data-mode") || "recruiter"));
  });

  function addMessage(text, role) {
    const div = document.createElement("div");
    div.className = `chat-message ${role}`;
    div.textContent = text;
    div.setAttribute("role", "listitem");
    messagesEl.appendChild(div);
    messagesEl.scrollTop = messagesEl.scrollHeight;
  }

  function setLoading(loading) {
    sendBtn.disabled = loading;
    inputEl.disabled = loading;
  }

  async function sendMessage(text) {
    const trimmed = text.trim();
    if (!trimmed) return;

    addMessage(trimmed, "user");
    inputEl.value = "";
    updateCharCount();

    const loadingEl = document.createElement("div");
    loadingEl.className = "chat-message assistant loading";
    loadingEl.textContent = "Thinking...";
    loadingEl.setAttribute("role", "listitem");
    messagesEl.appendChild(loadingEl);
    messagesEl.scrollTop = messagesEl.scrollHeight;

    setLoading(true);

    try {
      const res = await fetch("/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message: trimmed, mode: getMode() })
      });
      const data = await res.json();

      loadingEl.remove();

      if (!res.ok) {
        addMessage(data.error || "Something went wrong. Please try again.", "assistant");
        return;
      }

      addMessage(data.reply || "I couldn't generate a response.", "assistant");
    } catch (err) {
      loadingEl.remove();
      addMessage("Unable to connect. Please check your connection and try again.", "assistant");
    } finally {
      setLoading(false);
    }
  }

  function updateCharCount() {
    if (charRemainingEl) {
      const len = inputEl.value.length;
      charRemainingEl.textContent = maxLen - len;
    }
  }

  sendBtn.addEventListener("click", () => sendMessage(inputEl.value));
  inputEl.addEventListener("keydown", (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage(inputEl.value);
    }
  });
  inputEl.addEventListener("input", updateCharCount);

  const transparencyToggle = document.getElementById("chat-transparency-toggle");
  const transparencyContent = document.getElementById("chat-transparency-content");
  if (transparencyToggle && transparencyContent) {
    transparencyToggle.addEventListener("click", () => {
      const isExpanded = transparencyToggle.getAttribute("aria-expanded") === "true";
      transparencyToggle.setAttribute("aria-expanded", !isExpanded);
      transparencyContent.hidden = isExpanded;
    });
  }

  initJobMatch();
}

function initJobMatch() {
  const inputEl = document.getElementById("job-match-input");
  const analyzeBtn = document.getElementById("job-match-analyze");
  const resultEl = document.getElementById("job-match-result");
  const charRemainingEl = document.getElementById("job-match-char-remaining");
  const scoreValueEl = document.getElementById("job-match-score-value");
  const skillsEl = document.getElementById("job-match-skills");
  const gapsEl = document.getElementById("job-match-gaps");
  const talkingEl = document.getElementById("job-match-talking");
  const maxLen = 4000;

  if (!inputEl || !analyzeBtn || !resultEl) return;

  function updateCharCount() {
    if (charRemainingEl) {
      const len = inputEl.value.length;
      charRemainingEl.textContent = maxLen - len;
    }
  }

  function renderList(ul, items) {
    if (!ul) return;
    ul.innerHTML = "";
    if (!items || items.length === 0) {
      const li = document.createElement("li");
      li.textContent = "None";
      li.className = "chat-job-match-empty";
      ul.appendChild(li);
      return;
    }
    items.forEach((item) => {
      const li = document.createElement("li");
      li.textContent = item;
      ul.appendChild(li);
    });
  }

  async function analyze() {
    const trimmed = inputEl.value.trim();
    if (!trimmed) return;

    analyzeBtn.disabled = true;
    resultEl.hidden = true;

    try {
      const res = await fetch("/api/chat/job-match", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ jobDescription: trimmed })
      });
      const data = await res.json();

      if (!res.ok) {
        alert(data.error || "Something went wrong. Please try again.");
        return;
      }

      if (scoreValueEl) scoreValueEl.textContent = data.matchScore ?? 0;
      renderList(skillsEl, data.skillsAligned);
      renderList(gapsEl, data.gaps);
      renderList(talkingEl, data.talkingPoints);
      resultEl.hidden = false;
    } catch (err) {
      alert("Unable to connect. Please check your connection and try again.");
    } finally {
      analyzeBtn.disabled = false;
    }
  }

  inputEl.addEventListener("input", updateCharCount);
  analyzeBtn.addEventListener("click", analyze);
}

// PWA: Register the service worker for offline support
function registerServiceWorker() {
  if (!("serviceWorker" in navigator)) return;

  navigator.serviceWorker
    .register("/sw.js")
    .then((reg) => reg.update())
    .catch((err) => console.warn("Service worker registration failed:", err));
}


