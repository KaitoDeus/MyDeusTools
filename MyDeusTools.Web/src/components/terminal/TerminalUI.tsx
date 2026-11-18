import React from 'react';

/* ============================================================================
 * TuiButton
 * Transparent fill at rest with 1px semantic border; inverted fill on hover.
 * ============================================================================ */
export type TuiButtonVariant = 'primary' | 'secondary' | 'amber' | 'ghost' | 'destructive';
export type TuiButtonSize = 'sm' | 'md' | 'lg';

interface TuiButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: TuiButtonVariant;
  size?: TuiButtonSize;
  prefix?: '>' | '$' | string;
}

const BUTTON_VARIANTS: Record<TuiButtonVariant, string> = {
  primary:
    'bg-transparent text-[var(--tui-primary)] border border-[var(--tui-primary)] hover:bg-[var(--tui-primary)] hover:text-[var(--tui-primary-fg)]',
  secondary:
    'bg-transparent text-[var(--tui-tertiary)] border border-[var(--tui-tertiary)] hover:bg-[var(--tui-tertiary)] hover:text-[var(--tui-primary-fg)]',
  amber:
    'bg-transparent text-[var(--tui-secondary)] border border-[var(--tui-secondary)] hover:bg-[var(--tui-secondary)] hover:text-[var(--tui-primary-fg)]',
  ghost:
    'bg-transparent text-[var(--tui-text-muted)] border border-transparent hover:border-[var(--tui-border-medium)] hover:text-[var(--tui-text)]',
  destructive:
    'bg-transparent text-[var(--tui-error)] border border-[var(--tui-error)] hover:bg-[var(--tui-error)] hover:text-[var(--tui-primary-fg)]',
};

const BUTTON_SIZES: Record<TuiButtonSize, string> = {
  sm: 'h-[28px] px-3 py-1 text-[12px]',
  md: 'h-[36px] px-4 py-2 text-[13px]',
  lg: 'h-[44px] px-6 py-2.5 text-[14px]',
};

export const TuiButton: React.FC<TuiButtonProps> = ({
  variant = 'primary',
  size = 'md',
  prefix,
  className = '',
  children,
  ...props
}) => {
  return (
    <button
      {...props}
      className={`inline-flex items-center justify-center gap-2 font-semibold uppercase tracking-[0.04em] cursor-pointer select-none disabled:opacity-30 disabled:cursor-not-allowed disabled:border-[var(--tui-disabled-border)] disabled:hover:bg-transparent disabled:hover:text-[var(--tui-text-dim)] ${BUTTON_VARIANTS[variant]} ${BUTTON_SIZES[size]} ${className}`}
    >
      {prefix && <span className="font-bold">{prefix}</span>}
      <span>{children}</span>
    </button>
  );
};

/* ============================================================================
 * TuiCard
 * #141414 fill, 1px #222222 border (or elevated #1A1A1A / #444444), 0px radius.
 * ============================================================================ */
interface TuiCardProps {
  title?: string;
  rightMeta?: React.ReactNode;
  elevated?: boolean;
  interactive?: boolean;
  onClick?: () => void;
  className?: string;
  bodyClassName?: string;
  children: React.ReactNode;
}

export const TuiCard: React.FC<TuiCardProps> = ({
  title,
  rightMeta,
  elevated = false,
  interactive = false,
  onClick,
  className = '',
  bodyClassName = '',
  children,
}) => {
  const surfaceClass = elevated
    ? 'bg-[var(--tui-surface-elevated)] border-[var(--tui-border-medium)] hover:border-[var(--tui-primary)]'
    : 'bg-[var(--tui-surface)] border-[var(--tui-border-subtle)] hover:border-[var(--tui-border-medium)]';

  return (
    <div
      onClick={onClick}
      className={`border p-4 flex flex-col ${surfaceClass} ${interactive ? 'cursor-pointer' : ''} ${className}`}
    >
      {(title || rightMeta) && (
        <div className="w-full shrink-0 mb-3 pb-2 border-b border-dashed border-[var(--tui-border-medium)]">
          <div className="flex flex-wrap items-center justify-between gap-2 text-[13px] font-bold text-[var(--tui-primary)] tracking-[0.04em]">
            <span className="min-w-0 truncate">{title}</span>
            {rightMeta && (
              <span className="text-[11px] font-medium text-[var(--tui-text-muted)] shrink-0">
                {rightMeta}
              </span>
            )}
          </div>
        </div>
      )}
      <div className={`w-full flex-1 min-w-0 ${bodyClassName}`}>{children}</div>
    </div>
  );
};

