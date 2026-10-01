using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Services.Impl
{
    public enum RenameTargetType
    {
        FilesAndFolders,
        FilesOnly,
        FoldersOnly
    }

    public enum CaseConversionMode
    {
        None,
        Lower,
        Upper,
        TitleCase,
        SentenceCase,
        CamelCase,
        PascalCase,
        SnakeCase,
        KebabCase
    }

    public enum NumberingPlacement
    {
        Prefix,
        Suffix,
        Replace
    }

    public enum DatePlacement
    {
        None,
        Prefix,
        Suffix
    }

    public enum DateSource
    {
        CurrentDate,
        DateModified,
        DateCreated
    }

    public enum RenameItemStatus
    {
        Ready,
        Unchanged,
        InvalidCharacters,
        ConflictDuplicate,
        ConflictExists,
        Renamed,
        Failed,
        Reverted
    }

    public class RenameConfig
    {
        public RenameTargetType TargetType { get; set; } = RenameTargetType.FilesAndFolders;
        public bool IncludeExtension { get; set; } = false;

        // Find & Replace
        public string FindText { get; set; } = string.Empty;
        public string ReplaceText { get; set; } = string.Empty;
        public bool MatchCase { get; set; } = false;
        public bool UseRegex { get; set; } = false;

        // Insert & Remove
        public string Prefix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public string InsertText { get; set; } = string.Empty;
        public int InsertPosition { get; set; } = 0; // 0 = disabled, 1-based index
        public int RemoveFirstN { get; set; } = 0;
        public int RemoveLastN { get; set; } = 0;
        public bool TrimWhitespace { get; set; } = false;

        // Case conversion
        public CaseConversionMode CaseMode { get; set; } = CaseConversionMode.None;

        // Numbering
        public bool EnableNumbering { get; set; } = false;
        public NumberingPlacement NumberPlacement { get; set; } = NumberingPlacement.Suffix;
        public int StartNumber { get; set; } = 1;
        public int NumberStep { get; set; } = 1;
        public int ZeroPadding { get; set; } = 2; // e.g. 01, 001
        public string NumberSeparator { get; set; } = "_";

        // Date & Time
        public DatePlacement DatePlacement { get; set; } = DatePlacement.None;
        public DateSource DateSource { get; set; } = DateSource.CurrentDate;
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string DateSeparator { get; set; } = "_";
    }

    public class RenameResult
    {
        public int TotalItems { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; } = new();
        public List<RenameHistoryEntry> History { get; } = new();
    }

    public class RenameHistoryEntry
    {
        public string OriginalPath { get; set; } = string.Empty;
        public string NewPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public interface IBulkRenamerService
    {
        string ApplyRenameRule(
            string originalName,
            bool isDirectory,
            RenameConfig config,
            int itemIndex,
            DateTime? dateCreated = null,
            DateTime? dateModified = null);

        void UpdatePreviews(IList<RenameItemViewModel> items, RenameConfig config);

        Task<RenameResult> ExecuteRenameAsync(
            IList<RenameItemViewModel> items,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);

        Task<RenameResult> UndoRenameAsync(
            IList<RenameHistoryEntry> history,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);

        IReadOnlyList<string> ScanPaths(IEnumerable<string> paths, bool recursive, RenameTargetType targetType);
    }
}
