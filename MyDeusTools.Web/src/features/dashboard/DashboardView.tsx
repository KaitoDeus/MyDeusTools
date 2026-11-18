import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { TOOLS_CONFIG, ToolCategory } from '../../data/toolsData';
import { Search, ShieldCheck, Zap, Sparkles, ArrowRight, Laptop } from 'lucide-react';

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
      toolName.includes(search.toLowerCase()) || toolDesc.includes(search.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  return (
    <div className="space-y-8 pb-12">
      {/* Hero Banner */}
      <div className="relative overflow-hidden rounded-2xl bg-gradient-to-br from-indigo-50 via-white to-purple-50 dark:from-indigo-950/70 dark:via-slate-900/80 dark:to-purple-950/70 p-8 border border-indigo-200/80 dark:border-indigo-500/20 shadow-xl shadow-indigo-500/5 dark:shadow-2xl backdrop-blur-xl">
        <div className="absolute top-0 right-0 -mr-16 -mt-16 w-80 h-80 rounded-full bg-indigo-500/10 blur-3xl pointer-events-none" />
        <div className="absolute bottom-0 left-0 -ml-16 -mb-16 w-80 h-80 rounded-full bg-purple-500/10 blur-3xl pointer-events-none" />

        <div className="relative z-10 max-w-3xl space-y-4">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-indigo-500/10 border border-indigo-500/25 dark:border-indigo-500/30 text-indigo-600 dark:text-indigo-400 text-xs font-semibold tracking-wide uppercase">
            <Sparkles className="w-3.5 h-3.5" />
            <span>Pure Web Architecture 2026</span>
          </div>

          <h1 className="text-3xl md:text-5xl font-extrabold tracking-tight text-slate-900 dark:text-white leading-tight">
            {t.common.appName}
          </h1>

          <p className="text-base md:text-lg text-slate-600 dark:text-slate-300 font-normal leading-relaxed">
            {t.common.tagline}
          </p>

          <div className="flex flex-wrap items-center gap-3 pt-2 text-xs text-slate-600 dark:text-slate-400">
            <div className="flex items-center gap-1.5 bg-white/80 dark:bg-slate-800/60 px-3 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700/50 shadow-2xs dark:shadow-none">
              <ShieldCheck className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
              <span>100% Client-Side Privacy</span>
            </div>
            <div className="flex items-center gap-1.5 bg-white/80 dark:bg-slate-800/60 px-3 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700/50 shadow-2xs dark:shadow-none">
              <Zap className="w-4 h-4 text-amber-500 dark:text-amber-400" />
              <span>WebAssembly & Web Workers</span>
            </div>
            <div className="flex items-center gap-1.5 bg-white/80 dark:bg-slate-800/60 px-3 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700/50 shadow-2xs dark:shadow-none">
              <Laptop className="w-4 h-4 text-blue-600 dark:text-blue-400" />
              <span>PWA Offline Ready</span>
            </div>
          </div>
        </div>
      </div>

      {/* Search & Filter Bar */}
      <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
        {/* Search Input */}
        <div className="relative w-full md:w-96">
          <Search className="w-4 h-4 absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t.common.searchPlaceholder}
            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 rounded-xl text-sm text-slate-900 dark:text-slate-200 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500/50 transition-all shadow-xs dark:shadow-inner"
          />
        </div>

        {/* Category Filter Pills */}
        <div className="flex items-center gap-1.5 overflow-x-auto w-full md:w-auto pb-1 scrollbar-none">
          {(['all', 'productivity', 'developer', 'media', 'system'] as const).map((cat) => (
            <button
              key={cat}
              onClick={() => setSelectedCategory(cat)}
              className={`px-3.5 py-1.5 rounded-lg text-xs font-medium transition-all whitespace-nowrap cursor-pointer ${
                selectedCategory === cat
                  ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/25 border border-indigo-500'
                  : 'bg-white text-slate-600 hover:text-slate-900 hover:bg-slate-100 border border-slate-200 dark:bg-slate-900/60 dark:text-slate-400 dark:hover:text-slate-200 dark:hover:bg-slate-800/80 dark:border-slate-800/60'
              }`}
            >
              {t.categories[cat]}
            </button>
          ))}
        </div>
      </div>

      {/* Tool Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
        {filteredTools.map((tool) => {
          const Icon = tool.icon;
          return (
            <div
              key={tool.id}
              onClick={() => onSelectTool(tool.id)}
              className="group relative flex flex-col justify-between p-5 rounded-2xl glass-card glass-card-hover cursor-pointer transition-all duration-200"
            >
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div
                    className={`w-12 h-12 rounded-xl bg-gradient-to-br ${tool.gradient} p-2.5 text-white shadow-lg group-hover:scale-105 transition-transform duration-200 flex items-center justify-center`}
                  >
                    <Icon className="w-6 h-6" />
                  </div>
                  {tool.badge && (
                    <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded-full bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 border border-indigo-500/20">
                      {tool.badge}
                    </span>
                  )}
                </div>

                <div className="space-y-1.5">
                  <h3 className="font-semibold text-slate-900 dark:text-slate-100 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors text-base flex items-center justify-between">
                    <span>{t.nav[tool.nameKey]}</span>
                    <ArrowRight className="w-4 h-4 text-slate-400 dark:text-slate-500 opacity-0 -translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all duration-200" />
                  </h3>
                  <p className="text-xs text-slate-600 dark:text-slate-400 line-clamp-2 leading-relaxed">
                    {t.descriptions[tool.descKey]}
                  </p>
                </div>
              </div>

              <div className="mt-4 pt-3 border-t border-slate-100 dark:border-slate-800/50 flex items-center justify-between text-[11px] text-slate-500">
                <span className="capitalize">{tool.category}</span>
                <span className="group-hover:text-indigo-600 dark:group-hover:text-slate-300 transition-colors font-medium">
                  {t.common.start} &rarr;
                </span>
              </div>
            </div>
          );
        })}
      </div>

      {filteredTools.length === 0 && (
        <div className="text-center py-16 space-y-3">
          <p className="text-slate-500 dark:text-slate-400 text-sm">
            Không tìm thấy công cụ phù hợp với từ khóa.
          </p>
          <button
            onClick={() => {
              setSearch('');
              setSelectedCategory('all');
            }}
            className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer"
          >
            Đặt lại bộ lọc
          </button>
        </div>
      )}
    </div>
  );
};