/* ============================================================================
 * TuiInput / TuiTextarea / TuiSelect
 * // label above with 4px gap, optional > prompt inside, sharp 1px borders.
 * ============================================================================ */
interface TuiInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  helperText?: string;
  error?: string;
  prompt?: boolean;
}

export const TuiInput: React.FC<TuiInputProps> = ({
  label,
  helperText,
  error,
  prompt = false,
  className = '',
  ...props
}) => {
  return (
    <div className="group flex flex-col gap-1 w-full">
      {label && (
        <label
          className={`text-[12px] font-medium leading-[1.4] ${
            error
              ? 'text-[var(--tui-error)]'
              : 'text-[var(--tui-text-muted)] group-focus-within:text-[var(--tui-primary)]'
          }`}
        >
          // {label}
        </label>
      )}
      <div
        className={`flex items-center bg-[var(--tui-bg)] border px-3 py-2 ${
          error
            ? 'border-[var(--tui-error)]'
            : 'border-[var(--tui-border-medium)] hover:border-[var(--tui-border-hover)] focus-within:border-[var(--tui-primary)]'
        } ${props.disabled ? 'opacity-50 bg-[var(--tui-surface)] border-[var(--tui-border-subtle)]' : ''}`}
      >
        {prompt && (
          <span className="text-[var(--tui-primary)] font-bold mr-2 select-none">&gt;</span>
        )}
        <input
          {...props}
          className={`w-full bg-transparent text-[13px] text-[var(--tui-text)] placeholder:text-[var(--tui-text-dim)] focus:outline-none disabled:cursor-not-allowed ${className}`}
        />
      </div>
      {(error || helperText) && (
        <span
          className={`text-[11px] leading-[1.4] ${
            error ? 'text-[var(--tui-error)]' : 'text-[var(--tui-text-dim)]'
          }`}
        >
          {error ? `[ERR] ${error}` : helperText}
        </span>
      )}
    </div>
  );
};

interface TuiTextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  rightAction?: React.ReactNode;
  error?: string;
}

export const TuiTextarea: React.FC<TuiTextareaProps> = ({
  label,
  rightAction,
  error,
  className = '',
  ...props
}) => {
  return (
    <div className="group flex flex-col gap-1 w-full">
      {(label || rightAction) && (
        <div className="flex items-center justify-between gap-2">
          {label && (
            <label
              className={`text-[12px] font-medium leading-[1.4] ${
                error
                  ? 'text-[var(--tui-error)]'
                  : 'text-[var(--tui-text-muted)] group-focus-within:text-[var(--tui-primary)]'
              }`}
            >
              // {label}
            </label>
          )}
          {rightAction}
        </div>
      )}
      <textarea
        {...props}
        className={`w-full bg-[var(--tui-bg)] border px-3 py-2 text-[13px] leading-[1.6] text-[var(--tui-text)] placeholder:text-[var(--tui-text-dim)] focus:outline-none ${
          error
            ? 'border-[var(--tui-error)]'
            : 'border-[var(--tui-border-medium)] hover:border-[var(--tui-border-hover)] focus:border-[var(--tui-primary)]'
        } ${className}`}
      />
    </div>
  );
};

interface TuiSelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
}

export const TuiSelect: React.FC<TuiSelectProps> = ({
  label,
  className = '',
  children,
  ...props
}) => {
  return (
    <div className="group flex flex-col gap-1 w-full">
      {label && (
        <label className="text-[12px] font-medium leading-[1.4] text-[var(--tui-text-muted)] group-focus-within:text-[var(--tui-primary)]">
          // {label}
        </label>
      )}
      <select
        {...props}
        className={`w-full h-[36px] bg-[var(--tui-bg)] border border-[var(--tui-border-medium)] hover:border-[var(--tui-border-hover)] focus:border-[var(--tui-primary)] px-3 py-1.5 text-[13px] text-[var(--tui-text)] focus:outline-none cursor-pointer ${className}`}
      >
        {children}
      </select>
    </div>
  );
};

/* ============================================================================
 * TuiChip
 * Filter & Status chips: square, transparent at rest, inverted when active.
 * ============================================================================ */
interface TuiChipProps {
  active?: boolean;
  tone?: 'primary' | 'secondary' | 'tertiary' | 'error' | 'muted';
  onClick?: () => void;
  className?: string;
  children: React.ReactNode;
}

const CHIP_TONES = {
  primary: 'text-[var(--tui-primary)] border-[var(--tui-primary)]',
  secondary: 'text-[var(--tui-secondary)] border-[var(--tui-secondary)]',
  tertiary: 'text-[var(--tui-tertiary)] border-[var(--tui-tertiary)]',
  error: 'text-[var(--tui-error)] border-[var(--tui-error)]',
  muted: 'text-[var(--tui-text-muted)] border-[var(--tui-border-medium)]',
};

