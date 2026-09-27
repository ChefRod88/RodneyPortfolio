// Standalone RC-SHELL Terminal Engine with Vector RAG & Typewriter Animations
document.addEventListener('DOMContentLoaded', () => {
  const terminalInput = document.getElementById('standaloneTerminalInput');
  const terminalHistory = document.getElementById('standaloneTerminalHistory');
  const terminalBody = document.getElementById('standaloneTerminalBody');
  const clearBtn = document.getElementById('terminalClearBtn');
  const helpBtn = document.getElementById('terminalHelpBtn');
  const promptChips = document.querySelectorAll('.rag-prompt-chip');

  if (!terminalInput || !terminalHistory || !terminalBody) return;

  const escapeHtml = (str) => {
    return String(str)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  };

  const renderTerminalMarkdown = (str) => {
    let safe = escapeHtml(str);
    // Convert markdown links [text](url) where url starts with / or http/https
    safe = safe.replace(/\[([^\]]+)\]\(((\/|https?:\/\/)[^\s\)]+)\)/g, (match, text, url) => {
      const isExternal = url.startsWith('http');
      const targetAttr = isExternal ? ' target="_blank" rel="noopener noreferrer"' : '';
      return `<a href="${url}"${targetAttr} style="color:var(--c);text-decoration:underline;font-weight:600;">${text}</a>`;
    });
    // Convert inline code `code`
    safe = safe.replace(/`([^`]+)`/g, '<code style="background:rgba(0,212,255,0.12);color:var(--c);padding:1px 5px;border-radius:3px;font-family:var(--font-mono);font-size:0.85em;">$1</code>');
    // Convert bold **text**
    safe = safe.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
    return safe;
  };

  const commands = {
    help: () => '<strong>RC-SHELL v1.0.4 Built-In Diagnostics & Query Modes:</strong><br>' +
                ' • <span style="color:var(--c)">Ask anything</span>: Type any natural question about Rodney\'s background, why he implemented a specific feature, or how any code in this repository works!<br>' +
                ' • <span style="color:var(--c)">codebase</span>: Repository architecture, C# clean architecture, Worker middleware & Vector RAG stats<br>' +
                ' • <span style="color:var(--c)">about</span>: Summary of Rodney\'s credentials & profile<br>' +
                ' • <span style="color:var(--c)">skills</span>: Full technical stack breakdown<br>' +
                ' • <span style="color:var(--c)">experience</span>: Recent career history & enterprise roles<br>' +
                ' • <span style="color:var(--c)">articles</span>: Published technical engineering deep-dives<br>' +
                ' • <span style="color:var(--c)">projects</span>: Key production systems & widgets<br>' +
                ' • <span style="color:var(--c)">contact</span>: Business coordinates & direct links<br>' +
                ' • <span style="color:var(--c)">clear</span>: Wipe terminal buffer<br>' +
                ' • <span style="color:var(--c)">status</span>: Edge node diagnostics & neural uplink health',
    about: () => 'Rodney Amos Chery is a Technical Support Specialist III at LeadVenture and Full-Stack Developer with 3+ years of enterprise IT experience. Specializes in Classic ASP/server architecture, C#/.NET, modern JavaScript, Cloudflare, and SLA-driven production support.',
    codebase: () => '<strong>Repository Architecture & Codebase Metrics:</strong><br>' +
                    ' • <strong>Vector RAG Store:</strong> 32 embedded multi-document & code chunks (OpenAI text-embedding-3-small, 512 dimensions)<br>' +
                    ' • <strong>Edge Infrastructure:</strong> Cloudflare Worker (V8 runtime, native rate limiting, Turnstile verification)<br>' +
                    ' • <strong>Backend Services:</strong> C# .NET 10 clean architecture (Input validation, content filtering, multi-provider LLM orchestration)<br>' +
                    ' • <strong>Frontend & UI:</strong> Cybernetic HUD, PWA service worker v3 offline caching, accessible theme inverter<br>' +
                    ' • <strong>Automation:</strong> PowerShell + WinSCP .NET assembly deployment pipelines, AST knowledge graph generator',
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
    articles: () => '<strong>Published Engineering Articles:</strong><br>' +
                    ' • <a href="/Articles/ai-assisted-support-engineering-workflow" style="color:var(--c);text-decoration:underline;font-weight:600;">How I Built an AI-Assisted Support Engineering Workflow</a> (PowerShell, WinSCP, ICM, Trust boundaries)<br>' +
                    ' • <a href="/Articles/zero-cost-aspnet-core-markdown-blog" style="color:var(--c);text-decoration:underline;font-weight:600;">Zero-Cost ASP.NET Core: Building a Markdown Blog on Cloudflare Pages</a> (Markdig, SSG crawler, SEO)',
    projects: () => '<strong>Key Engineering Projects:</strong><br>' +
                    ' • <strong>Dealer Spike Integration Widgets:</strong> Production pricing calculators, dynamic inventory filters & CTAs<br>' +
                    ' • <strong>C# / .NET RESTful APIs:</strong> Clean architecture backend services deployed with CI/CD<br>' +
                    ' • <strong>Cybernetic Portfolio HUD:</strong> High-performance, zero-latency Cloudflare Edge site with Full Codebase Vector RAG',
    contact: () => 'Email: <a href="mailto:rodney@globalrcdev.com" style="color:var(--c);text-decoration:underline;">rodney@globalrcdev.com</a><br>' +
                   'LinkedIn: <a href="https://www.linkedin.com/in/rodneyachery/" target="_blank" style="color:var(--c);text-decoration:underline;">linkedin.com/in/rodneyachery</a><br>' +
                   'GitHub: <a href="https://github.com/ChefRod88" target="_blank" style="color:var(--c);text-decoration:underline;">github.com/ChefRod88</a>',
    status: () => 'DIAGNOSTICS: SYSTEM_ONLINE // CLOUDFLARE_EDGE_ACTIVE // 32_CODE_VECTORS_LOADED // NEURAL_UPLINK_READY // SSL_STRICT_ACTIVE',
    clear: () => {
      terminalHistory.innerHTML = '';
      return '';
    }
  };

  function executeQuery(rawValue) {
    if (!rawValue) return;
    const cmd = rawValue.toLowerCase().trim();
    terminalHistory.innerHTML += `<p class="history-cmd">guest@rc-dev:~$ ${escapeHtml(rawValue)}</p>`;

    if (commands[cmd]) {
      const res = commands[cmd]();
      if (res) {
        terminalHistory.innerHTML += `<p class="history-res">${res}</p>`;
      }
      terminalBody.scrollTop = terminalBody.scrollHeight;
    } else {
      const thinkingId = 'thinking-' + Date.now();
      terminalHistory.innerHTML += `<p class="history-res" id="${thinkingId}" style="color:var(--c); font-style:italic;"><span class="terminal-spinner">◐</span> RC-AI // Vector RAG search & neural synthesis across repository...</p>`;
      terminalBody.scrollTop = terminalBody.scrollHeight;

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
            el.outerHTML = `<p class="history-res" style="color:var(--white); line-height:1.6;"><span style="color:var(--c); font-weight:700;">[RC-AI]:</span> ${renderTerminalMarkdown(data.reply)}</p>`;
          } else {
            el.outerHTML = `<p class="history-res" style="color:#ff8888;"><span style="color:#ff5555; font-weight:700;">[RC-AI]:</span> ${renderTerminalMarkdown(data.reply || data.error || 'Unable to process query.')}</p>`;
          }
        }
        terminalBody.scrollTop = terminalBody.scrollHeight;
      })
      .catch(() => {
        const el = document.getElementById(thinkingId);
        if (el) {
          el.outerHTML = `<p class="history-res" style="color:#ff8888;"><span style="color:#ff5555; font-weight:700;">[RC-AI ERROR]:</span> Network error connecting to terminal assistant. Please email rodney@globalrcdev.com directly.</p>`;
        }
        terminalBody.scrollTop = terminalBody.scrollHeight;
      });
    }
  }

  terminalInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') {
      const val = terminalInput.value.trim();
      terminalInput.value = '';
      executeQuery(val);
    }
  });

  if (clearBtn) {
    clearBtn.addEventListener('click', () => {
      commands.clear();
      terminalInput.focus();
    });
  }

  if (helpBtn) {
    helpBtn.addEventListener('click', () => {
      executeQuery('help');
      terminalInput.focus();
    });
  }

  // Typewriter simulated input for suggested chips
  let isTypingIntoInput = false;
  promptChips.forEach(chip => {
    chip.addEventListener('click', () => {
      if (isTypingIntoInput) return;
      const promptText = chip.getAttribute('data-prompt') || chip.innerText.replace(/^⚡\s*/, '').replace(/^"|"$/g, '').trim();
      if (!promptText) return;

      isTypingIntoInput = true;
      terminalInput.value = '';
      terminalInput.focus();
      terminalBody.scrollIntoView({ behavior: 'smooth', block: 'nearest' });

      let charIdx = 0;
      function typeChar() {
        if (charIdx < promptText.length) {
          terminalInput.value += promptText.charAt(charIdx);
          charIdx++;
          setTimeout(typeChar, 14);
        } else {
          setTimeout(() => {
            terminalInput.value = '';
            executeQuery(promptText);
            isTypingIntoInput = false;
          }, 150);
        }
      }
      typeChar();
    });
  });

  // ── INTRO TYPEWRITER GENERATOR ──
  function initTypewriterIntro() {
    const textEl = document.getElementById('typewriterText');
    const cursorEl = document.getElementById('typewriterCursor');
    const titleEl = document.getElementById('typewriterChipsTitle');
    const chipsContainer = document.querySelector('.rag-prompt-chips');

    if (!textEl) return;

    const part1 = "Welcome to the dedicated neural terminal for my engineering portfolio. This terminal is powered by an edge-computed Multi-Document Vector RAG engine with 512-dimension embeddings indexing my entire repository — including my C# clean architecture backend, Cloudflare Worker edge routing & rate limiters, LeadVenture production widgets, PowerShell automation pipelines, published technical articles, and client agreements.";
    const part2 = "Feel free to ask me anything about my background, why I implemented specific architectural patterns, or how any code in this repository works!";
    const part3 = "SUGGESTED TECHNICAL INQUIRIES (CLICK TO RUN):";

    let i = 0;
    let currentStage = 1;
    let isFastForwarded = false;

    function fastForward() {
      if (isFastForwarded) return;
      isFastForwarded = true;
      textEl.innerHTML = part1 + '<br><br>' + part2;
      if (titleEl) titleEl.innerText = part3;
      if (cursorEl) cursorEl.style.display = 'none';
      if (chipsContainer) chipsContainer.classList.add('visible');
    }

    const heroCard = document.querySelector('.rag-hero-card');
    if (heroCard) {
      heroCard.addEventListener('click', (e) => {
        if (!e.target.closest('.rag-prompt-chip')) {
          fastForward();
        }
      });
    }

    function typeNext() {
      if (isFastForwarded) return;

      if (currentStage === 1) {
        if (i < part1.length) {
          textEl.textContent += part1.charAt(i);
          i++;
          let delay = 9;
          const char = part1.charAt(i - 1);
          if (char === '.' || char === '—') delay = 90;
          else if (char === ',') delay = 45;
          setTimeout(typeNext, delay);
        } else {
          currentStage = 2;
          i = 0;
          textEl.innerHTML += '<br><br>';
          setTimeout(typeNext, 180);
        }
      } else if (currentStage === 2) {
        if (i < part2.length) {
          textEl.innerHTML += part2.charAt(i);
          i++;
          let delay = 9;
          const char = part2.charAt(i - 1);
          if (char === '.' || char === '!') delay = 110;
          else if (char === ',') delay = 45;
          setTimeout(typeNext, delay);
        } else {
          currentStage = 3;
          i = 0;
          if (cursorEl) cursorEl.style.display = 'none';
          setTimeout(typeNext, 160);
        }
      } else if (currentStage === 3) {
        if (titleEl && i < part3.length) {
          titleEl.textContent += part3.charAt(i);
          i++;
          setTimeout(typeNext, 12);
        } else {
          if (chipsContainer) {
            chipsContainer.classList.add('visible');
          }
        }
      }
    }

    setTimeout(typeNext, 250);
  }

  initTypewriterIntro();

  // Focus terminal input
  setTimeout(() => {
    terminalInput.focus();
  }, 200);
});
