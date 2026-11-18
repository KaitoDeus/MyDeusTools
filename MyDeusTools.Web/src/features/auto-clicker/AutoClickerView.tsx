import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { useOsAgent } from '../../contexts/AgentContext';
import { agentClient, OsAutoClickStatus } from '../../services/agentClient';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiSelect,
  TuiChip,
  TuiAlert,
  TuiCheckbox,
} from '../../components/terminal/TerminalUI';

type ClickerMode = 'web' | 'os';

export const AutoClickerView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.clicker;
  const { isOnline, checking, refreshAgentStatus } = useOsAgent();

  // Default to OS tab automatically if Agent is already connected
  const [mode, setMode] = useState<ClickerMode>(() => (isOnline ? 'os' : 'web'));
  const [copiedCmd, setCopiedCmd] = useState(false);

  // --- Shared Settings ---
  const [intervalMs, setIntervalMs] = useState(100);
  const [repeatMode, setRepeatMode] = useState<'infinite' | 'fixed'>('infinite');
  const [fixedCount, setFixedCount] = useState(100);

  // --- Web DOM Sandbox State ---
  const [isRunning, setIsRunning] = useState(false);
  const [clicksDone, setClicksDone] = useState(0);
  const [cps, setCps] = useState(0);
  const [copiedBookmarklet, setCopiedBookmarklet] = useState(false);
  const [targetHits, setTargetHits] = useState(0);
  const targetBtnRef = useRef<HTMLButtonElement>(null);
  const timerRef = useRef<any>(null);

  // --- OS Native Agent State ---
  const [osButton, setOsButton] = useState<'left' | 'right' | 'middle'>('left');
  const [osClickType, setOsClickType] = useState<'single' | 'double'>('single');
  const [useRecordedPoints, setUseRecordedPoints] = useState(true);
  const [osStatus, setOsStatus] = useState<OsAutoClickStatus | null>(null);
  const [captureCountdown, setCaptureCountdown] = useState<number | null>(null);
  const [osError, setOsError] = useState<string | null>(null);

  // Switch automatically when agent first comes online if user hasn't started web loop
  useEffect(() => {
    if (isOnline && !isRunning) {
      setMode('os');
    }
  }, [isOnline]);

  // Poll OS Agent status when OS tab is open & agent is online
  useEffect(() => {
    if (mode !== 'os' || !isOnline) return;
    let active = true;

    const poll = async () => {
      try {
        const s = await agentClient.getAutoClickStatus();
        if (active) {
          setOsStatus(s);
          setOsError(null);
        }
      } catch {
        // Ignore transient poll errors
      }
    };

    poll();
    const id = setInterval(poll, 300);
    return () => {
      active = false;
      clearInterval(id);
    };
  }, [mode, isOnline]);

  // --- Web Loop Handlers ---
  const handleStartWeb = () => {
    setIsRunning(true);
    setClicksDone(0);
    setCps(Math.round(1000 / Math.max(10, intervalMs)));
  };

  const handleStopWeb = () => {
    setIsRunning(false);
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = null;
    }
    setCps(0);
  };

  useEffect(() => {
    if (isRunning) {
      const clampedInterval = Math.max(10, intervalMs);
      timerRef.current = setInterval(() => {
        setClicksDone((prev) => {
          const next = prev + 1;
          if (targetBtnRef.current) {
            targetBtnRef.current.click();
          }
          if (repeatMode === 'fixed' && next >= fixedCount) {
            handleStopWeb();
          }
          return next;
        });
      }, clampedInterval);
    }
    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, [isRunning, intervalMs, repeatMode, fixedCount]);

  // --- OS Agent Handlers ---
  const handleStartOs = async () => {
    setOsError(null);
    try {
      const res = await agentClient.startOsAutoClick({
        intervalMs,
        button: osButton,
        clickType: osClickType,
        repeatCount: repeatMode === 'fixed' ? fixedCount : 0,
        useRecordedPoints,
      });
      setOsStatus(res);
    } catch (err: any) {
      setOsError(err?.message || 'Agent request failed');
      refreshAgentStatus();
    }
  };

  const handleStopOs = async () => {
    try {
      const res = await agentClient.stopOsAutoClick();
      setOsStatus(res);
    } catch (err: any) {
      setOsError(err?.message || 'Agent request failed');
    }
  };

  const handleCaptureIn3s = async () => {
    if (captureCountdown !== null) return;
    setCaptureCountdown(3);
    const countdownTimer = setInterval(() => {
      setCaptureCountdown((c) => (c && c > 1 ? c - 1 : null));
    }, 1000);

    try {
      const res = await agentClient.captureCursorPoint(3);
      setOsStatus(res.status);
    } catch (err: any) {
      setOsError(err?.message || 'Capture failed');
    } finally {
      clearInterval(countdownTimer);
      setCaptureCountdown(null);
    }
  };

  const handleClearPoints = async () => {
    try {
      const res = await agentClient.clearCapturedPoints();
      setOsStatus(res);
    } catch (err: any) {
      setOsError(err?.message || 'Clear points failed');
    }
  };

  const bookmarkletCode = `javascript:(function(){let interval=prompt('Enter click interval (ms):','100');if(!interval)return;let active=true;let count=0;alert('Click on any element to start clicking it! Press Escape anytime to stop.');window.addEventListener('click',function handler(e){window.removeEventListener('click',handler);let el=e.target;let t=setInterval(function(){if(!active){clearInterval(t);return;}el.click();count++;},parseInt(interval)||100);window.addEventListener('keydown',function kh(ev){if(ev.key==='Escape'){active=false;clearInterval(t);window.removeEventListener('keydown',kh);alert('Auto-clicker stopped after '+count+' clicks.');}});});})();`;

  const copyBookmarklet = () => {
    navigator.clipboard.writeText(bookmarkletCode);
    setCopiedBookmarklet(true);
    setTimeout(() => setCopiedBookmarklet(false), 2000);
  };

  const launchCommand = 'dotnet run --project MyDeusTools.Agent';
  const copyLaunchCommand = () => {
    navigator.clipboard.writeText(launchCommand);
    setCopiedCmd(true);
    setTimeout(() => setCopiedCmd(false), 2000);
  };

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="auto-click"
        title={t.nav.autoClicker}
        description={t.descriptions.autoClicker}
        badge={isOnline ? '[OS_AGENT:ONLINE]' : '[DOM_MACRO]'}
      />

      {/* Mode Switcher Tabs */}
      <div className="flex flex-wrap gap-2">
        <TuiChip active={mode === 'web'} onClick={() => setMode('web')}>
          {tt.tabWebDom}
        </TuiChip>
        <TuiChip active={mode === 'os'} onClick={() => setMode('os')}>
          {tt.tabOsAgent}
        </TuiChip>
      </div>

      {/* MODE 1: WEB DOM SANDBOX */}
      {mode === 'web' && (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          <TuiCard title={tt.configTitle} className="lg:col-span-7 p-6" bodyClassName="space-y-5">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <TuiInput
                label={tt.intervalLabel}
                type="number"
                min={10}
                value={intervalMs}
                disabled={isRunning}
                onChange={(e) => setIntervalMs(Math.max(10, parseInt(e.target.value) || 10))}
                helperText={`${tt.ratePrefix} ~${Math.round(1000 / Math.max(10, intervalMs))} ${tt.cpsSuffix}`}
              />

              <TuiSelect
                label={tt.repeatModeLabel}
                value={repeatMode}
                disabled={isRunning}
                onChange={(e) => setRepeatMode(e.target.value as any)}
              >
                <option value="infinite">{tt.repeatInfinite}</option>
                <option value="fixed">{tt.repeatFixed}</option>
              </TuiSelect>
            </div>

            {repeatMode === 'fixed' && (
              <TuiInput
                label={tt.maxCountLabel}
                type="number"
                min={1}
                value={fixedCount}
                disabled={isRunning}
                onChange={(e) => setFixedCount(Math.max(1, parseInt(e.target.value) || 1))}
              />
            )}

            <div className="flex flex-wrap items-center gap-3 pt-2">
              {!isRunning ? (
                <TuiButton variant="primary" prefix=">" onClick={handleStartWeb}>
                  {tt.startLoop}
                </TuiButton>
              ) : (
                <TuiButton variant="destructive" prefix="!" onClick={handleStopWeb}>
                  {tt.abortLoop}
                </TuiButton>
              )}

              <TuiButton
                variant="secondary"
                onClick={() => {
                  handleStopWeb();
                  setClicksDone(0);
                  setTargetHits(0);
                }}
              >
                [{t.common.reset.toUpperCase()}]
              </TuiButton>
            </div>

            <div className="p-4 bg-[var(--tui-bg)] border border-[var(--tui-border-subtle)] space-y-2">
              <div className="flex items-center justify-between gap-2">
                <span className="text-[12px] font-bold text-[var(--tui-primary)]">
                  {tt.bookmarkletTitle}
                </span>
                <TuiButton size="sm" variant="ghost" onClick={copyBookmarklet}>
                  {copiedBookmarklet ? `[${t.common.copied.toUpperCase()}]` : tt.copyJs}
                </TuiButton>
              </div>
              <p className="text-[11px] text-[var(--tui-text-muted)] leading-[1.5]">
                {tt.bookmarkletHintPrefix}{' '}
                <span className="text-[var(--tui-primary)] font-bold">[ESC]</span>{' '}
                {tt.bookmarkletHintSuffix}
              </p>
            </div>
          </TuiCard>

          <TuiCard
            title={tt.sandboxTitle}
            rightMeta={`${tt.dispatchedPrefix} ${clicksDone}`}
            elevated
            className="lg:col-span-5 p-6"
            bodyClassName="flex flex-col items-center justify-center py-4 space-y-6 text-center"
          >
            <div className="space-y-1">
              <div className="text-[11px] uppercase tracking-[0.1em] text-[var(--tui-text-muted)]">
                // {tt.registeredLabel}
              </div>
              <div className="text-[48px] font-bold text-[var(--tui-primary)] leading-none py-1">
                {targetHits}
              </div>
              <div className="text-[12px] text-[var(--tui-text-dim)]">
                {tt.speedPrefix} {cps} CPS
              </div>
            </div>

            <button
              ref={targetBtnRef}
              type="button"
              onClick={() => setTargetHits((h) => h + 1)}
              className="w-44 h-28 border-2 border-[var(--tui-primary)] bg-[var(--tui-bg)] hover:bg-[var(--tui-primary)] hover:text-[var(--tui-primary-fg)] text-[var(--tui-primary)] font-bold flex flex-col items-center justify-center gap-1 cursor-pointer select-none"
            >
              <span className="text-[16px]">[ + ]</span>
              <span className="text-[12px] tracking-[0.06em] uppercase">{tt.hitTarget}</span>
            </button>

            <p className="text-[11px] text-[var(--tui-text-dim)] max-w-xs">{tt.sandboxHint}</p>
          </TuiCard>
        </div>
      )}

      {/* MODE 2: OS NATIVE COMPANION AGENT */}
      {mode === 'os' && (
        <div className="space-y-6">
          {!isOnline ? (
            <TuiCard
              title={tt.osLaunchGuideTitle}
              rightMeta={<TuiChip tone="error">[AGENT:OFFLINE]</TuiChip>}
              className="p-6"
              bodyClassName="space-y-5"
            >
              <TuiAlert type="warn">{tt.osAgentOfflineWarn}</TuiAlert>

              <div className="space-y-2">
                <div className="text-[12px] text-[var(--tui-text-muted)]">// {tt.osCmdLabel}</div>
                <div className="flex flex-wrap items-center justify-between gap-2 p-3 bg-[var(--tui-bg)] border border-[var(--tui-border-medium)] text-[13px]">
                  <code className="text-[var(--tui-primary)] font-bold select-all">
                    $ {launchCommand}
                  </code>
                  <TuiButton size="sm" variant="secondary" onClick={copyLaunchCommand}>
                    {copiedCmd ? `[${t.common.copied.toUpperCase()}]` : `[${t.common.copy.toUpperCase()}]`}
                  </TuiButton>
                </div>
                <div className="text-[11px] text-[var(--tui-text-dim)]">
                  OR: <code className="text-[var(--tui-text-muted)]">$ npm run agent:dev</code>
                </div>
              </div>

              <div className="pt-2">
                <TuiButton
                  variant="primary"
                  prefix=">"
                  onClick={() => refreshAgentStatus()}
                  disabled={checking}
                >
                  {checking ? tt.osChecking : tt.osCheckBtn}
                </TuiButton>
              </div>
            </TuiCard>
          ) : (
            <>
              <TuiAlert type="info">{tt.osHotkeysBanner}</TuiAlert>
              {osError && <TuiAlert type="err">{osError}</TuiAlert>}

              <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
                {/* Win32 Controls (7 cols) */}
                <TuiCard
                  title={tt.osConfigTitle}
                  rightMeta={<TuiChip tone="primary">[WIN32_READY]</TuiChip>}
                  className="lg:col-span-7 p-6"
                  bodyClassName="space-y-5"
                >
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <TuiInput
                      label={tt.intervalLabel}
                      type="number"
                      min={10}
                      value={intervalMs}
                      disabled={osStatus?.isRunning}
                      onChange={(e) => setIntervalMs(Math.max(10, parseInt(e.target.value) || 10))}
                      helperText={`${tt.ratePrefix} ~${Math.round(1000 / Math.max(10, intervalMs))} ${tt.cpsSuffix}`}
                    />

                    <TuiSelect
                      label={tt.repeatModeLabel}
                      value={repeatMode}
                      disabled={osStatus?.isRunning}
                      onChange={(e) => setRepeatMode(e.target.value as any)}
                    >
                      <option value="infinite">{tt.repeatInfinite}</option>
                      <option value="fixed">{tt.repeatFixed}</option>
                    </TuiSelect>

                    <TuiSelect
                      label={tt.mouseBtnLabel}
                      value={osButton}
                      disabled={osStatus?.isRunning}
                      onChange={(e) => setOsButton(e.target.value as any)}
                    >
                      <option value="left">{tt.mouseButtons.left}</option>
                      <option value="right">{tt.mouseButtons.right}</option>
                      <option value="middle">{tt.mouseButtons.middle}</option>
                    </TuiSelect>

                    <TuiSelect
                      label={tt.clickTypeLabel}
                      value={osClickType}
                      disabled={osStatus?.isRunning}
                      onChange={(e) => setOsClickType(e.target.value as any)}
                    >
                      <option value="single">{tt.clickTypes.single}</option>
                      <option value="double">{tt.clickTypes.double}</option>
                    </TuiSelect>
                  </div>

                  {repeatMode === 'fixed' && (
                    <TuiInput
                      label={tt.maxCountLabel}
                      type="number"
                      min={1}
                      value={fixedCount}
                      disabled={osStatus?.isRunning}
                      onChange={(e) => setFixedCount(Math.max(1, parseInt(e.target.value) || 1))}
                    />
                  )}

                  <TuiCheckbox
                    checked={useRecordedPoints}
                    disabled={osStatus?.isRunning}
                    onChange={setUseRecordedPoints}
                    label={tt.osReplayCheckbox}
                  />

                  <div className="flex flex-wrap items-center gap-3 pt-2 border-t border-dashed border-[var(--tui-border-subtle)]">
                    {!osStatus?.isRunning ? (
                      <TuiButton variant="primary" size="lg" prefix=">" onClick={handleStartOs}>
                        {tt.osStartBtn}
                      </TuiButton>
                    ) : (
                      <TuiButton variant="destructive" size="lg" prefix="!" onClick={handleStopOs}>
                        {tt.osStopBtn}
                      </TuiButton>
                    )}

                    <TuiButton
                      variant="secondary"
                      onClick={handleCaptureIn3s}
                      disabled={captureCountdown !== null || osStatus?.isRunning}
                    >
                      {captureCountdown !== null
                        ? tt.osCapturingCountdown.replace('{s}', String(captureCountdown))
                        : `[${tt.osCapture3s}]`}
                    </TuiButton>
                  </div>
                </TuiCard>

                {/* Live Screen Telemetry & Coordinates (5 cols) */}
                <TuiCard
                  title={tt.osStatusCardTitle}
                  rightMeta={
                    <TuiChip tone={osStatus?.isRunning ? 'primary' : 'muted'}>
                      {osStatus?.isRunning ? 'RUNNING' : 'IDLE'}
                    </TuiChip>
                  }
                  elevated
                  className="lg:col-span-5 p-6"
                  bodyClassName="space-y-5"
                >
                  <div className="grid grid-cols-2 gap-3 text-center">
                    <div className="p-3 bg-[var(--tui-bg)] border border-[var(--tui-border-subtle)]">
                      <div className="text-[11px] text-[var(--tui-text-muted)]">
                        {tt.osClicksSentLabel}
                      </div>
                      <div className="text-[28px] font-bold text-[var(--tui-primary)] mt-1">
                        {osStatus?.clicksDone ?? 0}
                      </div>
                    </div>

                    <div className="p-3 bg-[var(--tui-bg)] border border-[var(--tui-border-subtle)]">
                      <div className="text-[11px] text-[var(--tui-text-muted)]">
                        {tt.osCursorLabel}
                      </div>
                      <div className="text-[14px] font-bold text-[var(--tui-tertiary)] mt-2.5">
                        X:{osStatus?.cursorX ?? 0} | Y:{osStatus?.cursorY ?? 0}
                      </div>
                    </div>
                  </div>

                  {/* Captured Waypoints List */}
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-[12px]">
                      <span className="font-bold text-[var(--tui-secondary)]">
                        {tt.osPointsTitle} ({osStatus?.points.length ?? 0})
                      </span>
                      {(osStatus?.points.length ?? 0) > 0 && (
                        <TuiButton size="sm" variant="destructive" onClick={handleClearPoints}>
                          [{t.common.clear.toUpperCase()}]
                        </TuiButton>
                      )}
                    </div>

                    <div className="max-h-40 overflow-y-auto bg-[var(--tui-bg)] border border-[var(--tui-border-subtle)] p-2">
                      {(osStatus?.points.length ?? 0) > 0 ? (
                        osStatus?.points.map((pt, i) => (
                          <div
                            key={`${pt.x}-${pt.y}-${i}`}
                            className="flex items-center justify-between py-1 px-2 border-b border-dashed border-[var(--tui-border-subtle)] text-[12px]"
                          >
                            <span className="text-[var(--tui-text-dim)]">#{i + 1}</span>
                            <span className="text-[var(--tui-primary)] font-bold">
                              X={pt.x}, Y={pt.y}
                            </span>
                          </div>
                        ))
                      ) : (
                        <div className="text-[11px] text-[var(--tui-text-dim)] py-4 text-center">
                          {tt.osPointsEmpty}
                        </div>
                      )}
                    </div>
                  </div>
                </TuiCard>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
};
