import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiChip,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

export const SmartTimerView: React.FC = () => {
  const { t } = useI18n();
  const [hours, setHours] = useState(0);
  const [minutes, setMinutes] = useState(25);
  const [seconds, setSeconds] = useState(0);

  const [totalSecondsRemaining, setTotalSecondsRemaining] = useState<number>(25 * 60);
  const [isRunning, setIsRunning] = useState(false);
  const [wakeLockActive, setWakeLockActive] = useState(false);
  const [webhookUrl, setWebhookUrl] = useState('');

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

    if ('wakeLock' in navigator) {
      try {
        wakeLockRef.current = await (navigator as any).wakeLock.request('screen');
        setWakeLockActive(true);
      } catch (err) {
        console.warn('Wake Lock error:', err);
      }
    }
  };

  const handleStop = () => {
    setIsRunning(false);
    if (wakeLockRef.current) {
      wakeLockRef.current.release().catch(() => {});
      wakeLockRef.current = null;
      setWakeLockActive(false);
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
            alert('Timer finished! Time to take a break or step away.');
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [isRunning, totalSecondsRemaining]);

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
        badge="[WAKE_LOCK_API]"
        accent="secondary"
      />

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Countdown Display (7 cols) */}
        <TuiCard
          title="COUNTDOWN_CLOCK"
          rightMeta={
            <TuiChip tone={isRunning ? 'primary' : 'muted'}>
              {isRunning ? 'STATUS:RUNNING' : 'STATUS:IDLE'}
            </TuiChip>
          }
          elevated
          className="lg:col-span-7 flex flex-col items-center justify-center p-8 space-y-6 text-center"
        >
          <div className="text-[48px] sm:text-[64px] font-bold tracking-[0.04em] text-[var(--tui-primary)] select-all leading-none py-2">
            {formatCountdown(totalSecondsRemaining)}
          </div>

          <div className="flex flex-wrap justify-center gap-2">
            {[
              { label: '15M_BREAK', h: 0, m: 15, s: 0 },
              { label: '25M_FOCUS', h: 0, m: 25, s: 0 },
              { label: '45M_DEEP', h: 0, m: 45, s: 0 },
              { label: '70M->1H10M', h: 0, m: 70, s: 0 },
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

        {/* Configuration Panel (5 cols) */}
        <TuiCard title="TIME_NORMALIZER_CONFIG" className="lg:col-span-5 space-y-5">
          <div className="grid grid-cols-3 gap-3">
            <TuiInput
              label="hh"
              type="number"
              min={0}
              value={hours}
              disabled={isRunning}
              onChange={(e) => setHours(Math.max(0, parseInt(e.target.value) || 0))}
              onBlur={() => normalizeTime(hours, minutes, seconds)}
              className="text-center font-bold"
            />
            <TuiInput
              label="mm"
              type="number"
              min={0}
              value={minutes}
              disabled={isRunning}
              onChange={(e) => setMinutes(Math.max(0, parseInt(e.target.value) || 0))}
              onBlur={() => normalizeTime(hours, minutes, seconds)}
              className="text-center font-bold"
            />
            <TuiInput
              label="ss"
              type="number"
              min={0}
              value={seconds}
              disabled={isRunning}
              onChange={(e) => setSeconds(Math.max(0, parseInt(e.target.value) || 0))}
              onBlur={() => normalizeTime(hours, minutes, seconds)}
              className="text-center font-bold"
            />
          </div>

          <TuiAlert type="info">
            AUTO_NORMALIZATION: Entering 70 minutes automatically resolves to 01h 10m on blur or
            start.
          </TuiAlert>

          <TuiInput
            label="remote_webhook (optional)"
            type="url"
            prompt
            value={webhookUrl}
            onChange={(e) => setWebhookUrl(e.target.value)}
            placeholder="http://localhost:5000/api/shutdown"
          />

          <div className="p-3 bg-[var(--tui-bg)] border border-[var(--tui-border-subtle)] text-[12px] space-y-1">
            <div className="flex items-center justify-between">
              <span className="text-[var(--tui-text-muted)]">SCREEN_WAKE_LOCK:</span>
              <span
                className={
                  wakeLockActive
                    ? 'text-[var(--tui-primary)] font-bold'
                    : 'text-[var(--tui-text-dim)]'
                }
              >
                {wakeLockActive ? '[ACTIVE]' : '[STANDBY]'}
              </span>
            </div>
            <div className="text-[11px] text-[var(--tui-text-dim)]">
              Prevents display sleep while timer is running.
            </div>
          </div>
        </TuiCard>
      </div>
    </div>
  );
};
