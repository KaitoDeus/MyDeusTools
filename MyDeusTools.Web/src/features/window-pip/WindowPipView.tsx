import React from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { useTheme } from '../../contexts/ThemeContext';
import { TuiPageHeader, TuiButton, TuiCard, TuiAlert } from '../../components/terminal/TerminalUI';

export const WindowPipView: React.FC = () => {
  const { t } = useI18n();
  const { theme } = useTheme();

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

      const isDark = theme === 'dark';
      pip.document.body.innerHTML = `
        <div style="font-family: 'JetBrains Mono', monospace; display:flex; flex-direction:column; align-items:center; justify-content:center; height:100%; background:${isDark ? '#0d0d0d' : '#f4f4ee'}; color:${isDark ? '#00ff41' : '#007a1f'}; border: 1px solid ${isDark ? '#00ff41' : '#007a1f'}; margin:0; box-sizing:border-box;">
          <div id="time" style="font-size:32px; font-weight:bold;">00:00:00</div>
          <div id="date" style="font-size:12px; color:#8b8b8b; margin-top:4px;"></div>
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

      const isDark = theme === 'dark';
      pip.document.body.innerHTML = `
        <div style="font-family: 'JetBrains Mono', monospace; display:flex; flex-direction:column; height:100%; background:${isDark ? '#0d0d0d' : '#f4f4ee'}; color:${isDark ? '#e2e2e2' : '#141414'}; border: 1px solid ${isDark ? '#ff6600' : '#cc4d00'}; margin:0; padding:12px; box-sizing:border-box;">
          <div style="font-size:12px; font-weight:bold; color:${isDark ? '#ff6600' : '#cc4d00'}; margin-bottom:8px;">> FLOATING_SCRATCHPAD</div>
          <textarea style="flex:1; width:100%; background:${isDark ? '#141414' : '#ffffff'}; color:${isDark ? '#e2e2e2' : '#141414'}; border:1px solid #444444; padding:8px; font-size:12px; font-family:'JetBrains Mono', monospace; resize:none; outline:none;" placeholder="Type notes here..."></textarea>
        </div>
      `;
    } catch (e) {
      console.error(e);
    }
  };

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="pip-float"
        title={t.nav.windowPinner}
        description={t.descriptions.windowPinner}
        badge="[DOCUMENT_PiP_API]"
        accent="tertiary"
      />

      <TuiAlert type="info">
        DOCUMENT_PICTURE_IN_PICTURE: Spawns an independent OS-level always-on-top terminal widget
        above other desktop applications.
      </TuiAlert>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
        <TuiCard
          title="WIDGET_01 :: DIGITAL_CLOCK"
          rightMeta="[TOPMOST]"
          className="flex flex-col justify-between space-y-5"
        >
          <p className="text-[13px] text-[var(--tui-text-muted)] leading-[1.6]">
            Launch a compact always-on-top monospaced digital clock overlay across all windows.
          </p>
          <TuiButton variant="secondary" prefix=">" onClick={openClockPip} className="w-full">
            SPAWN_CLOCK_PiP
          </TuiButton>
        </TuiCard>

        <TuiCard
          title="WIDGET_02 :: SCRATCHPAD"
          rightMeta="[TOPMOST]"
          className="flex flex-col justify-between space-y-5"
        >
          <p className="text-[13px] text-[var(--tui-text-muted)] leading-[1.6]">
            Spawn an always-on-top monospaced notepad buffer while reading code or documentation.
          </p>
          <TuiButton variant="amber" prefix=">" onClick={openScratchpadPip} className="w-full">
            SPAWN_SCRATCHPAD_PiP
          </TuiButton>
        </TuiCard>
      </div>
    </div>
  );
};
