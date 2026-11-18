export type Language = 'vi' | 'en';

export interface Translations {
  common: {
    appName: string;
    tagline: string;
    searchPlaceholder: string;
    theme: string;
    language: string;
    allTools: string;
    favorites: string;
    pinned: string;
    actions: string;
    copy: string;
    copied: string;
    clear: string;
    delete: string;
    save: string;
    cancel: string;
    start: string;
    stop: string;
    reset: string;
    browse: string;
    dropFilesHere: string;
    privacyNotice: string;
    offlineReady: string;
    success: string;
    error: string;
    processing: string;
    download: string;
  };
  nav: {
    dashboard: string;
    autoClicker: string;
    scheduleShutdown: string;
    stickyNotes: string;
    clipboardManager: string;
    qrStudio: string;
    colorPicker: string;
    textDevTools: string;
    windowPinner: string;
    videoConverter: string;
    imageStudio: string;
    pdfToolkit: string;
    bulkRenamer: string;
    settings: string;
  };
  descriptions: {
    dashboard: string;
    autoClicker: string;
    scheduleShutdown: string;
    stickyNotes: string;
    clipboardManager: string;
    qrStudio: string;
    colorPicker: string;
    textDevTools: string;
    windowPinner: string;
    videoConverter: string;
    imageStudio: string;
    pdfToolkit: string;
    bulkRenamer: string;
  };
  categories: {
    all: string;
    productivity: string;
    developer: string;
    media: string;
    system: string;
  };
}

