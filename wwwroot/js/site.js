// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Client sites link to https://www.rodneyachery.com/#support — send them to the support page.
if (location.pathname === "/" && location.hash === "#support") {
  location.replace("/Support");
}

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

  if (!messagesEl || !inputEl || !sendBtn) return;

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
  
  // ICM elements
  const icmToggleEl = document.getElementById("job-match-icm-toggle");
  const icmLoaderEl = document.getElementById("job-match-icm-loading");
  const icmStatusEl = document.getElementById("icm-status-message");
  const icmPipelineEl = document.getElementById("job-match-icm-pipeline");
  const stage1El = document.getElementById("icm-stage-1");
  const stage2El = document.getElementById("icm-stage-2");
  const stage3El = document.getElementById("icm-stage-3");
  
  const maxLen = 4000;

  if (!inputEl || !analyzeBtn || !resultEl) return;

  // Setup tab events
  const tabBtns = document.querySelectorAll(".icm-tab-btn");
  tabBtns.forEach(btn => {
    btn.addEventListener("click", () => {
      tabBtns.forEach(b => b.classList.remove("active"));
      btn.classList.add("active");
      
      const targetId = btn.getAttribute("data-tab");
      const contents = document.querySelectorAll(".icm-tab-content");
      contents.forEach(c => {
        if (c.id === targetId) {
          c.style.display = "block";
        } else {
          c.style.display = "none";
        }
      });
    });
  });

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

    const useIcm = icmToggleEl ? icmToggleEl.checked : false;

    analyzeBtn.disabled = true;
    resultEl.hidden = true;
    if (icmPipelineEl) icmPipelineEl.hidden = true;

    let statusInterval;
    if (useIcm && icmLoaderEl && icmStatusEl) {
      icmLoaderEl.style.display = "block";
      icmStatusEl.textContent = "Stage 1: Extracting Job Requirements (01_skill_extraction)...";
      let elapsed = 0;
      statusInterval = setInterval(() => {
        elapsed += 1;
        if (elapsed < 4) {
          icmStatusEl.textContent = "Stage 1: Extracting Job Requirements (01_skill_extraction)...";
        } else if (elapsed < 9) {
          icmStatusEl.textContent = "Stage 2: Evaluating Fit & Identifying Gaps (02_resume_comparison)...";
        } else {
          icmStatusEl.textContent = "Stage 3: Generating Prep Guide & Final Scoring (03_interview_preparation)...";
        }
      }, 1000);
    }

    try {
      const res = await fetch("/api/chat/job-match", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ 
          jobDescription: trimmed,
          useIcm: useIcm
        })
      });
      const data = await res.json();

      if (!res.ok) {
        alert(data.error || "Something went wrong. Please try again.");
        return;
      }

      // Render main results
      if (scoreValueEl) scoreValueEl.textContent = data.matchScore ?? 0;
      renderList(skillsEl, data.skillsAligned);
      renderList(gapsEl, data.gaps);
      renderList(talkingEl, data.talkingPoints);
      resultEl.hidden = false;

      // Handle ICM specific rendering
      if (data.useIcm) {
        if (stage1El) stage1El.textContent = data.stage1Output || "No output generated.";
        if (stage2El) stage2El.textContent = data.stage2Output || "No output generated.";
        if (stage3El) stage3El.textContent = data.stage3Output || "No output generated.";
        
        if (icmPipelineEl) icmPipelineEl.hidden = false;
      }
    } catch (err) {
      alert("Unable to connect. Please check your connection and try again.");
    } finally {
      if (statusInterval) clearInterval(statusInterval);
      if (icmLoaderEl) icmLoaderEl.style.display = "none";
      analyzeBtn.disabled = false;
    }
  }

  inputEl.addEventListener("input", updateCharCount);
  analyzeBtn.addEventListener("click", analyze);
}

