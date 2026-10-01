using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Services.Impl
{
    public class BulkRenamerService : IBulkRenamerService
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        public string ApplyRenameRule(
            string originalName,
            bool isDirectory,
            RenameConfig config,
            int itemIndex,
            DateTime? dateCreated = null,
            DateTime? dateModified = null)
        {
            if (string.IsNullOrEmpty(originalName))
                return string.Empty;

            // Target filtering
            if (config.TargetType == RenameTargetType.FilesOnly && isDirectory)
                return originalName;
            if (config.TargetType == RenameTargetType.FoldersOnly && !isDirectory)
                return originalName;

            string baseName;
            string extension;

            if (isDirectory)
            {
                baseName = originalName;
                extension = string.Empty;
            }
            else
            {
                if (config.IncludeExtension)
                {
                    baseName = originalName;
                    extension = string.Empty;
                }
                else
                {
                    baseName = Path.GetFileNameWithoutExtension(originalName);
                    extension = Path.GetExtension(originalName);
                }
            }

            // 1. Find & Replace
            if (!string.IsNullOrEmpty(config.FindText))
            {
                if (config.UseRegex)
                {
                    try
                    {
                        var regexOptions = config.MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                        baseName = Regex.Replace(baseName, config.FindText, config.ReplaceText ?? string.Empty, regexOptions);
                    }
                    catch (ArgumentException)
                    {
                        // Invalid regex - keep baseName unchanged
                    }
                }
                else
                {
                    baseName = ReplaceString(
                        baseName,
                        config.FindText,
                        config.ReplaceText ?? string.Empty,
                        config.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                }
            }

            // 2. Character Removal & Trimming
            if (config.RemoveFirstN > 0)
            {
                baseName = config.RemoveFirstN >= baseName.Length
                    ? string.Empty
                    : baseName.Substring(config.RemoveFirstN);
            }

            if (config.RemoveLastN > 0)
            {
                baseName = config.RemoveLastN >= baseName.Length
                    ? string.Empty
                    : baseName.Substring(0, baseName.Length - config.RemoveLastN);
            }

            if (config.TrimWhitespace)
            {
                baseName = baseName.Trim();
            }

            // 3. Text Insertion
            if (!string.IsNullOrEmpty(config.InsertText) && config.InsertPosition > 0)
            {
                int insertIdx = Math.Clamp(config.InsertPosition - 1, 0, baseName.Length);
                baseName = baseName.Insert(insertIdx, config.InsertText);
            }

            if (!string.IsNullOrEmpty(config.Prefix))
            {
                baseName = config.Prefix + baseName;
            }

            if (!string.IsNullOrEmpty(config.Suffix))
            {
                baseName = baseName + config.Suffix;
            }

            // 4. Case Conversion
            baseName = ApplyCaseConversion(baseName, config.CaseMode);

            // 5. Numbering
            if (config.EnableNumbering)
            {
                int num = config.StartNumber + (itemIndex * config.NumberStep);
                string numStr = config.ZeroPadding > 0
                    ? num.ToString(new string('0', Math.Clamp(config.ZeroPadding, 1, 10)))
                    : num.ToString();

                string sep = config.NumberSeparator ?? string.Empty;

                switch (config.NumberPlacement)
                {
                    case NumberingPlacement.Replace:
                        baseName = numStr;
                        break;
                    case NumberingPlacement.Prefix:
                        baseName = string.IsNullOrEmpty(baseName) ? numStr : (numStr + sep + baseName);
                        break;
                    case NumberingPlacement.Suffix:
                        baseName = string.IsNullOrEmpty(baseName) ? numStr : (baseName + sep + numStr);
                        break;
                }
            }

            // 6. Date & Time
            if (config.DatePlacement != DatePlacement.None)
            {
                DateTime dt = config.DateSource switch
                {
                    DateSource.DateModified => dateModified ?? DateTime.Now,
                    DateSource.DateCreated => dateCreated ?? DateTime.Now,
                    _ => DateTime.Now
                };

                string format = string.IsNullOrWhiteSpace(config.DateFormat) ? "yyyy-MM-dd" : config.DateFormat;
                string dateStr;
                try
                {
                    dateStr = dt.ToString(format, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    dateStr = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                string dateSep = config.DateSeparator ?? string.Empty;

                if (config.DatePlacement == DatePlacement.Prefix)
                {
                    baseName = string.IsNullOrEmpty(baseName) ? dateStr : (dateStr + dateSep + baseName);
                }
                else if (config.DatePlacement == DatePlacement.Suffix)
                {
                    baseName = string.IsNullOrEmpty(baseName) ? dateStr : (baseName + dateSep + dateStr);
                }
            }

            return baseName + extension;
        }

        public void UpdatePreviews(IList<RenameItemViewModel> items, RenameConfig config)
        {
            if (items == null || items.Count == 0)
                return;

            // 1. Calculate new names
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.Index = i + 1;
                item.NewName = ApplyRenameRule(
                    item.OriginalName,
                    item.IsDirectory,
                    config,
                    i,
                    item.DateCreated,
                    item.DateModified);

                item.NewPath = Path.Combine(item.DirectoryPath, item.NewName);
            }

            // 2. Validate conflicts, invalid characters, duplicates
            var groups = items
                .GroupBy(it => (it.DirectoryPath.ToLowerInvariant(), it.NewName.ToLowerInvariant()))
                .ToList();

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.NewName))
                {
                    item.Status = RenameItemStatus.InvalidCharacters;
                    item.StatusMessage = "Tên không được để trống";
                    continue;
                }

                if (item.NewName.IndexOfAny(InvalidFileNameChars) >= 0)
                {
                    item.Status = RenameItemStatus.InvalidCharacters;
                    item.StatusMessage = "Chứa ký tự không hợp lệ";
                    continue;
                }

                // Check duplicate within the same batch & directory
                var groupKey = (item.DirectoryPath.ToLowerInvariant(), item.NewName.ToLowerInvariant());
                var matchGroup = groups.FirstOrDefault(g => g.Key == groupKey);
                if (matchGroup != null && matchGroup.Count() > 1)
                {
                    item.Status = RenameItemStatus.ConflictDuplicate;
                    item.StatusMessage = "Trùng tên trong danh sách";
                    continue;
                }

                if (string.Equals(item.OriginalName, item.NewName, StringComparison.Ordinal))
                {
                    item.Status = RenameItemStatus.Unchanged;
                    item.StatusMessage = "Giữ nguyên";
                    continue;
                }

                // Check if target already exists on disk (and is not itself)
                bool targetExists = item.IsDirectory
                    ? Directory.Exists(item.NewPath)
                    : File.Exists(item.NewPath);

                if (targetExists && !string.Equals(item.OriginalPath, item.NewPath, StringComparison.OrdinalIgnoreCase))
                {
                    item.Status = RenameItemStatus.ConflictExists;
                    item.StatusMessage = "Tệp đích đã tồn tại trên đĩa";
                    continue;
                }

                item.Status = RenameItemStatus.Ready;
                item.StatusMessage = "Sẵn sàng";
            }
        }

        public async Task<RenameResult> ExecuteRenameAsync(
            IList<RenameItemViewModel> items,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var result = new RenameResult { TotalItems = items.Count };
            var readyItems = items.Where(it => it.Status == RenameItemStatus.Ready).ToList();

            if (readyItems.Count == 0)
            {
                result.SkippedCount = items.Count;
                return result;
            }

            return await Task.Run(() =>
            {
                int processed = 0;

                // Sort: for directories, deeper paths first so renaming parents doesn't invalidate children
                var sorted = readyItems
                    .OrderByDescending(it => it.IsDirectory)
                    .ThenByDescending(it => it.OriginalPath.Length)
                    .ToList();

                foreach (var item in sorted)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (item.IsDirectory)
                        {
                            RenameDirectorySafe(item.OriginalPath, item.NewPath);
                        }
                        else
                        {
                            RenameFileSafe(item.OriginalPath, item.NewPath);
                        }

                        result.History.Add(new RenameHistoryEntry
                        {
                            OriginalPath = item.OriginalPath,
                            NewPath = item.NewPath,
                            IsDirectory = item.IsDirectory,
                            Timestamp = DateTime.Now
                        });

                        item.OriginalPath = item.NewPath;
                        item.OriginalName = item.NewName;
                        item.OriginalBaseName = item.IsDirectory
                            ? item.NewName
                            : Path.GetFileNameWithoutExtension(item.NewName);
                        item.OriginalExtension = item.IsDirectory
                            ? string.Empty
                            : Path.GetExtension(item.NewName);

                        item.Status = RenameItemStatus.Renamed;
                        item.StatusMessage = "Đã đổi tên";
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        item.Status = RenameItemStatus.Failed;
                        item.StatusMessage = ex.Message;
                        result.FailedCount++;
                        result.Errors.Add($"{item.OriginalName}: {ex.Message}");
                    }

                    processed++;
                    progress?.Report((double)processed / sorted.Count * 100.0);
                }

                result.SkippedCount = items.Count - readyItems.Count;
                return result;
            }, cancellationToken);
        }

        public async Task<RenameResult> UndoRenameAsync(
            IList<RenameHistoryEntry> history,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var result = new RenameResult { TotalItems = history.Count };

            return await Task.Run(() =>
            {
                // Revert in reverse order
                var reversed = history.AsEnumerable().Reverse().ToList();
                int processed = 0;

                foreach (var entry in reversed)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (entry.IsDirectory)
                        {
                            if (Directory.Exists(entry.NewPath))
                            {
                                RenameDirectorySafe(entry.NewPath, entry.OriginalPath);
                                result.SuccessCount++;
                            }
                            else
                            {
                                result.FailedCount++;
                                result.Errors.Add($"Thư mục không tồn tại: {entry.NewPath}");
                            }
                        }
                        else
                        {
                            if (File.Exists(entry.NewPath))
                            {
                                RenameFileSafe(entry.NewPath, entry.OriginalPath);
                                result.SuccessCount++;
                            }
                            else
                            {
                                result.FailedCount++;
                                result.Errors.Add($"Tệp không tồn tại: {entry.NewPath}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(ex.Message);
                    }

                    processed++;
                    progress?.Report((double)processed / reversed.Count * 100.0);
                }

                return result;
            }, cancellationToken);
        }

        public IReadOnlyList<string> ScanPaths(IEnumerable<string> paths, bool recursive, RenameTargetType targetType)
        {
            var results = new List<string>();

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    if (targetType != RenameTargetType.FoldersOnly)
                    {
                        results.Add(path);
                    }
                }
                else if (Directory.Exists(path))
                {
                    if (targetType != RenameTargetType.FilesOnly)
                    {
                        results.Add(path);
                    }

                    try
                    {
                        var opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                        if (targetType != RenameTargetType.FoldersOnly)
                        {
                            results.AddRange(Directory.EnumerateFiles(path, "*", opt));
                        }

                        if (targetType != RenameTargetType.FilesOnly && recursive)
                        {
                            results.AddRange(Directory.EnumerateDirectories(path, "*", opt));
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignore inaccessible folders
                    }
                }
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void RenameFileSafe(string source, string destination)
        {
            if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
            {
                // Case-only rename on Windows requires temporary intermediate move
                string temp = source + ".tmp_" + Guid.NewGuid().ToString("N");
                File.Move(source, temp);
                File.Move(temp, destination);
            }
            else
            {
                File.Move(source, destination);
            }
        }

        private static void RenameDirectorySafe(string source, string destination)
        {
            if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
            {
                string temp = source + "_tmp_" + Guid.NewGuid().ToString("N");
                Directory.Move(source, temp);
                Directory.Move(temp, destination);
            }
            else
            {
                Directory.Move(source, destination);
            }
        }

        private static string ReplaceString(string text, string search, string replace, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
                return text;

            var sb = new StringBuilder();
            int previousIndex = 0;
            int index = text.IndexOf(search, comparison);

            while (index >= 0)
            {
                sb.Append(text.Substring(previousIndex, index - previousIndex));
                sb.Append(replace);
                previousIndex = index + search.Length;
                index = text.IndexOf(search, previousIndex, comparison);
            }

            sb.Append(text.Substring(previousIndex));
            return sb.ToString();
        }

        private static string ApplyCaseConversion(string text, CaseConversionMode mode)
        {
            if (string.IsNullOrEmpty(text) || mode == CaseConversionMode.None)
                return text;

            return mode switch
            {
                CaseConversionMode.Lower => text.ToLowerInvariant(),
                CaseConversionMode.Upper => text.ToUpperInvariant(),
                CaseConversionMode.TitleCase => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLowerInvariant()),
                CaseConversionMode.SentenceCase => ToSentenceCase(text),
                CaseConversionMode.CamelCase => ToCamelCase(text),
                CaseConversionMode.PascalCase => ToPascalCase(text),
                CaseConversionMode.SnakeCase => ToSnakeCase(text),
                CaseConversionMode.KebabCase => ToKebabCase(text),
                _ => text
            };
        }

        private static string ToSentenceCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var lower = text.ToLowerInvariant();
            return char.ToUpperInvariant(lower[0]) + (lower.Length > 1 ? lower.Substring(1) : string.Empty);
        }

        private static List<string> SplitIntoWords(string text)
        {
            // Split by underscores, hyphens, spaces, and camelCase boundaries
            var pattern = @"([A-Z]+(?=[A-Z][a-z])|[A-Z][a-z]+|[a-z]+|[0-9]+)";
            var matches = Regex.Matches(text, pattern);
            var words = new List<string>();

            foreach (Match match in matches)
            {
                if (!string.IsNullOrWhiteSpace(match.Value))
                {
                    words.Add(match.Value);
                }
            }

            if (words.Count == 0 && !string.IsNullOrWhiteSpace(text))
            {
                words.AddRange(text.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries));
            }

            return words;
        }

        private static string ToCamelCase(string text)
        {
            var words = SplitIntoWords(text);
            if (words.Count == 0) return text;

            var sb = new StringBuilder();
            sb.Append(words[0].ToLowerInvariant());

            for (int i = 1; i < words.Count; i++)
            {
                var w = words[i].ToLowerInvariant();
                sb.Append(char.ToUpperInvariant(w[0])).Append(w.Length > 1 ? w.Substring(1) : string.Empty);
            }

            return sb.ToString();
        }

        private static string ToPascalCase(string text)
        {
            var words = SplitIntoWords(text);
            if (words.Count == 0) return text;

            var sb = new StringBuilder();
            for (int i = 0; i < words.Count; i++)
            {
                var w = words[i].ToLowerInvariant();
                sb.Append(char.ToUpperInvariant(w[0])).Append(w.Length > 1 ? w.Substring(1) : string.Empty);
            }

            return sb.ToString();
        }

        private static string ToSnakeCase(string text)
        {
            var words = SplitIntoWords(text);
            if (words.Count == 0) return text;

            return string.Join("_", words.Select(w => w.ToLowerInvariant()));
        }

        private static string ToKebabCase(string text)
        {
            var words = SplitIntoWords(text);
            if (words.Count == 0) return text;

            return string.Join("-", words.Select(w => w.ToLowerInvariant()));
        }
    }
}
