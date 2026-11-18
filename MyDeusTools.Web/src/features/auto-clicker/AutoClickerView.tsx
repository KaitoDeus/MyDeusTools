import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiSelect,
} from '../../components/terminal/TerminalUI';

export const AutoClickerView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.clicker;
  const [intervalMs, setIntervalMs] = useState(100);
  const [repeatMode, setRepeatMode] = useState<'infinite' | 'fixed'>('infinite');
  const [fixedCount, setFixedCount] = useState(100);

  const [isRunning, setIsRunning] = useState(false);
  const [clicksDone, setClicksDone] = useState(0);
  const [cps, setCps] = useState(0);
  const [copiedBookmarklet, setCopiedBookmarklet] = useState(false);

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
    <div className="space-y-8">
      <TuiPageHeader
        command="auto-click"
        title={t.nav.autoClicker}
        description={t.descriptions.autoClicker}
        badge="[DOM_MACRO]"
      />

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Settings (7 cols) */}
        <TuiCard
          title={tt.configTitle}
          className="lg:col-span-7 p-6"
          bodyClassName="space-y-5"
        >
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
              <TuiButton variant="primary" prefix=">" onClick={handleStart}>
                {tt.startLoop}
              </TuiButton>
            ) : (
              <TuiButton variant="destructive" prefix="!" onClick={handleStop}>
                {tt.abortLoop}
              </TuiButton>
            )}

            <TuiButton
              variant="secondary"
              onClick={() => {
                handleStop();
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

        {/* Live Simulator Target Sandbox (5 cols) */}
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
    </div>
  );
};
