import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  MousePointerClick,
  Play,
  Square,
  RotateCcw,
  Sliders,
  Sparkles,
  Target,
  Code2,
  Copy,
  Check,
} from 'lucide-react';

export const AutoClickerView: React.FC = () => {
  const { t } = useI18n();
  const [intervalMs, setIntervalMs] = useState(100);
  const [repeatMode, setRepeatMode] = useState<'infinite' | 'fixed'>('infinite');
  const [fixedCount, setFixedCount] = useState(100);
  const [buttonType, setButtonType] = useState<'left' | 'right' | 'middle'>('left');
  const [clickType, setClickType] = useState<'single' | 'double'>('single');

  const [isRunning, setIsRunning] = useState(false);
  const [clicksDone, setClicksDone] = useState(0);
  const [cps, setCps] = useState(0);
  const [copiedBookmarklet, setCopiedBookmarklet] = useState(false);

  // Target sandbox state
  const [targetHits, setTargetHits] = useState(0);
  const targetBtnRef = useRef<HTMLButtonElement>(null);
  const timerRef = useRef<any>(null);

  const handleStart = () => {
    setIsRunning(true);
    setClicksDone(0);
    setCps(Math.round(1000 / Math.max(10, intervalMs)));
  };

  const handleStop = () => {
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
          // Trigger click on sandbox target
          if (targetBtnRef.current) {
            targetBtnRef.current.click();
          }
          if (repeatMode === 'fixed' && next >= fixedCount) {
            handleStop();
          }
          return next;
        });
      }, clampedInterval);
    }
    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, [isRunning, intervalMs, repeatMode, fixedCount]);

  const bookmarkletCode = `javascript:(function(){let interval=prompt('Enter click interval (ms):','100');if(!interval)return;let active=true;let count=0;alert('Click on any element to start clicking it! Press Escape anytime to stop.');window.addEventListener('click',function handler(e){window.removeEventListener('click',handler);let el=e.target;let t=setInterval(function(){if(!active){clearInterval(t);return;}el.click();count++;},parseInt(interval)||100);window.addEventListener('keydown',function kh(ev){if(ev.key==='Escape'){active=false;clearInterval(t);window.removeEventListener('keydown',kh);alert('Auto-clicker stopped after '+count+' clicks.');}});});})();`;

  const copyBookmarklet = () => {
    navigator.clipboard.writeText(bookmarkletCode);
    setCopiedBookmarklet(true);
    setTimeout(() => setCopiedBookmarklet(false), 2000);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-white flex items-center gap-2">
          <MousePointerClick className="w-6 h-6 text-emerald-400" />
          <span>{t.nav.autoClicker}</span>
        </h2>
        <p className="text-sm text-slate-400 mt-1">{t.descriptions.autoClicker}</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Settings (7 cols) */}
        <div className="lg:col-span-7 space-y-5 glass-card p-6 rounded-2xl border border-slate-800">
          <h3 className="text-sm font-semibold text-slate-200 flex items-center gap-2">
            <Sliders className="w-4 h-4 text-emerald-400" />
            <span>Click Interval & Options</span>
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-400">
                Click Interval (Milliseconds)
              </label>
              <input
                type="number"
                min={10}
                value={intervalMs}
                disabled={isRunning}
                onChange={(e) => setIntervalMs(Math.max(10, parseInt(e.target.value) || 10))}
                className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none"
              />
              <span className="text-[11px] text-slate-500">
                ~{Math.round(1000 / Math.max(10, intervalMs))} Clicks Per Second (CPS)
              </span>
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-400">Repeat Mode</label>
              <select
                value={repeatMode}
                disabled={isRunning}
                onChange={(e) => setRepeatMode(e.target.value as any)}
                className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-medium text-slate-200 focus:outline-none"
              >
                <option value="infinite">Repeat until stopped</option>
                <option value="fixed">Fixed count (Limit)</option>
              </select>
            </div>
          </div>

          {repeatMode === 'fixed' && (
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-400">Total Click Limit</label>
              <input
                type="number"
                min={1}
                value={fixedCount}
                disabled={isRunning}
                onChange={(e) => setFixedCount(Math.max(1, parseInt(e.target.value) || 1))}
                className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none"
              />
            </div>
          )}

          <div className="flex items-center gap-3 pt-2">
            {!isRunning ? (
              <button
                onClick={handleStart}
                className="px-6 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl text-xs font-bold uppercase tracking-wider shadow-lg shadow-emerald-600/25 transition-all cursor-pointer flex items-center gap-2"
              >
                <Play className="w-4 h-4 fill-white" />
                <span>Start Click Loop</span>
              </button>
            ) : (
              <button
                onClick={handleStop}
                className="px-6 py-2.5 bg-rose-600 hover:bg-rose-500 text-white rounded-xl text-xs font-bold uppercase tracking-wider shadow-lg shadow-rose-600/25 transition-all cursor-pointer flex items-center gap-2"
              >
                <Square className="w-4 h-4 fill-white" />
                <span>Stop</span>
              </button>
            )}

            <button
              onClick={() => {
                handleStop();
                setClicksDone(0);
                setTargetHits(0);
              }}
              className="px-4 py-2.5 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-all cursor-pointer flex items-center gap-2"
            >
              <RotateCcw className="w-3.5 h-3.5" />
              <span>Reset</span>
            </button>
          </div>

          {/* Bookmarklet Section for Any Website */}
          <div className="p-4 bg-slate-900/60 rounded-xl border border-slate-800 space-y-2 mt-4">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-slate-300 flex items-center gap-1.5">
                <Code2 className="w-3.5 h-3.5 text-emerald-400" />
                <span>Auto-Click Any Website (Bookmarklet)</span>
              </span>
              <button
                onClick={copyBookmarklet}
                className="text-xs text-emerald-400 hover:underline flex items-center gap-1 cursor-pointer"
              >
                {copiedBookmarklet ? <Check className="w-3 h-3" /> : <Copy className="w-3 h-3" />}
                <span>{copiedBookmarklet ? t.common.copied : 'Copy Bookmarklet'}</span>
              </button>
            </div>
            <p className="text-[11px] text-slate-500 leading-relaxed">
              Drag or copy this code into your browser bookmarks bar. Click it on any webpage, then click the element you want to auto-click. Press <code className="text-emerald-400">Esc</code> to stop!
            </p>
          </div>
        </div>

        {/* Live Simulator Target Sandbox (5 cols) */}
        <div className="lg:col-span-5 flex flex-col items-center justify-center p-6 glass-card rounded-2xl border border-slate-800 space-y-6 text-center">
          <div className="space-y-1">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">
              Live Click Target Sandbox
            </span>
            <div className="text-5xl font-mono font-black text-emerald-400">
              {targetHits}
            </div>
            <div className="text-xs text-slate-500">
              Clicks registered • {cps} CPS
            </div>
          </div>

          <button
            ref={targetBtnRef}
            onClick={() => setTargetHits((h) => h + 1)}
            className="w-36 h-36 rounded-full bg-gradient-to-br from-emerald-600 to-teal-700 hover:from-emerald-500 hover:to-teal-600 active:scale-95 text-white font-bold shadow-2xl shadow-emerald-600/30 border-4 border-emerald-400/30 flex flex-col items-center justify-center gap-2 cursor-pointer transition-transform"
          >
            <Target className="w-8 h-8" />
            <span className="text-xs tracking-wider uppercase">Hit Target</span>
          </button>

          <p className="text-xs text-slate-500 max-w-xs">
            Start the auto-clicker above to observe high-frequency automated clicks on this target element in real time.
          </p>
        </div>
      </div>
    </div>
  );
};
