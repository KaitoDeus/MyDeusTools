import React, { useState, useEffect, useCallback } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { useTheme } from '../../contexts/ThemeContext';
import { useOsAgent } from '../../contexts/AgentContext';
import { agentClient, OsWindowInfo } from '../../services/agentClient';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiAlert,
  TuiChip,
} from '../../components/terminal/TerminalUI';

export const WindowPipView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.pip;
  const { theme } = useTheme();
  const { isOnline } = useOsAgent();

  const [tab, setTab] = useState<'web' | 'os'>(() => (isOnline ? 'os' : 'web'));
  const [windows, setWindows] = useState<OsWindowInfo[]>([]);
  const [loading, setLoading] = useState(false);

  const loadWindows = useCallback(async () => {
    if (!isOnline) return;
    setLoading(true);
    try {
      const list = await agentClient.getOsWindows();
      setWindows(list);
    } catch {
      // Ignored
    } finally {
      setLoading(false);
    }
  }, [isOnline]);

  useEffect(() => {
    if (tab === 'os' && isOnline) {
      loadWindows();
    }
  }, [tab, isOnline, loadWindows]);

  const handleToggleTopMost = async (win: OsWindowInfo) => {
    await agentClient.pinOsWindow(win.handle, !win.isTopMost);
    await loadWindows();
  };

  const handleOpacity = async (win: OsWindowInfo, pct: number) => {
    await agentClient.setOsWindowOpacity(win.handle, pct);
    await loadWindows();
  };

  const handleFocus = async (win: OsWindowInfo) => {
    await agentClient.focusOsWindow(win.handle);
  };

  const openClockPip = async () => {
    if (!('documentPictureInPicture' in window)) {
      alert(tt.unsupported);
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
          <div style="font-size:10px; color:#8b8b8b; margin-bottom:4px;">${tt.clockLabel}</div>
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
      alert(tt.unsupported);
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
          <div style="font-size:12px; font-weight:bold; color:${isDark ? '#ff6600' : '#cc4d00'}; margin-bottom:8px;">${tt.padLabel}</div>
          <textarea style="flex:1; width:100%; background:${isDark ? '#141414' : '#ffffff'}; color:${isDark ? '#e2e2e2' : '#141414'}; border:1px solid #444444; padding:8px; font-size:12px; font-family:'JetBrains Mono', monospace; resize:none; outline:none;" placeholder="${tt.padPlaceholder}"></textarea>
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
        badge={isOnline ? '[OS_AGENT:ONLINE]' : '[DOCUMENT_PiP_API]'}
        accent="tertiary"
      />

      <div className="flex flex-wrap gap-2">
        <TuiChip active={tab === 'web'} onClick={() => setTab('web')}>
          {tt.tabWebPip}
        </TuiChip>
        <TuiChip active={tab === 'os'} onClick={() => setTab('os')}>
          {tt.tabOsWindows}
        </TuiChip>
      </div>

      {tab === 'web' && (
        <div className="space-y-6">
          <TuiAlert type="info">{tt.infoBanner}</TuiAlert>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            <TuiCard
              title={tt.clockTitle}
              rightMeta={tt.topmostTag}
              bodyClassName="flex flex-col justify-between space-y-5"
            >
              <p className="text-[13px] text-[var(--tui-text-muted)] leading-[1.6]">
                {tt.clockDesc}
              </p>
              <TuiButton variant="secondary" prefix=">" onClick={openClockPip} className="w-full">
                {tt.spawnClock}
              </TuiButton>
            </TuiCard>

            <TuiCard
              title={tt.padTitle}
              rightMeta={tt.topmostTag}
              bodyClassName="flex flex-col justify-between space-y-5"
            >
              <p className="text-[13px] text-[var(--tui-text-muted)] leading-[1.6]">
                {tt.padDesc}
              </p>
              <TuiButton variant="amber" prefix=">" onClick={openScratchpadPip} className="w-full">
                {tt.spawnPad}
              </TuiButton>
            </TuiCard>
          </div>
        </div>
      )}

      {tab === 'os' && (
        <div className="space-y-4">
          {!isOnline ? (
            <TuiAlert type="warn">{tt.osOfflineWarn}</TuiAlert>
          ) : (
            <>
              <div className="flex justify-end">
                <TuiButton
                  size="sm"
                  variant="secondary"
                  prefix=">"
                  onClick={loadWindows}
                  disabled={loading}
                >
                  {tt.osRefreshBtn}
                </TuiButton>
              </div>

              <TuiCard title={`${tt.tabOsWindows} (${windows.length})`}>
                <div className="max-h-[480px] overflow-y-auto">
                  {windows.map((w, idx) => (
                    <div
                      key={w.handle}
                      className="flex flex-col md:flex-row md:items-center justify-between gap-2 py-2.5 px-2 border-b border-dashed border-[var(--tui-border-subtle)] hover:bg-[var(--tui-surface-elevated)] text-[12px]"
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <span className="text-[11px] text-[var(--tui-text-dim)] w-[3ch] text-right shrink-0">
                          {idx + 1}
                        </span>
                        <div className="min-w-0">
                          <div className="font-semibold text-[var(--tui-text)] truncate">
                            {w.title}
                          </div>
                          <div className="text-[11px] text-[var(--tui-text-dim)]">
                            {w.processName}.exe (PID:{w.processId}) | {w.opacityPercent}%
                          </div>
                        </div>
                      </div>

                      <div className="flex flex-wrap items-center gap-1.5 shrink-0">
                        <TuiButton
                          size="sm"
                          variant={w.isTopMost ? 'primary' : 'ghost'}
                          onClick={() => handleToggleTopMost(w)}
                        >
                          {w.isTopMost ? tt.osUnpin : tt.osPin}
                        </TuiButton>
                        <TuiButton size="sm" variant="secondary" onClick={() => handleFocus(w)}>
                          {tt.osFocus}
                        </TuiButton>
                        {[100, 80, 60, 40].map((pct) => (
                          <TuiButton
                            key={pct}
                            size="sm"
                            variant={Math.abs(w.opacityPercent - pct) <= 4 ? 'amber' : 'ghost'}
                            onClick={() => handleOpacity(w, pct)}
                          >
                            {pct}%
                          </TuiButton>
                        ))}
                      </div>
                    </div>
                  ))}
                  {windows.length === 0 && (
                    <div className="text-center py-8 text-[12px] text-[var(--tui-text-dim)]">
                      {tt.osEmpty}
                    </div>
                  )}
                </div>
              </TuiCard>
            </>
          )}
        </div>
      )}
    </div>
  );
};
