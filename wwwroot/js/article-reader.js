/**
 * Rodney Chery Portfolio - Article Reading Mode / Color Inversion Controller
 * Manages the high-contrast paper reading mode for technical articles.
 * Persists user preference via localStorage.
 */
(function () {
  'use strict';

  var STORAGE_KEY = 'rc_article_reader_mode';

  function updateToggleButtons(isReading) {
    var buttons = document.querySelectorAll('.rc-reading-toggle');
    for (var i = 0; i < buttons.length; i++) {
      var btn = buttons[i];
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
      document.body.classList.add('reading-mode');
    } else {
      document.body.classList.remove('reading-mode');
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

  // Pre-hydration check (runs if body already exists)
  try {
    if (localStorage.getItem(STORAGE_KEY) === 'true' && document.body) {
      document.body.classList.add('reading-mode');
    }
  } catch (e) {}

  document.addEventListener('DOMContentLoaded', function () {
    var isReading = false;
    try {
      isReading = localStorage.getItem(STORAGE_KEY) === 'true';
    } catch (e) {}

    setReadingMode(isReading, false);

    document.addEventListener('click', function (e) {
      var btn = e.target.closest('.rc-reading-toggle');
      if (!btn) return;
      e.preventDefault();

      var currentlyReading = document.body.classList.contains('reading-mode');
      setReadingMode(!currentlyReading, true);
    });
  });
})();
