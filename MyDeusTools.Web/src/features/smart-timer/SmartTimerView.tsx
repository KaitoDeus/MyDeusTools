import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { useOsAgent } from '../../contexts/AgentContext';
import { agentClient } from '../../services/agentClient';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiSelect,
  TuiChip,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

type PowerActionMode = 'webOnly' | 'shutdown' | 'restart' | 'hibernate';

export const SmartTimerView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.timer;
  const { isOnline } = useOsAgent();
  const [hours, setHours] = useState(0);
  const [minutes, setMinutes] = useState(25);
  const [seconds, setSeconds] = useState(0);

  const [totalSecondsRemaining, setTotalSecondsRemaining] = useState<number>(25 * 60);
  const [isRunning, setIsRunning] = useState(false);
  const [wakeLockActive, setWakeLockActive] = useState(false);
  const [webhookUrl, setWebhookUrl] = useState('');
  const [osPowerMode, setOsPowerMode] = useState<PowerActionMode>('webOnly');
  const [osPowerMessage, setOsPowerMessage] = useState<string | null>(null);

  const wakeLockRef = useRef<any>(null);

  const normalizeTime = (h: number, m: number, s: number) => {
    let totalSec = h * 3600 + m * 60 + s;
    if (totalSec < 0) totalSec = 0;

    const normH = Math.floor(totalSec / 3600);
    const normM = Math.floor((totalSec % 3600) / 60);
    const normS = totalSec % 60;

    setHours(normH);
    setMinutes(normM);
    setSeconds(normS);
    if (!isRunning) {
      setTotalSecondsRemaining(totalSec);
    }
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
      // Ignored
    }
  };

  const triggerWebhook = async () => {
    if (!webhookUrl) return;
    try {
      await fetch(webhookUrl, { method: 'POST', mode: 'no-cors' });
    } catch (e) {
      console.error(e);
    }
  };

  const handleStart = async () => {
    normalizeTime(hours, minutes, seconds);
    const total = hours * 3600 + minutes * 60 + seconds;
    if (total <= 0) return;

    setTotalSecondsRemaining(total);
    setIsRunning(true);
    setOsPowerMessage(null);

    if (isOnline && osPowerMode !== 'webOnly') {
      try {
        await agentClient.scheduleOsPower(osPowerMode, total);
        setOsPowerMessage(
          tt.osPowerScheduled
            .replace('{mode}', osPowerMode.toUpperCase())
            .replace('{sec}', String(total))
        );
      } catch {
        // Fallback to web alarm
      }
    }

    if ('wakeLock' in navigator) {
      try {
        wakeLockRef.current = await (navigator as any).wakeLock.request('screen');
        setWakeLockActive(true);
      } catch (err) {
        console.warn('Wake Lock error:', err);
      }
    }
  };

  const handleStop = async () => {
    setIsRunning(false);
    if (wakeLockRef.current) {
      wakeLockRef.current.release().catch(() => {});
      wakeLockRef.current = null;
      setWakeLockActive(false);
    }
    if (isOnline && osPowerMode !== 'webOnly') {
      try {
        await agentClient.cancelOsPower();
        setOsPowerMessage(tt.osPowerCancelled);
      } catch {
        // Ignored
      }
    }
  };

  const handleReset = () => {
    handleStop();
    setTotalSecondsRemaining(hours * 3600 + minutes * 60 + seconds);
  };

  useEffect(() => {
    let interval: any = null;
    if (isRunning && totalSecondsRemaining > 0) {
      interval = setInterval(() => {
        setTotalSecondsRemaining((prev) => {
          if (prev <= 1) {
            clearInterval(interval);
            setIsRunning(false);
            playChime();
            triggerWebhook();
            if (osPowerMode === 'webOnly') {
              alert(tt.finishedAlert);
            }
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [isRunning, totalSecondsRemaining, tt.finishedAlert, osPowerMode]);

  const formatCountdown = (total: number) => {
    const h = Math.floor(total / 3600);
    const m = Math.floor((total % 3600) / 60);
    const s = total % 60;
    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  };

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="smart-timer"
        title={t.nav.scheduleShutdown}
        description={t.descriptions.scheduleShutdown}
        badge={isOnline ? '[OS_AGENT:ONLINE]' : '[WAKE_LOCK_API]'}
        accent="secondary"
      />

      {osPowerMessage && <TuiAlert type="warn">{osPowerMessage}</TuiAlert>}

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        <TuiCard
          title={tt.cardTitle}
          rightMeta={
            <TuiChip tone={isRunning ? 'primary' : 'muted'}>
              {isRunning ? tt.statusRunning : tt.statusIdle}
            </TuiChip>
          }
          elevated
          className="lg:col-span-7 p-6"
          bodyClassName="flex flex-col items-center justify-center py-6 space-y-6 text-center"
        >
          <div className="text-[48px] sm:text-[64px] font-bold tracking-[0.04em] text-[var(--tui-primary)] select-all leading-none py-2">
            {formatCountdown(totalSecondsRemaining)}
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
                disabled={isRunning}
                onClick={() => {
                  setHours(p.h);
                  setMinutes(p.m);
                  setSeconds(p.s);
                  normalizeTime(p.h, p.m, p.s);
                }}
              >
                [{p.label}]
              </TuiButton>
            ))}
          </div>

          <div className="flex flex-wrap items-center justify-center gap-3">
            {!isRunning ? (
              <TuiButton variant="primary" size="lg" prefix=">" onClick={handleStart}>
                {t.common.start}
              </TuiButton>
            ) : (
              <TuiButton variant="amber" size="lg" prefix="!" onClick={handleStop}>
                {t.common.stop}
              </TuiButton>
            )}

            <TuiButton variant="secondary" size="lg" onClick={handleReset}>
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
              value={hours}
              disabled={isRunning}
              onChange={(e) => setHours(Math.max(0, parseInt(e.target.value) || 0))}
              onBlur={() => normalizeTime(hours, minutes, seconds)}
              className="text-center font-bold"
            />
            <TuiInput
              label={tt.minutesLabel}
              type="number"
              min={0}
              value={minutes}
              disabled={isRunning}
              onChange={(e) => setMinutes(Math.max(0, parseInt(e.target.value) || 0))}
              onBlur={() => normalizeTime(hours, minutes, seconds)}
              className="text-center font-bold"
            />
            <TuiInput
              label={tt.secondsLabel}
              type="number"
              min={0}
              value={seconds}
              disabled={isRunning}
              onChange={(e) => setSeconds(Math.max(0, parseInt(e.target.value) || 0))}
              onBlur={() => normalizeTime(hours, minutes, seconds)}
              className="text-center font-bold"
            />
          </div>

          <TuiAlert type="info">{tt.normHint}</TuiAlert>

          <div className="space-y-1">
            <TuiSelect
              label={tt.osPowerLabel}
              value={osPowerMode}
              disabled={isRunning || !isOnline}
              onChange={(e) => setOsPowerMode(e.target.value as PowerActionMode)}
            >
              <option value="webOnly">{tt.osPowerModes.webOnly}</option>
              <option value="shutdown">{tt.osPowerModes.shutdown}</option>
              <option value="restart">{tt.osPowerModes.restart}</option>
              <option value="hibernate">{tt.osPowerModes.hibernate}</option>
            </TuiSelect>
            {!isOnline && (
              <div className="text-[11px] text-[var(--tui-text-dim)]">{tt.osAgentNeededHint}</div>
            )}
          </div>

          <TuiInput
            label={tt.webhookLabel}
            type="url"
            prompt
            value={webhookUrl}
            onChange={(e) => setWebhookUrl(e.target.value)}
            placeholder="http://localhost:5000/api/shutdown"
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
    </div>
  );
};
