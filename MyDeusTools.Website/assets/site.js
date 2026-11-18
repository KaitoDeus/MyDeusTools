/**
 * MyDeusTools — Shared TerminalUI Website Engine
 * Handles Dark/Light Theme, CRT Scanline Toggle, Bilingual EN/VI i18n, Active Nav & Copy Helpers
 */

const commonI18n = {
  en: {
    nav_home: 'Home',
    nav_features: 'Features',
    nav_architecture: 'Architecture',
    nav_download: 'Download',
    nav_faq: 'FAQ',
    nav_btn_download: '[LAUNCH_SETUP]',
    nav_badge_v: 'TTY // v1.0.0',

    footer_developed: '-- Built by',
    footer_github: '[GITHUB_REPO]',
    footer_docs: '[DOCS]',
    footer_contributors: '[CONTRIBUTORS]',
    footer_license: '[MIT_LICENSE]',
    footer_tagline:
      'Hybrid TerminalUI Web App + Windows Local Companion Agent + Native WPF Suite.',
  },
  vi: {
    nav_home: 'Trang chủ',
    nav_features: 'Tính năng',
    nav_architecture: 'Kiến trúc',
    nav_download: 'Tải về',
    nav_faq: 'Hỏi đáp',
    nav_btn_download: '[CÀI ĐẶT_NHANH]',
    nav_badge_v: 'TTY // v1.0.0',

    footer_developed: '-- Phát triển bởi',
    footer_github: '[KHO_GITHUB]',
    footer_docs: '[TÀI_LIỆU]',
    footer_contributors: '[ĐÓNG_GÓP]',
    footer_license: '[GIẤY_PHÉP_MIT]',
    footer_tagline:
      'Bộ công cụ lai TerminalUI Web App + Windows Local Companion Agent + Native WPF.',
  },
};

const pageI18n = {
  en: {},
  vi: {},
};

function registerTranslations(extraData) {
  if (extraData && extraData.en) {
    Object.assign(pageI18n.en, extraData.en);
  }
  if (extraData && extraData.vi) {
    Object.assign(pageI18n.vi, extraData.vi);
  }
  applyTranslations(currentLang);
}

// Language preference
let currentLang = localStorage.getItem('mdt_lang') || 'en';

function applyTranslations(lang) {
  currentLang = lang;
  localStorage.setItem('mdt_lang', lang);
  document.documentElement.lang = lang;

  const dict = {
    ...commonI18n[lang],
    ...(pageI18n[lang] || {}),
  };

  document.querySelectorAll('[data-i18n]').forEach((el) => {
    const key = el.getAttribute('data-i18n');
    if (dict[key] !== undefined) {
      el.innerHTML = dict[key];
    }
  });

  const langBtn = document.getElementById('langToggleBtn');
  if (langBtn) {
    langBtn.textContent = lang === 'en' ? '[LANG:EN]' : '[NGÔN NGỮ:VI]';
  }
}

function toggleLanguage() {
  applyTranslations(currentLang === 'en' ? 'vi' : 'en');
}

// Theme preference (Dark #0D0D0D default vs Light #F4F4EE TTY Paper)
let currentTheme = localStorage.getItem('mdt_theme') || 'dark';

function applyTheme(theme) {
  currentTheme = theme;
  localStorage.setItem('mdt_theme', theme);
  const root = document.documentElement;

  if (theme === 'dark') {
    root.classList.add('dark');
  } else {
    root.classList.remove('dark');
  }
  root.style.colorScheme = theme;

  const themeBtn = document.getElementById('themeToggleBtn');
  if (themeBtn) {
    themeBtn.textContent = theme === 'dark' ? '[THEME:DARK]' : '[THEME:LIGHT]';
  }
}

function toggleTheme() {
  applyTheme(currentTheme === 'dark' ? 'light' : 'dark');
}

// Optional CRT Scanlines Overlay
let crtEnabled = localStorage.getItem('mdt_crt') !== 'false';

function applyCrt(enabled) {
  crtEnabled = enabled;
  localStorage.setItem('mdt_crt', String(enabled));
  let overlay = document.getElementById('crtOverlay');
  if (!overlay) {
    overlay = document.createElement('div');
    overlay.id = 'crtOverlay';
    overlay.className = 'fixed inset-0 z-[100] tui-scanlines';
    overlay.setAttribute('aria-hidden', 'true');
    document.body.prepend(overlay);
  }
  overlay.style.display = enabled ? 'block' : 'none';

  const crtBtn = document.getElementById('crtToggleBtn');
  if (crtBtn) {
    crtBtn.textContent = enabled ? '[CRT:ON]' : '[CRT:OFF]';
    if (enabled) {
      crtBtn.style.borderColor = 'var(--tui-primary)';
      crtBtn.style.color = 'var(--tui-primary)';
    } else {
      crtBtn.style.borderColor = 'var(--tui-border-medium)';
      crtBtn.style.color = 'var(--tui-text-muted)';
    }
  }
}

function toggleCrt() {
  applyCrt(!crtEnabled);
}

// Active Nav highlighting
function highlightActiveNav() {
  const path = window.location.pathname.toLowerCase();
  let currentFile = path.substring(path.lastIndexOf('/') + 1);
  if (!currentFile || currentFile === '') currentFile = 'index.html';

  document.querySelectorAll('.nav-link').forEach((link) => {
    const href = (link.getAttribute('href') || '').toLowerCase();
    if (
      href === currentFile ||
      (currentFile === 'index.html' && (href === './' || href === 'index.html' || href === ''))
    ) {
      link.classList.add('active');
    } else {
      link.classList.remove('active');
    }
  });
}

// Copy command helper
function copyCommand(btn, text) {
  navigator.clipboard.writeText(text).then(() => {
    const original = btn.textContent;
    btn.textContent = '[OK]';
    setTimeout(() => {
      btn.textContent = original;
    }, 1500);
  });
}

function toggleMobileMenu() {
  const menu = document.getElementById('mobileMenu');
  if (menu) {
    menu.classList.toggle('hidden');
  }
}

document.addEventListener('DOMContentLoaded', () => {
  applyTheme(currentTheme);
  applyCrt(crtEnabled);
  applyTranslations(currentLang);
  highlightActiveNav();

  const langBtn = document.getElementById('langToggleBtn');
  if (langBtn) langBtn.addEventListener('click', toggleLanguage);

  const themeBtn = document.getElementById('themeToggleBtn');
  if (themeBtn) themeBtn.addEventListener('click', toggleTheme);

  const crtBtn = document.getElementById('crtToggleBtn');
  if (crtBtn) crtBtn.addEventListener('click', toggleCrt);

  const mobileMenuBtn = document.getElementById('mobileMenuBtn');
  if (mobileMenuBtn) mobileMenuBtn.addEventListener('click', toggleMobileMenu);
});