// PWA: Register the service worker for offline support
function registerServiceWorker() {
  if (!("serviceWorker" in navigator)) return;

  const migrateKey = "rodney-portfolio-sw-v2";

  navigator.serviceWorker
    .getRegistrations()
    .then((registrations) => {
      if (!localStorage.getItem(migrateKey)) {
        return Promise.all(registrations.map((r) => r.unregister())).then(() => {
          localStorage.setItem(migrateKey, "1");
        });
      }
    })
    .then(() => navigator.serviceWorker.register("/sw.js"))
    .then((reg) => reg.update())
    .catch((err) => console.warn("Service worker registration failed:", err));
}

// ============================================================
// FUTURISTIC / HUD INTERACTIVE FUNCTIONS
// ============================================================

// 1. Mouse coordinates telemetry tracker
document.addEventListener('mousemove', (e) => {
  const xEl = document.getElementById('hud-coord-x');
  const yEl = document.getElementById('hud-coord-y');
  if (xEl && yEl) {
    const x = String(Math.round(e.clientX)).padStart(3, '0');
    const y = String(Math.round(e.clientY)).padStart(3, '0');
    xEl.textContent = x;
    yEl.textContent = y;
  }
});

document.addEventListener('DOMContentLoaded', () => {
  // 3. CLI Terminal Drawer
  const terminalDrawer = document.getElementById('terminalDrawer');
  const terminalHeader = document.getElementById('terminalHeader');
  const terminalToggleBtn = document.getElementById('terminalToggleBtn');
  const terminalInput = document.getElementById('terminalInput');
  const terminalHistory = document.getElementById('terminalHistory');

  if (terminalDrawer && (terminalHeader || terminalToggleBtn) && terminalInput && terminalHistory) {
    const toggleDrawer = () => {
      terminalDrawer.classList.toggle('open');
      if (terminalDrawer.classList.contains('open')) {
        setTimeout(() => terminalInput.focus(), 100);
      }
    };

    if (terminalHeader) {
      terminalHeader.addEventListener('click', (e) => {
        if (e.target !== terminalToggleBtn) {
          toggleDrawer();
        }
      });
    }
    if (terminalToggleBtn) {
      terminalToggleBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        toggleDrawer();
      });
    }

    const escapeHtml = (str) => {
      return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
    };

    terminalInput.setAttribute('placeholder', "Ask any question about Rodney or type 'help'...");

    const commands = {
      help: () => '<strong>Available commands & AI query modes:</strong><br>' +
                  ' • <span style="color:var(--c)">Ask anything</span>: Type any natural question about Rodney (e.g. <em>"What is Rodney\'s experience with Classic ASP?"</em> or <em>"Tell me about his role at LeadVenture"</em>)<br>' +
                  ' • <span style="color:var(--c)">about</span>: Summary of Rodney\'s background & credentials<br>' +
                  ' • <span style="color:var(--c)">skills</span>: Full technical stack breakdown<br>' +
                  ' • <span style="color:var(--c)">experience</span>: Recent career history & employers<br>' +
                  ' • <span style="color:var(--c)">contact</span>: Coordinates & direct links<br>' +
                  ' • <span style="color:var(--c)">clear</span>: Wipe terminal buffer<br>' +
                  ' • <span style="color:var(--c)">status</span>: Node diagnostic details',
      about: () => 'Rodney Amos Chery is a Technical Support Specialist III at LeadVenture and Full-Stack Developer with 3+ years of enterprise IT experience. Specializes in Classic ASP/server architecture, C#/.NET, modern JavaScript, Cloudflare, and SLA-driven production support.',
      skills: () => '<strong>Technical Skillset:</strong><br>' +
                   ' • <strong>Languages & Web:</strong> JavaScript (ES6+), C#, .NET MAUI, HTML5, CSS3/LESS, Bootstrap, Classic ASP, SQL, SQLite<br>' +
                   ' • <strong>Cloud & DevOps:</strong> Cloudflare, AWS, Git/GitHub, Dealer Spike CMS, FTP/SFTP, VS Code<br>' +
                   ' • <strong>Analytics & SEO:</strong> Chrome DevTools, GTM, GA4, Tealium, JSON-LD / Schema.org, Technical SEO<br>' +
                   ' • <strong>Enterprise:</strong> Salesforce, Jira, Active Directory, Wireshark, Incident Resolution',
      experience: () => '<strong>Career History:</strong><br>' +
                        ' • <strong>LeadVenture (June 2026 – Present):</strong> Technical Support Specialist III (Dealer Spike Tier III production support & front-end engineering)<br>' +
                        ' • <strong>Canon ITS (March 2025 – June 2026):</strong> Technical Support Specialist (Enterprise incident management)<br>' +
                        ' • <strong>Freelance Full-Stack Developer (2023 – Present):</strong> Custom web platforms & APIs<br>' +
                        ' • <strong>InCharge Debt Solutions (2022 – 2025):</strong> Client Services Professional (Salesforce CRM & compliance)<br>' +
                        ' • <strong>New Bethel Baptist Church (2025 – Present):</strong> Minister of Technology',
      contact: () => 'Email: <a href="mailto:rodney@globalrcdev.com" style="color:var(--c);text-decoration:underline;">rodney@globalrcdev.com</a><br>' +
                     'LinkedIn: <a href="https://www.linkedin.com/in/rodneyachery/" target="_blank" style="color:var(--c);text-decoration:underline;">linkedin.com/in/rodneyachery</a><br>' +
                     'GitHub: <a href="https://github.com/ChefRod88" target="_blank" style="color:var(--c);text-decoration:underline;">github.com/ChefRod88</a>',
      status: () => 'DIAGNOSTICS: SYSTEM_ONLINE // CLOUDFLARE_EDGE_ACTIVE // AI_NEURAL_UPLINK_READY // SECURE_SSL_ACTIVE',
      clear: () => {
        terminalHistory.innerHTML = '';
        return '';
      }
    };

    terminalInput.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') {
        const rawValue = terminalInput.value.trim();
        terminalInput.value = '';
        if (!rawValue) return;

        const body = terminalDrawer.querySelector('.terminal-drawer-body');
        const cmd = rawValue.toLowerCase();
        terminalHistory.innerHTML += `<p class="history-cmd">guest@rc-dev:~$ ${escapeHtml(rawValue)}</p>`;

        if (commands[cmd]) {
          const res = commands[cmd]();
          if (res) {
            terminalHistory.innerHTML += `<p class="history-res">${res}</p>`;
          }
          body.scrollTop = body.scrollHeight;
        } else {
          // Natural language question routed to AI Assistant
          const thinkingId = 'thinking-' + Date.now();
          terminalHistory.innerHTML += `<p class="history-res" id="${thinkingId}" style="color:var(--c); font-style:italic;"><span class="terminal-spinner">◐</span> RC-AI // Analyzing resume & generating response...</p>`;
          body.scrollTop = body.scrollHeight;

          fetch('/api/chat', {
            method: 'POST',
            headers: { 'content-type': 'application/json' },
            body: JSON.stringify({ message: rawValue })
          })
          .then(res => res.json())
          .then(data => {
            const el = document.getElementById(thinkingId);
            if (el) {
              if (data.ok && data.reply) {
                el.outerHTML = `<p class="history-res" style="color:var(--white); line-height:1.6;"><span style="color:var(--c); font-weight:700;">[RC-AI]:</span> ${escapeHtml(data.reply)}</p>`;
              } else {
                el.outerHTML = `<p class="history-res" style="color:#ff8888;"><span style="color:#ff5555; font-weight:700;">[RC-AI]:</span> ${escapeHtml(data.reply || data.error || 'Unable to process query.')}</p>`;
              }
            }
            body.scrollTop = body.scrollHeight;
          })
          .catch(() => {
            const el = document.getElementById(thinkingId);
            if (el) {
              el.outerHTML = `<p class="history-res" style="color:#ff8888;"><span style="color:#ff5555; font-weight:700;">[RC-AI ERROR]:</span> Network error connecting to terminal assistant. Please email rodney@globalrcdev.com directly.</p>`;
            }
            body.scrollTop = body.scrollHeight;
          });
        }
      }
    });
  }
});



