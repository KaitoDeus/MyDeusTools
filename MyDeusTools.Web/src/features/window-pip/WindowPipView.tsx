import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  PictureInPicture2,
  ExternalLink,
  Clock,
  StickyNote,
  Timer,
  Sparkles,
  ShieldCheck,
} from 'lucide-react';

export const WindowPipView: React.FC = () => {
  const { t } = useI18n();
  const [isSupported] = useState(() => 'documentPictureInPicture' in window);

  const openClockPip = async () => {
    if (!('documentPictureInPicture' in window)) {
      alert('Document Picture-in-Picture is supported in Chromium browsers (Chrome, Edge, Opera).');
      return;
    }
    try {
      // @ts-ignore
      const pip = await window.documentPictureInPicture.requestWindow({
        width: 280,
        height: 180,
      });

      pip.document.body.innerHTML = `
        <div style="font-family: monospace; display:flex; flex-direction:column; align-items:center; justify-content:center; height:100%; background:#0f172a; color:#38bdf8; margin:0;">
          <div id="time" style="font-size:32px; font-weight:bold;">00:00:00</div>
          <div id="date" style="font-size:12px; color:#94a3b8; margin-top:4px;"></div>
        </div>
      `;

      const update = () => {
        const now = new Date();
        const timeEl = pip.document.getElementById('time');
        const dateEl = pip.document.getElementById('date');
        if (timeEl) timeEl.innerText = now.toLocaleTimeString();
        if (dateEl) dateEl.innerText = now.toLocaleDateString();
      };
      update();
      setInterval(update, 1000);
    } catch (e) {
      console.error(e);
    }
  };

  const openScratchpadPip = async () => {
    if (!('documentPictureInPicture' in window)) {
      alert('Document Picture-in-Picture is supported in modern Chrome/Edge browsers.');
      return;
    }
    try {
      // @ts-ignore
      const pip = await window.documentPictureInPicture.requestWindow({
        width: 320,
        height: 320,
      });

      pip.document.body.innerHTML = `
        <div style="font-family: sans-serif; display:flex; flex-direction:column; height:100%; background:#0f172a; color:#f8fafc; margin:0; padding:12px; box-sizing:border-box;">
          <div style="font-size:12px; font-weight:bold; color:#fbbf24; margin-bottom:8px;">Floating Quick Note</div>
          <textarea style="flex:1; width:100%; background:#1e293b; color:#f8fafc; border:1px solid #334155; border-radius:8px; padding:8px; font-size:12px; font-family:monospace; resize:none; outline:none;" placeholder="Type notes here..."></textarea>
        </div>
      `;
    } catch (e) {
      console.error(e);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-white flex items-center gap-2">
          <PictureInPicture2 className="w-6 h-6 text-indigo-400" />
          <span>{t.nav.windowPinner}</span>
        </h2>
        <p className="text-sm text-slate-400 mt-1">{t.descriptions.windowPinner}</p>
      </div>

      <div className="p-4 bg-indigo-500/10 border border-indigo-500/20 rounded-2xl flex items-center gap-3 text-xs text-indigo-300">
        <Sparkles className="w-5 h-5 text-indigo-400 shrink-0" />
        <span>
          <strong>Document Picture-in-Picture API:</strong> Ghim bất kỳ tiện ích web nào thành một cửa sổ nổi độc lập luôn nằm trên cùng (Always-on-Top) trên màn hình làm việc của Windows/macOS.
        </span>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
        {/* Widget 1: Floating Clock */}
        <div className="glass-card p-6 rounded-2xl border border-slate-800 space-y-4 flex flex-col justify-between">
          <div className="space-y-2">
            <div className="w-10 h-10 rounded-xl bg-sky-500/10 text-sky-400 flex items-center justify-center">
              <Clock className="w-5 h-5" />
            </div>
            <h3 className="font-semibold text-slate-100">Always-on-Top Desktop Clock</h3>
            <p className="text-xs text-slate-400 leading-relaxed">
              Mở đồng hồ kỹ thuật số nổi nhỏ gọn luôn hiển thị trên các ứng dụng khác khi làm việc.
            </p>
          </div>
          <button
            onClick={openClockPip}
            className="w-full py-2.5 bg-sky-600 hover:bg-sky-500 text-white rounded-xl text-xs font-semibold transition-all flex items-center justify-center gap-2 cursor-pointer shadow-lg shadow-sky-600/20"
          >
            <ExternalLink className="w-4 h-4" />
            <span>Launch Floating Clock</span>
          </button>
        </div>

        {/* Widget 2: Floating Scratchpad */}
        <div className="glass-card p-6 rounded-2xl border border-slate-800 space-y-4 flex flex-col justify-between">
          <div className="space-y-2">
            <div className="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-400 flex items-center justify-center">
              <StickyNote className="w-5 h-5" />
            </div>
            <h3 className="font-semibold text-slate-100">Floating Scratchpad</h3>
            <p className="text-xs text-slate-400 leading-relaxed">
              Cửa sổ ghi chú nổi độc lập, tiện lợi để lưu nháp văn bản khi đối chiếu code hoặc dữ liệu.
            </p>
          </div>
          <button
            onClick={openScratchpadPip}
            className="w-full py-2.5 bg-amber-600 hover:bg-amber-500 text-white rounded-xl text-xs font-semibold transition-all flex items-center justify-center gap-2 cursor-pointer shadow-lg shadow-amber-600/20"
          >
            <ExternalLink className="w-4 h-4" />
            <span>Launch Floating Note</span>
          </button>
        </div>
      </div>
    </div>
  );
};
