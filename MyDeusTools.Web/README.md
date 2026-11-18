# MyDeusTools Web - All-in-One Utility Suite

> **Pure Web & PWA Architecture (100% Client-Side Privacy)**  
> Built with **React 19**, **Vite**, **TypeScript**, and **Tailwind CSS**.

---

## 🌟 Overview

**MyDeusTools Web** brings the complete 12-utility experience of MyDeusTools to the browser. Designed with a **Privacy-First** architecture:
- **Zero Server Uploads**: 100% of video processing, image compression, PDF merging/splitting, text hashing, and file renaming happens locally in your device's browser memory via **WebAssembly**, **Web Workers**, and modern **Web APIs**.
- **No Installation Required**: Runs smoothly in any modern browser (Chrome, Edge, Firefox, Safari).
- **Offline-Ready PWA**: Can be installed to desktop or mobile like a native app.
- **Bilingual**: Seamless instant switching between Vietnamese (`vi`) and English (`en`).

---

## 🛠️ The 12 Web Utilities

1. **Dashboard Overview**: Central search hub (`Ctrl + K`), category filters, and quick tool launcher.
2. **Auto Clicker Web**: DOM click loop runner, CPS speed meter, target simulator, and draggable bookmarklet for any website.
3. **Smart Focus & Sleep Timer**: Focus & Pomodoro countdown with automatic human time normalization (e.g. 70m -> 1h 10m), Screen Wake Lock API to prevent display sleep, and audio chime alerts.
4. **Sticky Notes**: Colorful Kanban/Canvas board with pin-to-top, full text search, auto-saving into IndexedDB, and **Document Picture-in-Picture** floating window support!
5. **Clipboard & Snippet Vault**: Reusable code/text snippet manager with tag categorization, usage counters, and 1-click clipboard copying.
6. **QR Code Studio**: Customizable foreground/background color QR code generator with PNG export, and multi-source QR scanner (Camera, File, Clipboard paste).
7. **Color Eyedropper**: Native Web **EyeDropper API** to pick any pixel color from your entire screen, HEX/RGB/HSL/HSV auto-conversions, and persistent palette history in IndexedDB.
8. **Text & Dev Tools**: JSON Studio (format 2-spaces, minify, validate), Base64 converter (text & file), Web Crypto SHA-1/SHA-256/SHA-512 hashing, case conversion, and URL/HTML entity escaping.
9. **Window PiP Floater**: Leverages modern **Document Picture-in-Picture** to pop out always-on-top floating clocks and scratchpad notes over other desktop applications.
10. **Video Converter (WASM)**: Client-side video transcoder (MP4, WebM, GIF) and MP3 audio extractor powered by WebAssembly.
11. **Image Studio**: Squoosh-style batch image compression (WebP, JPEG, PNG), resolution scaling, and privacy EXIF/GPS stripping.
12. **PDF Toolkit**: 100% offline PDF merging, page range splitting (`1-3, 5`), and document management powered by `pdf-lib`.
13. **Bulk File Renamer**: Batch renaming using the **File System Access API** (`showDirectoryPicker`) to rename disk files directly, or rename uploaded batches with ZIP export.

---

## 🚀 Quick Start

### Prerequisites
- Node.js `v20+` or `v22+`
- npm `10+`

### Development Server
```bash
# Navigate to the web project
cd MyDeusTools.Web

# Install dependencies (first time only)
npm install

# Start development server on http://localhost:3000
npm run dev
```

Or from the root directory:
```bash
npm run web:dev
```

### Production Build
```bash
npm run build
```
Generates an optimized, code-split static build into `dist/` ready to be hosted on GitHub Pages, Cloudflare Pages, Vercel, or Netlify with zero server costs!