export const TuiChip: React.FC<TuiChipProps> = ({
  active = false,
  tone = 'muted',
  onClick,
  className = '',
  children,
}) => {
  const Tag = onClick ? 'button' : 'span';
  return (
    <Tag
      onClick={onClick}
      className={`inline-flex items-center px-2 py-0.5 text-[11px] font-medium tracking-[0.06em] uppercase border ${
        active
          ? 'bg-[var(--tui-primary)] text-[var(--tui-primary-fg)] border-[var(--tui-primary)] font-bold'
          : `bg-transparent ${CHIP_TONES[tone]}`
      } ${onClick ? 'cursor-pointer hover:border-[var(--tui-primary)]' : ''} ${className}`}
    >
      {children}
    </Tag>
  );
};

/* ============================================================================
 * TuiPageHeader
 * Authentic CLI header with path command, 24px bold headline, and description.
 * ============================================================================ */
interface TuiPageHeaderProps {
  command: string;
  title: string;
  description: string;
  badge?: string;
  accent?: 'primary' | 'secondary' | 'tertiary';
}

export const TuiPageHeader: React.FC<TuiPageHeaderProps> = ({
  command,
  title,
  description,
  badge,
  accent = 'primary',
}) => {
  const accentColor =
    accent === 'secondary'
      ? 'text-[var(--tui-secondary)]'
      : accent === 'tertiary'
      ? 'text-[var(--tui-tertiary)]'
      : 'text-[var(--tui-primary)]';

  return (
    <div className="border-b border-dashed border-[var(--tui-border-medium)] pb-4 space-y-2">
      <div className="flex flex-wrap items-center justify-between gap-2 text-[12px]">
        <div className="text-[var(--tui-text-muted)]">
          <span className="text-[var(--tui-primary)] font-bold">$</span> ./{command}
        </div>
        {badge && <TuiChip tone={accent}>{badge}</TuiChip>}
      </div>
      <h2 className={`text-[24px] font-bold leading-[1.25] tracking-[0.01em] ${accentColor}`}>
        &gt; {title}
      </h2>
      <p className="text-[14px] leading-[1.6] text-[var(--tui-text-muted)]">{description}</p>
    </div>
  );
};

/* ============================================================================
 * TuiAlert
 * Semantic status notification box [OK] / [ERR] / [WARN] / [INFO]
 * ============================================================================ */
interface TuiAlertProps {
  type?: 'ok' | 'err' | 'warn' | 'info';
  children: React.ReactNode;
}

export const TuiAlert: React.FC<TuiAlertProps> = ({ type = 'ok', children }) => {
  const styles = {
    ok: 'border-[var(--tui-primary)] text-[var(--tui-primary)] bg-[var(--tui-surface)]',
    err: 'border-[var(--tui-error)] text-[var(--tui-error)] bg-[var(--tui-surface)]',
    warn: 'border-[var(--tui-secondary)] text-[var(--tui-secondary)] bg-[var(--tui-surface)]',
    info: 'border-[var(--tui-tertiary)] text-[var(--tui-tertiary)] bg-[var(--tui-surface)]',
  };
  const tags = { ok: '[OK]', err: '[ERR]', warn: '[WARN]', info: '[INFO]' };

  return (
    <div className={`border px-3 py-2 text-[12px] flex items-start gap-2 ${styles[type]}`}>
      <span className="font-bold shrink-0">{tags[type]}</span>
      <span>{children}</span>
    </div>
  );
};

/* ============================================================================
 * TuiCheckbox & TuiRadio
 * ASCII rendered checkboxes [ ] / [x] and radios ( ) / (*)
 * ============================================================================ */
interface TuiCheckboxProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  label: string;
  disabled?: boolean;
}

export const TuiCheckbox: React.FC<TuiCheckboxProps> = ({
  checked,
  onChange,
  label,
  disabled = false,
}) => (
  <button
    type="button"
    disabled={disabled}
    onClick={() => onChange(!checked)}
    className={`inline-flex items-center gap-2 text-[13px] cursor-pointer disabled:opacity-30 disabled:cursor-not-allowed ${
      checked ? 'text-[var(--tui-primary)]' : 'text-[var(--tui-text-muted)] hover:text-[var(--tui-text)]'
    }`}
  >
    <span className="font-bold">[{checked ? 'x' : ' '}]</span>
    <span>{label}</span>
  </button>
);
