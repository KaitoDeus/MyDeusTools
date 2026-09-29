/**
 * MyDeusTools — Shared JavaScript Engine
 * Provides Theme Switching, Bilingual i18n, Active Nav, and Utilities
 */

const commonI18n = {
  en: {
    // Navigation
    nav_home: "Home",
    nav_features: "Features",
    nav_architecture: "Architecture",
    nav_download: "Download",
    nav_faq: "FAQ",
    nav_btn_download: "Download",
    nav_badge_v: "v1.0.0",

    // Footer
    footer_developed: "— Developed by",
    footer_github: "GitHub Repository",
    footer_docs: "Documentation",
    footer_contributors: "Contributors",
    footer_license: "MIT License",
    footer_tagline: "Free, offline, open-source desktop productivity suite for Windows 10 & 11."
  },
  vi: {
    // Navigation
    nav_home: "Trang chủ",
    nav_features: "Tính năng",
    nav_architecture: "Kiến trúc",
    nav_download: "Tải về",
    nav_faq: "Hỏi đáp",
    nav_btn_download: "Tải về",
    nav_badge_v: "v1.0.0",

    // Footer
    footer_developed: "— Phát triển bởi",
    footer_github: "Mã nguồn GitHub",
    footer_docs: "Tài liệu",
    footer_contributors: "Đóng góp",
    footer_license: "Giấy phép MIT",
    footer_tagline: "Bộ công cụ tiện ích ngoại tuyến, miễn phí và mã nguồn mở cho Windows 10 & 11."
  }
};

let pageI18n = {
  en: {},
  vi: {}
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

// Language logic
let currentLang = localStorage.getItem('mdt_lang') || 'en';

function applyTranslations(lang) {
  currentLang = lang;
  localStorage.setItem('mdt_lang', lang);

  const dict = {
    ...commonI18n[lang],
    ...(pageI18n[lang] || {})
  };

  document.querySelectorAll('[data-i18n]').forEach(el => {
    const key = el.getAttribute('data-i18n');
    if (dict[key] !== undefined) {
      el.innerHTML = dict[key];
    }
  });

  const langFlag = document.getElementById('langFlag');
  const langLabel = document.getElementById('langLabel');
  if (langFlag && langLabel) {
    langFlag.textContent = lang === 'en' ? '🇻🇳' : '🇬🇧';
    langLabel.textContent = lang === 'en' ? 'VIE' : 'ENG';
  }
}

function toggleLanguage() {
  applyTranslations(currentLang === 'en' ? 'vi' : 'en');
}

// Theme logic (Dark by default)
let currentTheme = localStorage.getItem('mdt_theme') || 'dark';

function applyTheme(theme) {
  currentTheme = theme;
  localStorage.setItem('mdt_theme', theme);
  const moonIcon = document.getElementById('themeMoonIcon');
  const sunIcon = document.getElementById('themeSunIcon');

  if (theme === 'dark') {
    document.documentElement.classList.add('dark');
    if (sunIcon) sunIcon.classList.remove('hidden');
    if (moonIcon) moonIcon.classList.add('hidden');
  } else {
    document.documentElement.classList.remove('dark');
    if (sunIcon) sunIcon.classList.add('hidden');
    if (moonIcon) moonIcon.classList.remove('hidden');
  }
}

function toggleTheme() {
  applyTheme(currentTheme === 'dark' ? 'light' : 'dark');
}

// Active Nav Link detection
function highlightActiveNav() {
  const path = window.location.pathname.toLowerCase();
  let currentFile = path.substring(path.lastIndexOf('/') + 1);
  if (!currentFile || currentFile === '') currentFile = 'index.html';

  document.querySelectorAll('.nav-link').forEach(link => {
    const href = (link.getAttribute('href') || '').toLowerCase();
    if (href === currentFile || (currentFile === 'index.html' && (href === './' || href === 'index.html' || href === ''))) {
      link.classList.add('active');
    } else {
      link.classList.remove('active');
    }
  });
}

// Copy helper with visual tooltip
function copyCommand(btn, text) {
  navigator.clipboard.writeText(text).then(() => {
    const original = btn.innerHTML;
    btn.innerHTML = `<span class="text-emerald-400 font-bold">✓</span>`;
    setTimeout(() => {
      btn.innerHTML = original;
    }, 1500);
  });
}

function copyText(btn, text, label = 'Copied!') {
  navigator.clipboard.writeText(text).then(() => {
    const original = btn.textContent;
    btn.textContent = label;
    setTimeout(() => {
      btn.textContent = original;
    }, 1500);
  });
}

// Mobile menu toggle
function toggleMobileMenu() {
  const menu = document.getElementById('mobileMenu');
  if (menu) {
    menu.classList.toggle('hidden');
  }
}

// Initialize on DOM Ready
document.addEventListener('DOMContentLoaded', () => {
  applyTheme(currentTheme);
  applyTranslations(currentLang);
  highlightActiveNav();

  const langBtn = document.getElementById('langToggleBtn');
  if (langBtn) {
    langBtn.addEventListener('click', toggleLanguage);
  }

  const themeBtn = document.getElementById('themeToggleBtn');
  if (themeBtn) {
    themeBtn.addEventListener('click', toggleTheme);
  }

  const mobileMenuBtn = document.getElementById('mobileMenuBtn');
  if (mobileMenuBtn) {
    mobileMenuBtn.addEventListener('click', toggleMobileMenu);
  }
});