export const translations: Record<Language, Translations> = {
  vi: {
    common: {
      appName: 'MyDeusTools Web',
      tagline: 'Bộ Tiện Ích Đa Năng Trực Tuyến - 100% Bảo Mật & Xử Lý Tại Máy',
      searchPlaceholder: 'Tìm nhanh tiện ích (ấn Ctrl + K)...',
      theme: 'Giao diện',
      language: 'Ngôn ngữ',
      allTools: 'Tất cả công cụ',
      favorites: 'Yêu thích',
      pinned: 'Đã ghim',
      actions: 'Thao tác',
      copy: 'Sao chép',
      copied: 'Đã sao chép!',
      clear: 'Xoá tất cả',
      delete: 'Xoá',
      save: 'Lưu lại',
      cancel: 'Huỷ bỏ',
      start: 'Bắt đầu',
      stop: 'Dừng lại',
      reset: 'Đặt lại',
      browse: 'Chọn tập tin',
      dropFilesHere: 'Kéo thả tập tin vào đây',
      privacyNotice: '100% dữ liệu được xử lý trực tiếp trong trình duyệt bằng WebAssembly & Web APIs. Không một byte dữ liệu nào bị tải lên máy chủ ngoài.',
      offlineReady: 'Sẵn sàng hoạt động ngoại tuyến',
      success: 'Thành công',
      error: 'Lỗi',
      processing: 'Đang xử lý...',
      download: 'Tải về máy',
    },
    nav: {
      dashboard: 'Tổng quan công cụ',
      autoClicker: 'Auto Clicker Web',
      scheduleShutdown: 'Smart Timer & Focus',
      stickyNotes: 'Sticky Notes',
      clipboardManager: 'Clipboard Snippets',
      qrStudio: 'QR Code Studio',
      colorPicker: 'Color Eyedropper',
      textDevTools: 'Text & Dev Tools',
      windowPinner: 'Picture-in-Picture Floater',
      videoConverter: 'Video Converter (WASM)',
      imageStudio: 'Image Studio',
      pdfToolkit: 'PDF Toolkit',
      bulkRenamer: 'Bulk File Renamer',
      settings: 'Cài đặt hệ thống',
    },
    descriptions: {
      dashboard: 'Truy cập nhanh 12 công cụ đắc lực hỗ trợ công việc và lập trình.',
      autoClicker: 'Trình tự động lặp thao tác click chuột trên trang web và kiểm thử UI.',
      scheduleShutdown: 'Bộ hẹn giờ tập trung, nhắc nhở nghỉ ngơi với chuông báo & khoá màn hình.',
      stickyNotes: 'Ghi chú nổi kéo thả Kanban, phân loại màu sắc và lưu trữ tự động.',
      clipboardManager: 'Kho lưu trữ snippet bản ghi clipboard thường dùng, tìm kiếm & sao chép 1 chạm.',
      qrStudio: 'Tạo mã QR tuỳ biến màu sắc/logo và quét mã từ Camera, Ảnh dán Clipboard.',
      colorPicker: 'Trình bắt màu điểm ảnh màn hình thời gian thực (EyeDropper) & chuyển đổi HEX/RGB/HSL.',
      textDevTools: 'JSON Studio, Base64 Converter, SHA/MD5 Hash, Case Transform và JWT Inspector.',
      windowPinner: 'Ghim các widget, đồng hồ, ghi chú luôn nổi trên mọi ứng dụng (Document PiP).',
      videoConverter: 'Chuyển đổi định dạng MP4, WebM, GIF, tách nhạc MP3 thuần client với FFmpeg.wasm.',
      imageStudio: 'Nén ảnh hàng loạt, thay đổi kích thước, tạo icon .ico đa tầng, xoá sạch EXIF.',
      pdfToolkit: 'Ghép file PDF, tách trang theo dải, tạo PDF từ ảnh, bảo vệ mật khẩu 100% offline.',
      bulkRenamer: 'Đổi tên hàng loạt file trong thư mục ổ đĩa trực tiếp qua File System Access API.',
    },
    categories: {
      all: 'Tất cả (12)',
      productivity: 'Năng suất',
      developer: 'Lập trình viên',
      media: 'Đa phương tiện',
      system: 'Hệ thống',
    },
  },
  en: {
    common: {
      appName: 'MyDeusTools Web',
      tagline: 'Modern All-in-One Utility Suite - 100% Privacy & Client-Side',
      searchPlaceholder: 'Search utilities (Press Ctrl + K)...',
      theme: 'Theme',
      language: 'Language',
      allTools: 'All Tools',
      favorites: 'Favorites',
      pinned: 'Pinned',
      actions: 'Actions',
      copy: 'Copy',
      copied: 'Copied!',
      clear: 'Clear all',
      delete: 'Delete',
      save: 'Save',
      cancel: 'Cancel',
      start: 'Start',
      stop: 'Stop',
      reset: 'Reset',
      browse: 'Browse files',
      dropFilesHere: 'Drop files here',
      privacyNotice: '100% of data is processed directly inside your browser via WebAssembly & Web APIs. Zero bytes ever leave your device.',
      offlineReady: 'Offline Ready',
      success: 'Success',
      error: 'Error',
      processing: 'Processing...',
      download: 'Download',
    },
    nav: {
      dashboard: 'Dashboard',
      autoClicker: 'Auto Clicker Web',
      scheduleShutdown: 'Smart Timer & Focus',
      stickyNotes: 'Sticky Notes',
      clipboardManager: 'Clipboard Snippets',
      qrStudio: 'QR Code Studio',
      colorPicker: 'Color Eyedropper',
      textDevTools: 'Text & Dev Tools',
      windowPinner: 'Picture-in-Picture Floater',
      videoConverter: 'Video Converter (WASM)',
      imageStudio: 'Image Studio',
      pdfToolkit: 'PDF Toolkit',
      bulkRenamer: 'Bulk File Renamer',
      settings: 'Settings',
    },
    descriptions: {
      dashboard: 'Quick access to all 12 power utilities for daily productivity and engineering.',
      autoClicker: 'Automated web click loop runner, form filling, and UI stress testing.',
      scheduleShutdown: 'Focus timer, break reminder with audio chime & screen wake lock.',
      stickyNotes: 'Draggable Kanban sticky notes board with color categorization and auto-save.',
      clipboardManager: 'Reusable snippet clipboard vault, instant search, and one-click copy.',
      qrStudio: 'Custom branded QR code generator and multi-source camera/image scanner.',
      colorPicker: 'Pixel-perfect screen eyedropper HUD with HEX, RGB, HSL, HSV conversions.',
      textDevTools: 'JSON Studio, Base64 encoder, SHA/MD5 hashing, case converter, and JWT inspector.',
      windowPinner: 'Float web widgets, notes, and timers always-on-top via Document Picture-in-Picture.',
      videoConverter: 'Client-side video transcoder (MP4, WebM, GIF, MP3) powered by FFmpeg.wasm.',
      imageStudio: 'Batch image compression, resizer, multi-resolution .ico generator, EXIF stripper.',
      pdfToolkit: 'Merge PDFs, split by page ranges, images to PDF, metadata & password encryption.',
      bulkRenamer: 'Batch rename disk files directly in browser using File System Access API.',
    },
    categories: {
      all: 'All (12)',
      productivity: 'Productivity',
      developer: 'Developer',
      media: 'Media',
      system: 'System',
    },
  },
};