// ================================
// WELCOME POPUP
// ================================
// FUTURISTIC CYBERNETIC HUD MODAL
// ================================
document.addEventListener("DOMContentLoaded", () => {
    initWelcomePopup();
});

function initWelcomePopup() {
    const storageKey = "rcdev_welcome_dismissed";
    const sevenDaysMs = 7 * 24 * 60 * 60 * 1000;
    
    // Developer & Client Demo Bypass (e.g., /?modal=1 or /?preview=modal)
    let forceModal = false;
    try {
        const urlParams = new URLSearchParams(window.location.search);
        forceModal = urlParams.has("modal") || urlParams.has("preview");
    } catch (e) {}

    if (!forceModal) {
        // Only trigger automatically on homepage to prevent interrupting deep reading on articles
        const path = window.location.pathname.replace(/\/$/, "") || "/";
        if (path !== "/" && path !== "/index.html") {
            return;
        }

        try {
            const lastDismissed = localStorage.getItem(storageKey);
            if (lastDismissed) {
                const timeSince = Date.now() - parseInt(lastDismissed, 10);
                if (timeSince < sevenDaysMs) return; // Cooldown active
            }
        } catch (e) {}
    }

    // Dynamic latency reading for authentic telemetry
    const latencyVal = Math.floor(12 + Math.random() * 7);

    // Holographic HUD HTML
    const popupHtml = `
    <div id="welcomePopup" class="rc-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="welcomePopupTitle" tabindex="-1">
        <div class="rc-hud-modal">
            <!-- Laser scanline sweep -->
            <div class="rc-hud-scan-beam"></div>

            <!-- Corner Reticles / HUD Brackets -->
            <span class="rc-hud-corner tl"></span>
            <span class="rc-hud-corner tr"></span>
            <span class="rc-hud-corner bl"></span>
            <span class="rc-hud-corner br"></span>

            <!-- Telemetry Header Bar -->
            <div class="rc-hud-telemetry-bar">
                <div class="rc-hud-status">
                    <span class="rc-hud-pulse-node"></span>
                    <span>UPLINK // ESTABLISHED</span>
                </div>
                <div class="rc-hud-telemetry-meta">
                    <span class="rc-hud-chip">4096-BIT QUANTUM</span>
                    <span class="rc-hud-chip">LATENCY: ${latencyVal}MS</span>
                    <span class="rc-hud-chip">NODE: TAMPA_FL</span>
                </div>
                <button id="closeWelcomePopup" class="rc-hud-close-btn" type="button" aria-label="Terminate interface">
                    <span>ESC // CLOSE</span>
                    <span>&times;</span>
                </button>
            </div>

            <!-- Main HUD Body -->
            <div class="rc-hud-body">
                <div class="rc-hud-subhead">// SYSTEM ANNOUNCEMENT // ARCHITECTURE UPDATE</div>
                <h2 id="welcomePopupTitle" class="rc-hud-glitch-title">WELCOME TO RC DEV // MANAGED TECH PARTNER</h2>
                <p class="rc-hud-lede">
                    We deliver high-performance web applications, cloud infrastructure, and enterprise automation with a continuous managed partnership.
                </p>

                <!-- Dual-Engine Architecture Matrix Grid -->
                <div class="rc-hud-grid">
                    <!-- Module 01 -->
                    <div class="rc-hud-card">
                        <div class="rc-hud-card-header">
                            <span class="rc-module-tag">MOD // 01</span>
                            <span class="rc-module-title">BESPOKE BUILD</span>
                        </div>
                        <p class="rc-hud-card-text">
                            Full-lifecycle engineering, custom C# .NET solutions, robust API integrations, and zero-downtime deployment pipelines built to enterprise security standards.
                        </p>
                        <div class="rc-hud-card-foot">
                            <span class="rc-hud-badge">ONE-TIME BUILD</span>
                            <span class="rc-hud-badge">MODERN CLOUD</span>
                        </div>
                    </div>

                    <!-- Module 02 -->
                    <div class="rc-hud-card highlighted">
                        <div class="rc-hud-card-header">
                            <span class="rc-module-tag">MOD // 02</span>
                            <span class="rc-module-title">MANAGED SERVICES</span>
                        </div>
                        <p class="rc-hud-card-text">
                            A dedicated technology partner after launch. Continuous SRE maintenance, SLA incident resolution, security auditing, and on-demand engineering stewardship.
                        </p>
                        <div class="rc-hud-card-foot">
                            <span class="rc-hud-badge active">LIFELONG PARTNER</span>
                            <span class="rc-hud-badge active">SRE SUPPORT</span>
                        </div>
                    </div>
                </div>

                <!-- Mission statement -->
                <div class="rc-hud-mission">
                    <span class="rc-hud-mission-icon">❖</span>
                    <span>Engineered for small and growing businesses, professional practices, and organizations that need dependable digital infrastructure and dedicated technical execution.</span>
                </div>

                <!-- Action Controls -->
                <div class="rc-hud-actions">
                    <a href="/#quote" class="rc-hud-btn primary" id="btnWelcomeQuote">
                        <span class="btn-text">INITIALIZE CONSULTATION // GET A QUOTE</span>
                        <span class="btn-arrow">➔</span>
                    </a>
                    <a href="/#services" class="rc-hud-btn secondary" id="btnWelcomeServices">
                        <span class="btn-text">EXPLORE SERVICES</span>
                    </a>
                </div>
            </div>
        </div>
    </div>`;

    document.body.insertAdjacentHTML('beforeend', popupHtml);

    const popup = document.getElementById("welcomePopup");
    const closeBtn = document.getElementById("closeWelcomePopup");
    const popupLinks = popup.querySelectorAll("a, button");
    const titleEl = document.getElementById("welcomePopupTitle");

    // Matrix Glyph Decryption Animation
    function decodeMatrixText(element, finalText, durationMs) {
        if (!element) return;
        const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789アイウエオカキクケコサシスセソタチツテト#@*&%$§";
        const startTime = performance.now();
        const length = finalText.length;

        function step(now) {
            const elapsed = now - startTime;
            const progress = Math.min(elapsed / durationMs, 1);
            const resolvedCount = Math.floor(progress * length);

            let scrambled = "";
            for (let i = 0; i < length; i++) {
                if (finalText[i] === " " || finalText[i] === "/" || i < resolvedCount) {
                    scrambled += finalText[i];
                } else {
                    scrambled += chars[Math.floor(Math.random() * chars.length)];
                }
            }
            element.textContent = scrambled;

            if (progress < 1) {
                requestAnimationFrame(step);
            } else {
                element.textContent = finalText;
            }
        }
        requestAnimationFrame(step);
    }

    setTimeout(() => {
        popup.classList.add("show");
        popup.focus();
        decodeMatrixText(titleEl, "WELCOME TO RC DEV // MANAGED TECH PARTNER", 500);
    }, 600);

    const dismissPopup = () => {
        popup.classList.remove("show");
        if (!forceModal) {
            try {
                localStorage.setItem(storageKey, Date.now().toString());
            } catch (e) {}
        }
        
        // Remove from DOM after transition
        setTimeout(() => {
            if (popup.parentNode) {
                popup.parentNode.removeChild(popup);
            }
        }, 400);
    };

    closeBtn.addEventListener("click", dismissPopup);
    popup.addEventListener("click", (e) => {
        if (e.target === popup) dismissPopup();
    });

    popupLinks.forEach(link => {
        link.addEventListener("click", dismissPopup);
    });

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape" && popup.classList.contains("show")) {
            dismissPopup();
        }
    });
}
