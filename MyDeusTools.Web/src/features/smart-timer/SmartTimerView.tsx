import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  Timer,
  Play,
  Square,
  RotateCcw,
  Bell,
  Lock,
  Volume2,
  Sparkles,
} from 'lucide-react';

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

  // Time normalization logic (matches the desktop feature exactly: e.g. 70m -> 1h 10m)
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
      osc.type = 'sine';
      osc.frequency.setValueAtTime(587.33, audioCtx.currentTime); // D5
      osc.frequency.setValueAtTime(880, audioCtx.currentTime + 0.15); // A5
      gain.gain.setValueAtTime(0.3, audioCtx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + 0.8);
      osc.start();
      osc.stop(audioCtx.currentTime + 0.8);
    } catch {
      // Audio context might be restricted
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

    // Request Screen Wake Lock API to prevent system display sleep
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
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
          <Timer className="w-6 h-6 text-rose-600 dark:text-rose-500" />
          <span>{t.nav.scheduleShutdown}</span>
        </h2>
        <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
          {t.descriptions.scheduleShutdown}
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Countdown Display (7 cols) */}
        <div className="lg:col-span-7 flex flex-col items-center justify-center p-8 glass-card rounded-2xl space-y-6 text-center">
          <div className="space-y-2">
            <span className="text-xs font-semibold uppercase tracking-widest text-slate-500 dark:text-slate-400">
              {isRunning ? 'Timer Running' : 'Timer Ready'}
            </span>
            <div className="text-6xl sm:text-7xl font-mono font-black tracking-tight text-slate-900 dark:text-white select-all">
              {formatCountdown(totalSecondsRemaining)}
            </div>
          </div>

          {/* Quick presets */}
          <div className="flex flex-wrap justify-center gap-2">
            {[
              { label: '15m Break', h: 0, m: 15, s: 0 },
              { label: '25m Focus', h: 0, m: 25, s: 0 },
              { label: '45m Deep Work', h: 0, m: 45, s: 0 },
              { label: '70m (1h10m)', h: 0, m: 70, s: 0 },
            ].map((p) => (
              <button
                key={p.label}
                disabled={isRunning}
                onClick={() => {
                  setHours(p.h);
                  setMinutes(p.m);
                  setSeconds(p.s);
                  normalizeTime(p.h, p.m, p.s);
                }}
                className="px-3 py-1.5 rounded-lg bg-slate-100 border border-slate-200 hover:border-slate-300 text-slate-700 dark:bg-slate-900 dark:border-slate-800 dark:hover:border-slate-700 text-xs dark:text-slate-300 disabled:opacity-40 transition-all cursor-pointer"
              >
                {p.label}
              </button>
            ))}
          </div>

          {/* Controls */}
          <div className="flex items-center gap-3">
            {!isRunning ? (
              <button
                onClick={handleStart}
                className="px-8 py-3 bg-rose-600 hover:bg-rose-500 text-white rounded-xl text-sm font-bold uppercase tracking-wider shadow-lg shadow-rose-600/25 transition-all cursor-pointer flex items-center gap-2"
              >
                <Play className="w-4 h-4 fill-white" />
                <span>{t.common.start}</span>
              </button>
            ) : (
              <button
                onClick={handleStop}
                className="px-8 py-3 bg-amber-600 hover:bg-amber-500 text-white rounded-xl text-sm font-bold uppercase tracking-wider shadow-lg shadow-amber-600/25 transition-all cursor-pointer flex items-center gap-2"
              >
                <Square className="w-4 h-4 fill-white" />
                <span>{t.common.stop}</span>
              </button>
            )}

            <button
              onClick={handleReset}
              className="px-5 py-3 bg-slate-200 hover:bg-slate-300 text-slate-800 dark:bg-slate-800 dark:hover:bg-slate-700 dark:text-slate-200 rounded-xl text-sm font-semibold transition-all cursor-pointer flex items-center gap-2"
            >
              <RotateCcw className="w-4 h-4" />
              <span>{t.common.reset}</span>
            </button>
          </div>
        </div>

        {/* Configuration Panel (5 cols) */}
        <div className="lg:col-span-5 space-y-5 glass-card p-6 rounded-2xl">
          <h3 className="text-sm font-semibold text-slate-800 dark:text-slate-200 flex items-center gap-2">
            <Sparkles className="w-4 h-4 text-rose-500 dark:text-rose-400" />
            <span>Time Normalization & Setup</span>
          </h3>

          <div className="grid grid-cols-3 gap-3">
            <div className="space-y-1">
              <label className="text-[11px] font-medium text-slate-600 dark:text-slate-400">
                Hours
              </label>
              <input
                type="number"
                min={0}
                value={hours}
                disabled={isRunning}
                onChange={(e) => setHours(Math.max(0, parseInt(e.target.value) || 0))}
                onBlur={() => normalizeTime(hours, minutes, seconds)}
                className="w-full p-2.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-center text-sm font-mono text-slate-900 dark:text-slate-200 focus:outline-none"
              />
            </div>

            <div className="space-y-1">
              <label className="text-[11px] font-medium text-slate-600 dark:text-slate-400">
                Minutes
              </label>
              <input
                type="number"
                min={0}
                value={minutes}
                disabled={isRunning}
                onChange={(e) => setMinutes(Math.max(0, parseInt(e.target.value) || 0))}
                onBlur={() => normalizeTime(hours, minutes, seconds)}
                className="w-full p-2.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-center text-sm font-mono text-slate-900 dark:text-slate-200 focus:outline-none"
              />
            </div>

            <div className="space-y-1">
              <label className="text-[11px] font-medium text-slate-600 dark:text-slate-400">
                Seconds
              </label>
              <input
                type="number"
                min={0}
                value={seconds}
                disabled={isRunning}
                onChange={(e) => setSeconds(Math.max(0, parseInt(e.target.value) || 0))}
                onBlur={() => normalizeTime(hours, minutes, seconds)}
                className="w-full p-2.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-center text-sm font-mono text-slate-900 dark:text-slate-200 focus:outline-none"
              />
            </div>
          </div>

          <p className="text-[11px] text-slate-500">
            💡 <strong>Auto Time Normalization</strong>: Entering 70 minutes automatically converts
            to 1h 10m on blur or start.
          </p>

          <div className="space-y-2 pt-2 border-t border-slate-200 dark:border-slate-800">
            <label className="text-xs font-semibold text-slate-700 dark:text-slate-300">
              Optional Remote Webhook (Trigger shutdown on HomeAssistant / Local API)
            </label>
            <input
              type="url"
              value={webhookUrl}
              onChange={(e) => setWebhookUrl(e.target.value)}
              placeholder="http://localhost:5000/api/shutdown"
              className="w-full p-2.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs font-mono text-slate-900 dark:text-slate-200 focus:outline-none"
            />
          </div>

          <div className="p-3 bg-slate-50 dark:bg-slate-900/60 rounded-xl border border-slate-200 dark:border-slate-800 text-[11px] text-slate-600 dark:text-slate-400 space-y-1">
            <div className="flex items-center gap-1.5 text-slate-800 dark:text-slate-300 font-semibold">
              <Lock className="w-3.5 h-3.5 text-rose-500 dark:text-rose-400" />
              <span>Screen Wake Lock: {wakeLockActive ? 'Active' : 'Standby'}</span>
            </div>
            <p>Keeps display active while running to prevent unplanned computer sleep.</p>
          </div>
        </div>
      </div>
    </div>
  );
};
