import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { useOsAgent } from '../../contexts/AgentContext';
import { agentClient } from '../../services/agentClient';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiChip,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

type TimerTab = 'pomodoro' | 'shutdown';
type OsPowerAction = 'shutdown' | 'restart' | 'hibernate';

export const SmartTimerView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.timer;
  const { isOnline, checking, refreshAgentStatus } = useOsAgent();

  const [activeTab, setActiveTab] = useState<TimerTab>('pomodoro');

  // ---------------------------------------------------------------------------
  // Tab 1: Pomodoro Timer (Always works in pure browser)
  // ---------------------------------------------------------------------------
  const [pomoHours, setPomoHours] = useState(0);
  const [pomoMinutes, setPomoMinutes] = useState(25);
  const [pomoSeconds, setPomoSeconds] = useState(0);
  const [pomoRemaining, setPomoRemaining] = useState(25 * 60);
  const [pomoRunning, setPomoRunning] = useState(false);
  const [wakeLockActive, setWakeLockActive] = useState(false);
  const [webhookUrl, setWebhookUrl] = useState('');
  const wakeLockRef = useRef<any>(null);

  // ---------------------------------------------------------------------------
  // Tab 2: OS Scheduled Shutdown (Strictly enabled ONLY when Agent is online)
  // ---------------------------------------------------------------------------
  const [osHours, setOsHours] = useState(0);
  const [osMinutes, setOsMinutes] = useState(30);
  const [osSeconds, setOsSeconds] = useState(0);
  const [osRemaining, setOsRemaining] = useState(30 * 60);
  const [osRunning, setOsRunning] = useState(false);
  const [osPowerMode, setOsPowerMode] = useState<OsPowerAction>('shutdown');
  const [osPowerMessage, setOsPowerMessage] = useState<string | null>(null);
  const [osError, setOsError] = useState<string | null>(null);

  const normalizeHms = (h: number, m: number, s: number) => {
    const totalSec = Math.max(0, h * 3600 + m * 60 + s);
    return {
      h: Math.floor(totalSec / 3600),
      m: Math.floor((totalSec % 3600) / 60),
      s: totalSec % 60,
      totalSec,
    };
  };

  const applyPomoNormalize = (h: number, m: number, s: number) => {
    const norm = normalizeHms(h, m, s);
    setPomoHours(norm.h);
    setPomoMinutes(norm.m);
    setPomoSeconds(norm.s);
    if (!pomoRunning) setPomoRemaining(norm.totalSec);
    return norm.totalSec;
  };

  const applyOsNormalize = (h: number, m: number, s: number) => {
    const norm = normalizeHms(h, m, s);
    setOsHours(norm.h);
    setOsMinutes(norm.m);
    setOsSeconds(norm.s);
    if (!osRunning) setOsRemaining(norm.totalSec);
    return norm.totalSec;
  };

  const playChime = () => {
    try {
      const audioCtx = new (window.AudioContext || (window as any).webkitAudioContext)();
      const osc = audioCtx.createOscillator();
      const gain = audioCtx.createGain();
      osc.connect(gain);
      gain.connect(audioCtx.destination);
      osc.type = 'square';
      osc.frequency.setValueAtTime(587.33, audioCtx.currentTime);
      osc.frequency.setValueAtTime(880, audioCtx.currentTime + 0.15);
      gain.gain.setValueAtTime(0.2, audioCtx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + 0.8);
      osc.start();
      osc.stop(audioCtx.currentTime + 0.8);
    } catch {
      // AudioContext restricted
    }
  };

  const triggerWebhook = async () => {
    if (!webhookUrl) return;
    try {
      await fetch(webhookUrl, { method: 'POST', mode: 'no-cors' });
    } catch {
      // Ignored
    }
  };

  // --- Pomodoro Handlers ---
  const handleStartPomo = async () => {
    const total = applyPomoNormalize(pomoHours, pomoMinutes, pomoSeconds);
    if (total <= 0) return;

    setPomoRemaining(total);
    setPomoRunning(true);

    if ('wakeLock' in navigator) {
      try {
        wakeLockRef.current = await (navigator as any).wakeLock.request('screen');
        setWakeLockActive(true);
      } catch {
        // Wake lock unsupported
      }
    }
  };

  const handleStopPomo = () => {
    setPomoRunning(false);
    if (wakeLockRef.current) {
      wakeLockRef.current.release().catch(() => {});
      wakeLockRef.current = null;
      setWakeLockActive(false);
    }
  };

  const handleResetPomo = () => {
    handleStopPomo();
    setPomoRemaining(pomoHours * 3600 + pomoMinutes * 60 + pomoSeconds);
  };

  useEffect(() => {
    if (!pomoRunning) return;
    const id = setInterval(() => {
      setPomoRemaining((prev) => {
        if (prev <= 1) {
          clearInterval(id);
          setPomoRunning(false);
          if (wakeLockRef.current) {
            wakeLockRef.current.release().catch(() => {});
            wakeLockRef.current = null;
            setWakeLockActive(false);
          }
          playChime();
          triggerWebhook();
          alert(tt.finishedAlert);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(id);
  }, [pomoRunning, tt.finishedAlert, webhookUrl]);

  // --- OS Shutdown Handlers ---
  const handleStartOsShutdown = async () => {
    if (!isOnline) return;
    const total = applyOsNormalize(osHours, osMinutes, osSeconds);
    if (total <= 0) return;

    setOsError(null);
    setOsPowerMessage(null);

    try {
      await agentClient.scheduleOsPower(osPowerMode, total);
      setOsRemaining(total);
      setOsRunning(true);
      setOsPowerMessage(
        tt.osPowerScheduled
          .replace('{mode}', tt.osPowerModes[osPowerMode].toUpperCase())
          .replace('{sec}', String(total))
      );
    } catch (err: any) {
      setOsError(err?.message || tt.osPowerError);
      refreshAgentStatus();
    }
  };

  const handleStopOsShutdown = async () => {
    setOsRunning(false);
    if (!isOnline) return;
    setOsError(null);
    try {
      await agentClient.cancelOsPower();
      setOsPowerMessage(tt.osPowerCancelled);
    } catch (err: any) {
      setOsError(err?.message || tt.osPowerError);
    }
  };

  const handleResetOsShutdown = () => {
    handleStopOsShutdown();
    setOsRemaining(osHours * 3600 + osMinutes * 60 + osSeconds);
  };

  useEffect(() => {
    if (!osRunning) return;
    const id = setInterval(() => {
      setOsRemaining((prev) => {
        if (prev <= 1) {
          clearInterval(id);
          setOsRunning(false);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(id);
  }, [osRunning]);

  const formatCountdown = (total: number) => {
    const h = Math.floor(total / 3600);
    const m = Math.floor((total % 3600) / 60);
    const s = total % 60;
    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  };

  const osActionOptions: { mode: OsPowerAction; label: string }[] = [
    { mode: 'shutdown', label: tt.osPowerModes.shutdown },
    { mode: 'restart', label: tt.osPowerModes.restart },
    { mode: 'hibernate', label: tt.osPowerModes.hibernate },
  ];

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="smart-timer"
        title={t.nav.scheduleShutdown}
        description={t.descriptions.scheduleShutdown}
        badge={isOnline ? '[OS_AGENT:ONLINE]' : '[AGENT:OFFLINE]'}
        accent="secondary"
      />

      {/* Separated Tabs */}
      <div className="flex flex-wrap gap-2">
        <TuiChip active={activeTab === 'pomodoro'} onClick={() => setActiveTab('pomodoro')}>
          {tt.tabPomodoro}
        </TuiChip>
        <TuiChip active={activeTab === 'shutdown'} onClick={() => setActiveTab('shutdown')}>
          {tt.tabShutdown} {!isOnline ? '[LOCKED]' : ''}
        </TuiChip>
      </div>

      {/* =================================================================== */}
      {/* TAB 1: POMODORO TIMER (Pure Web, always available)                  */}
      {/* =================================================================== */}
      {activeTab === 'pomodoro' && (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          <TuiCard
            title={tt.cardTitle}
            rightMeta={
              <TuiChip tone={pomoRunning ? 'primary' : 'muted'}>
                {pomoRunning ? tt.statusRunning : tt.statusIdle}
              </TuiChip>
            }
            elevated
            className="lg:col-span-7 p-6"
            bodyClassName="flex flex-col items-center justify-center py-6 space-y-6 text-center"
          >
            <div className="text-[48px] sm:text-[64px] font-bold tracking-[0.04em] text-[var(--tui-primary)] select-all leading-none py-2">
              {formatCountdown(pomoRemaining)}
            </div>

            <div className="flex flex-wrap justify-center gap-2">
              {[
                { label: tt.presets.break15, h: 0, m: 15, s: 0 },
                { label: tt.presets.focus25, h: 0, m: 25, s: 0 },
                { label: tt.presets.deep45, h: 0, m: 45, s: 0 },
                { label: tt.presets.norm70, h: 0, m: 70, s: 0 },
              ].map((p) => (
                <TuiButton
                  key={p.label}
                  size="sm"
                  variant="ghost"
                  disabled={pomoRunning}
                  onClick={() => applyPomoNormalize(p.h, p.m, p.s)}
                >
                  [{p.label}]
                </TuiButton>
              ))}
            </div>

            <div className="flex flex-wrap items-center justify-center gap-3">
              {!pomoRunning ? (
                <TuiButton variant="primary" size="lg" prefix=">" onClick={handleStartPomo}>
                  {t.common.start}
                </TuiButton>
              ) : (
                <TuiButton variant="amber" size="lg" prefix="!" onClick={handleStopPomo}>
                  {t.common.stop}
                </TuiButton>
              )}

              <TuiButton variant="secondary" size="lg" onClick={handleResetPomo}>
                [{t.common.reset.toUpperCase()}]
              </TuiButton>
            </div>
          </TuiCard>

          <TuiCard
            title={tt.configTitle}
            className="lg:col-span-5 p-6"
            bodyClassName="space-y-5"
          >
            <div className="grid grid-cols-3 gap-3">
              <TuiInput
                label={tt.hoursLabel}
                type="number"
                min={0}
                value={pomoHours}
                disabled={pomoRunning}
                onChange={(e) => setPomoHours(Math.max(0, parseInt(e.target.value) || 0))}
                onBlur={() => applyPomoNormalize(pomoHours, pomoMinutes, pomoSeconds)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') applyPomoNormalize(pomoHours, pomoMinutes, pomoSeconds);
                }}
                className="text-center font-bold"
              />
              <TuiInput
                label={tt.minutesLabel}
                type="number"
                min={0}
                value={pomoMinutes}
                disabled={pomoRunning}
                onChange={(e) => setPomoMinutes(Math.max(0, parseInt(e.target.value) || 0))}
                onBlur={() => applyPomoNormalize(pomoHours, pomoMinutes, pomoSeconds)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') applyPomoNormalize(pomoHours, pomoMinutes, pomoSeconds);
                }}
                className="text-center font-bold"
              />
              <TuiInput
                label={tt.secondsLabel}
                type="number"
                min={0}
                value={pomoSeconds}
                disabled={pomoRunning}
                onChange={(e) => setPomoSeconds(Math.max(0, parseInt(e.target.value) || 0))}
                onBlur={() => applyPomoNormalize(pomoHours, pomoMinutes, pomoSeconds)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') applyPomoNormalize(pomoHours, pomoMinutes, pomoSeconds);
                }}
                className="text-center font-bold"
              />
            </div>

            <TuiAlert type="info">{tt.normHint}</TuiAlert>

            <TuiInput
              label={tt.webhookLabel}
              type="url"
              prompt
              value={webhookUrl}
              onChange={(e) => setWebhookUrl(e.target.value)}
              placeholder="http://localhost:5000/api/notify"
            />

            <div className="p-3 bg-[var(--tui-bg)] border border-[var(--tui-border-subtle)] text-[12px] space-y-1">
              <div className="flex items-center justify-between gap-2">
                <span className="text-[var(--tui-text-muted)]">{tt.wakeLockLabel}</span>
                <span
                  className={
                    wakeLockActive
                      ? 'text-[var(--tui-primary)] font-bold'
                      : 'text-[var(--tui-text-dim)]'
                  }
                >
                  {wakeLockActive ? tt.wakeLockActive : tt.wakeLockStandby}
                </span>
              </div>
              <div className="text-[11px] text-[var(--tui-text-dim)]">{tt.wakeLockDesc}</div>
            </div>
          </TuiCard>
        </div>
      )}

      {/* =================================================================== */}
      {/* TAB 2: SCHEDULED WINDOWS SHUTDOWN (Enabled ONLY when Agent Online)  */}
      {/* =================================================================== */}
      {activeTab === 'shutdown' && (
        <div className="space-y-6">
          {!isOnline && (
            <TuiAlert type="warn">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 w-full">
                <span>{tt.osAgentNeededHint}</span>
                <TuiButton
                  size="sm"
                  variant="primary"
                  onClick={() => refreshAgentStatus()}
                  disabled={checking}
                  className="shrink-0"
                >
                  [{tt.osCheckBtn}]
                </TuiButton>
              </div>
            </TuiAlert>
          )}

          {osError && <TuiAlert type="err">{osError}</TuiAlert>}
          {osPowerMessage && <TuiAlert type="warn">{osPowerMessage}</TuiAlert>}

          <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            <TuiCard
              title={tt.shutdownCardTitle}
              rightMeta={
                !isOnline ? (
                  <TuiChip tone="error">{tt.statusLocked}</TuiChip>
                ) : (
                  <TuiChip tone={osRunning ? 'secondary' : 'muted'}>
                    {osRunning ? tt.statusRunning : tt.statusIdle}
                  </TuiChip>
                )
              }
              elevated
              className="lg:col-span-7 p-6"
              bodyClassName="flex flex-col items-center justify-center py-6 space-y-6 text-center"
            >
              <div
                className={`text-[48px] sm:text-[64px] font-bold tracking-[0.04em] select-all leading-none py-2 ${
                  isOnline ? 'text-[var(--tui-secondary)]' : 'text-[var(--tui-text-dim)] opacity-40'
                }`}
              >
                {formatCountdown(osRemaining)}
              </div>

              <div className="flex flex-wrap justify-center gap-2">
                {[
                  { label: tt.shutdownPresets.p30, h: 0, m: 30, s: 0 },
                  { label: tt.shutdownPresets.p60, h: 0, m: 60, s: 0 },
                  { label: tt.shutdownPresets.p70, h: 0, m: 70, s: 0 },
                  { label: tt.shutdownPresets.p120, h: 0, m: 120, s: 0 },
                ].map((p) => (
                  <TuiButton
                    key={p.label}
                    size="sm"
                    variant="ghost"
                    disabled={!isOnline || osRunning}
                    onClick={() => applyOsNormalize(p.h, p.m, p.s)}
                  >
                    [{p.label}]
                  </TuiButton>
                ))}
              </div>

              <div className="flex flex-wrap items-center justify-center gap-3">
                {!osRunning ? (
                  <TuiButton
                    variant="primary"
                    size="lg"
                    prefix=">"
                    disabled={!isOnline}
                    onClick={handleStartOsShutdown}
                  >
                    {tt.startShutdownBtn}
                  </TuiButton>
                ) : (
                  <TuiButton
                    variant="amber"
                    size="lg"
                    prefix="!"
                    disabled={!isOnline}
                    onClick={handleStopOsShutdown}
                  >
                    {t.common.stop}
                  </TuiButton>
                )}

                <TuiButton
                  variant="secondary"
                  size="lg"
                  disabled={!isOnline}
                  onClick={handleResetOsShutdown}
                >
                  [{t.common.reset.toUpperCase()}]
                </TuiButton>

                <TuiButton
                  variant="destructive"
                  size="lg"
                  disabled={!isOnline}
                  onClick={handleStopOsShutdown}
                >
                  [{tt.cancelOsBtn}]
                </TuiButton>
              </div>
            </TuiCard>

            <TuiCard
              title={tt.shutdownConfigTitle}
              rightMeta={
                <TuiChip tone={isOnline ? 'primary' : 'error'}>
                  {isOnline ? 'AGENT:ONLINE' : 'AGENT:OFFLINE'}
                </TuiChip>
              }
              className="lg:col-span-5 p-6"
              bodyClassName="space-y-5"
            >
              <div className="space-y-2">
                <div className="text-[12px] text-[var(--tui-text-muted)]">
                  // {tt.osPowerLabel}
                </div>
                <div className="grid grid-cols-1 gap-2">
                  {osActionOptions.map((opt) => {
                    const selected = osPowerMode === opt.mode;
                    return (
                      <button
                        key={opt.mode}
                        type="button"
                        disabled={!isOnline || osRunning}
                        onClick={() => setOsPowerMode(opt.mode)}
                        className={`min-w-0 px-3 py-2 text-left text-[12px] border flex items-center gap-2 cursor-pointer disabled:opacity-30 disabled:cursor-not-allowed ${
                          selected
                            ? 'bg-[var(--tui-primary)] text-[var(--tui-primary-fg)] border-[var(--tui-primary)] font-bold'
                            : 'bg-[var(--tui-bg)] text-[var(--tui-text)] border-[var(--tui-border-medium)] hover:border-[var(--tui-primary)]'
                        }`}
                      >
                        <span className="shrink-0">{selected ? '(*)' : '( )'}</span>
                        <span className="truncate">{opt.label}</span>
                      </button>
                    );
                  })}
                </div>
              </div>

              <div className="grid grid-cols-3 gap-3">
                <TuiInput
                  label={tt.hoursLabel}
                  type="number"
                  min={0}
                  value={osHours}
                  disabled={!isOnline || osRunning}
                  onChange={(e) => setOsHours(Math.max(0, parseInt(e.target.value) || 0))}
                  onBlur={() => applyOsNormalize(osHours, osMinutes, osSeconds)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') applyOsNormalize(osHours, osMinutes, osSeconds);
                  }}
                  className="text-center font-bold"
                />
                <TuiInput
                  label={tt.minutesLabel}
                  type="number"
                  min={0}
                  value={osMinutes}
                  disabled={!isOnline || osRunning}
                  onChange={(e) => setOsMinutes(Math.max(0, parseInt(e.target.value) || 0))}
                  onBlur={() => applyOsNormalize(osHours, osMinutes, osSeconds)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') applyOsNormalize(osHours, osMinutes, osSeconds);
                  }}
                  className="text-center font-bold"
                />
                <TuiInput
                  label={tt.secondsLabel}
                  type="number"
                  min={0}
                  value={osSeconds}
                  disabled={!isOnline || osRunning}
                  onChange={(e) => setOsSeconds(Math.max(0, parseInt(e.target.value) || 0))}
                  onBlur={() => applyOsNormalize(osHours, osMinutes, osSeconds)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') applyOsNormalize(osHours, osMinutes, osSeconds);
                  }}
                  className="text-center font-bold"
                />
              </div>

              <TuiAlert type="info">{tt.normHint}</TuiAlert>
            </TuiCard>
          </div>
        </div>
      )}
    </div>
  );
};
