/**
 * Rodney Chery Portfolio - Article Reading Mode / Color Inversion Controller
 * Manages the high-contrast paper reading mode for technical articles.
 * Persists user preference via localStorage.
 */
(function () {
  'use strict';

  var STORAGE_KEY = 'rc_article_reader_mode';

  function updateToggleButtons(isReading) {
    var buttons = document.querySelectorAll('.rc-reading-toggle, #themeToggleBtn, #themeToggleBtnFloat');
    for (var i = 0; i < buttons.length; i++) {
      var btn = buttons[i];
      // Do not conflict with Agreement page's custom signature toggle if it's not .rc-reading-toggle
      if (btn.id === 'themeToggleBtn' && !btn.classList.contains('rc-reading-toggle') && document.querySelector('.agreement-page-wrapper')) {
        continue;
      }
      btn.setAttribute('aria-pressed', isReading ? 'true' : 'false');
      btn.setAttribute(
        'aria-label',
        isReading ? 'Switch back to dark matrix theme' : 'Invert colors for readability'
      );
      btn.setAttribute(
        'title',
        isReading ? 'Switch back to dark matrix theme' : 'Invert colors / Reading Mode'
      );

      var icon = btn.querySelector('.toggle-icon');
      var text = btn.querySelector('.toggle-text');
      if (icon) {
        icon.textContent = isReading ? '☀️' : '◐';
      }
      if (text) {
        text.textContent = isReading ? 'Dark Matrix' : 'Invert Colors';
      }
    }
  }

  function setReadingMode(enable, persist) {
    if (enable) {
      if (document.body) document.body.classList.add('reading-mode');
      if (document.documentElement) document.documentElement.classList.add('reading-mode');
    } else {
      if (document.body) document.body.classList.remove('reading-mode');
      if (document.documentElement) document.documentElement.classList.remove('reading-mode');
    }

    if (persist) {
      try {
        localStorage.setItem(STORAGE_KEY, enable ? 'true' : 'false');
      } catch (e) {
        // LocalStorage might be disabled in private browsing
      }
    }

    updateToggleButtons(enable);
  }

  // Expose global toggle function for inline onclick handlers and direct invocations
  window.toggleReadingMode = function () {
    var isReading = document.body ? document.body.classList.contains('reading-mode') : false;
    setReadingMode(!isReading, true);
    return !isReading;
  };

  // Immediate pre-hydration check (runs as soon as script is evaluated)
  try {
    if (localStorage.getItem(STORAGE_KEY) === 'true') {
      if (document.documentElement) document.documentElement.classList.add('reading-mode');
      if (document.body) document.body.classList.add('reading-mode');
    }
  } catch (e) {}

  function init() {
    var isReading = false;
    try {
      isReading = localStorage.getItem(STORAGE_KEY) === 'true';
    } catch (e) {}

    setReadingMode(isReading, false);

    // Direct event listener binding to all toggle buttons currently in the DOM
    var buttons = document.querySelectorAll('.rc-reading-toggle, #themeToggleBtn, #themeToggleBtnFloat');
    buttons.forEach(function (btn) {
      if (btn.id === 'themeToggleBtn' && !btn.classList.contains('rc-reading-toggle') && document.querySelector('.agreement-page-wrapper')) {
        return;
      }
      btn.addEventListener('click', function (e) {
        e.preventDefault();
        window.toggleReadingMode();
      });
    });
  }

  // Delegated event listener in capture phase to guarantee interception even if child elements are clicked
  document.addEventListener(
    'click',
    function (e) {
      var btn = e.target && e.target.closest && e.target.closest('.rc-reading-toggle, #themeToggleBtnFloat');
      if (!btn) {
        var targetBtn = e.target && e.target.closest && e.target.closest('#themeToggleBtn');
        if (targetBtn && targetBtn.classList.contains('rc-reading-toggle')) {
          btn = targetBtn;
        }
      }
      if (!btn) return;
      e.preventDefault();
      window.toggleReadingMode();
    },
    true
  );

  // Robust initialization that handles both loading and post-loading execution
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
