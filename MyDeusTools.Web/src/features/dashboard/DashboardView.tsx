import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { TOOLS_CONFIG, ToolCategory } from '../../data/toolsData';
import { TuiCard, TuiChip, TuiInput, TuiButton } from '../../components/terminal/TerminalUI';

interface DashboardViewProps {
  onSelectTool: (toolId: string) => void;
}

export const DashboardView: React.FC<DashboardViewProps> = ({ onSelectTool }) => {
  const { t } = useI18n();
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<ToolCategory | 'all'>('all');

  const filteredTools = TOOLS_CONFIG.filter((tool) => {
    const matchesCategory = selectedCategory === 'all' || tool.category === selectedCategory;
    const toolName = t.nav[tool.nameKey]?.toLowerCase() || '';
    const toolDesc = t.descriptions[tool.descKey]?.toLowerCase() || '';
    const matchesSearch =
      toolName.includes(search.toLowerCase()) ||
      toolDesc.includes(search.toLowerCase()) ||
      tool.cmd.toLowerCase().includes(search.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  return (
    <div className="space-y-8 pb-12">
      {/* Terminal Hero Banner */}
      <TuiCard elevated className="p-6 md:p-8" bodyClassName="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-2 text-[11px] uppercase tracking-[0.1em]">
          <span className="text-[var(--tui-primary)] font-bold">{t.dashboard.systemBanner}</span>
          <span className="text-[var(--tui-text-muted)]">{t.dashboard.pidMeta}</span>
        </div>

        <pre className="text-[11px] sm:text-[13px] leading-[1.3] text-[var(--tui-primary)] overflow-x-auto select-none font-bold">
{`+=========================================================================+
|  __  __       ____                 _____           _                    |
| |  \\/  |_   _|  _ \\  ___ _   _ ___|_   _|__   ___ | |___                |
| | |\\/| | | | | | | |/ _ \\ | | / __| | |/ _ \\ / _ \\| / __|  WEB TERMINAL |
| | |  | | |_| | |_| |  __/ |_| \\__ \\ | | (_) | (_) | \\__ \\  SUITE v1.0.0 |
| |_|  |_|\\__, |____/ \\___|\\__,_|___/ |_|\\___/ \\___/|_|___/               |
|         |___/                                                           |
+=========================================================================+`}
        </pre>

        <div className="space-y-2">
          <h1 className="text-[24px] md:text-[32px] font-bold leading-[1.2] tracking-[0.01em] text-[var(--tui-text)]">
            {t.common.appName}
          </h1>
          <p className="text-[14px] leading-[1.6] text-[var(--tui-text-muted)] max-w-3xl">
            {t.common.tagline}
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2 pt-2 border-t border-dashed border-[var(--tui-border-subtle)]">
          <TuiChip tone="primary">{t.dashboard.chipPrivacy}</TuiChip>
          <TuiChip tone="secondary">{t.dashboard.chipWasm}</TuiChip>
          <TuiChip tone="tertiary">{t.dashboard.chipPwa}</TuiChip>
        </div>
      </TuiCard>

      {/* Command Search & Filter Chips */}
      <div className="flex flex-col md:flex-row gap-4 items-start md:items-end justify-between">
        <div className="w-full md:w-96">
          <TuiInput
            label={t.dashboard.grepLabel}
            prompt
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t.common.searchPlaceholder}
          />
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {(['all', 'productivity', 'developer', 'media', 'system'] as const).map((cat) => (
            <TuiChip
              key={cat}
              active={selectedCategory === cat}
              onClick={() => setSelectedCategory(cat)}
            >
              {t.categories[cat]}
            </TuiChip>
          ))}
        </div>
      </div>

      {/* Tool Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        {filteredTools.map((tool) => {
          const accentColor =
            tool.accent === 'secondary'
              ? 'text-[var(--tui-secondary)]'
              : tool.accent === 'tertiary'
              ? 'text-[var(--tui-tertiary)]'
              : 'text-[var(--tui-primary)]';

          return (
            <div
              key={tool.id}
              onClick={() => onSelectTool(tool.id)}
              className="group bg-[var(--tui-surface)] border border-[var(--tui-border-subtle)] hover:bg-[var(--tui-surface-elevated)] hover:border-[var(--tui-primary)] p-4 flex flex-col justify-between cursor-pointer"
            >
              <div>
                <div className="flex items-center justify-between gap-2 text-[12px] pb-2 mb-2 border-b border-dashed border-[var(--tui-border-subtle)] group-hover:border-[var(--tui-border-medium)]">
                  <span className={`font-bold ${accentColor}`}>{tool.asciiTag}</span>
                  <div className="flex items-center gap-2">
                    {tool.badge && <TuiChip tone={tool.accent}>{tool.badge}</TuiChip>}
                    <span className="text-[11px] text-[var(--tui-text-dim)]">{tool.index}</span>
                  </div>
                </div>

                <div className="space-y-1.5">
                  <h3 className="text-[15px] font-bold text-[var(--tui-text)] group-hover:text-[var(--tui-primary)]">
                    &gt; {t.nav[tool.nameKey]}
                  </h3>
                  <p className="text-[12px] leading-[1.5] text-[var(--tui-text-muted)] line-clamp-3">
                    {t.descriptions[tool.descKey]}
                  </p>
                </div>
              </div>

              <div className="mt-4 pt-2 border-t border-dashed border-[var(--tui-border-subtle)] flex items-center justify-between text-[11px]">
                <span className="text-[var(--tui-text-dim)] uppercase">./{tool.cmd}</span>
                <span className="text-[var(--tui-primary)] font-bold">
                  [{t.common.start.toUpperCase()}]
                </span>
              </div>
            </div>
          );
        })}
      </div>

      {filteredTools.length === 0 && (
        <TuiCard bodyClassName="text-center py-8 space-y-4">
          <div className="text-[14px] text-[var(--tui-error)]">
            [ERR] {t.dashboard.noMatches} &quot;{search}&quot;
          </div>
          <div>
            <TuiButton
              variant="secondary"
              size="sm"
              prefix=">"
              onClick={() => {
                setSearch('');
                setSelectedCategory('all');
              }}
            >
              {t.common.reset}
            </TuiButton>
          </div>
        </TuiCard>
      )}
    </div>
  );
};
