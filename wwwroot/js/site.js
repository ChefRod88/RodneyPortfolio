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
function initChatBot() {
  const messagesEl = document.getElementById("chat-messages");
  const inputEl = document.getElementById("chat-input");
  const sendBtn = document.getElementById("chat-send");
  const charRemainingEl = document.getElementById("chat-char-remaining");
  const maxLen = 500;

  const statusDotEl = document.getElementById("chat-status-dot");
  const statusLabelEl = document.getElementById("chat-status-label");

  if (!messagesEl || !inputEl || !sendBtn) return;

  function updateStatus(source, isError) {
    if (!statusDotEl || !statusLabelEl) return;
    statusDotEl.classList.remove("chat-status-api", "chat-status-demo", "chat-status-pending");
    if (isError || source === "demo") {
      statusDotEl.classList.add("chat-status-demo");
      statusLabelEl.textContent = "Demo mode or unable to connect to API";
    } else if (source === "api") {
      statusDotEl.classList.add("chat-status-api");
      statusLabelEl.textContent = "Connected to OpenAI API";
    } else {
      statusDotEl.classList.add("chat-status-pending");
      statusLabelEl.textContent = "Send a message to check connection";
    }
  }

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
        body: JSON.stringify({ message: trimmed })
      });
      const data = await res.json();

      loadingEl.remove();

      if (!res.ok) {
        updateStatus("demo", true);
        addMessage(data.error || "Something went wrong. Please try again.", "assistant");
        return;
      }

      const source = data.source || "demo";
      updateStatus(source, false);
      addMessage(data.reply || "I couldn't generate a response.", "assistant");
    } catch (err) {
      loadingEl.remove();
      updateStatus("demo", true);
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

  document.querySelectorAll(".chat-suggestion-btn").forEach((btn) => {
    btn.addEventListener("click", () => {
      const q = btn.getAttribute("data-question");
      if (q) sendMessage(q);
    });
  });
}

// PWA: Register the service worker for offline support
function registerServiceWorker() {
  if (!("serviceWorker" in navigator)) return;

  navigator.serviceWorker
    .register("/sw.js")
    .then((reg) => reg.update())
    .catch((err) => console.warn("Service worker registration failed:", err));
}


