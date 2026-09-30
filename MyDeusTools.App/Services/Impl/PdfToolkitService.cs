using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace MyDeusTools.App.Services.Impl
{
    public class PdfToolkitService : IPdfToolkitService
    {
        public PdfFileInfo? GetPdfInfo(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var fi = new FileInfo(filePath);
                using var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);

                return new PdfFileInfo(
                    FilePath: filePath,
                    FileName: fi.Name,
                    FileSizeBytes: fi.Length,
                    FileSizeFormatted: FormatFileSize(fi.Length),
                    PageCount: doc.PageCount,
                    Title: doc.Info.Title ?? string.Empty,
                    Author: doc.Info.Author ?? string.Empty,
                    Subject: doc.Info.Subject ?? string.Empty,
                    Keywords: doc.Info.Keywords ?? string.Empty
                );
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> MergePdfsAsync(
            IEnumerable<string> sourceFilePaths,
            string outputFilePath,
            IProgress<double>? progress = null,
            CancellationToken ct = default)
        {
            var files = sourceFilePaths?.Where(File.Exists).ToList();
            if (files == null || files.Count == 0)
                return false;

            return await Task.Run(() =>
            {
                EnsureDirectory(outputFilePath);

                using var outputDoc = new PdfDocument();
                double total = files.Count;
                int current = 0;

                foreach (var file in files)
                {
                    ct.ThrowIfCancellationRequested();

                    using var inputDoc = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                    for (int i = 0; i < inputDoc.PageCount; i++)
                    {
                        ct.ThrowIfCancellationRequested();
                        outputDoc.AddPage(inputDoc.Pages[i]);
                    }

                    current++;
                    progress?.Report((current / total) * 100.0);
                }

                outputDoc.Save(outputFilePath);
                return true;
            }, ct);
        }

        public async Task<int> SplitPdfAsync(
            string sourceFilePath,
            string outputDirectory,
            PdfSplitMode mode,
            string pageRange,
            int splitEveryN,
            string baseFileName,
            IProgress<double>? progress = null,
            CancellationToken ct = default)
        {
            if (!File.Exists(sourceFilePath))
                return 0;

            if (string.IsNullOrWhiteSpace(outputDirectory))
                outputDirectory = Path.GetDirectoryName(sourceFilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Directory.CreateDirectory(outputDirectory);

            if (string.IsNullOrWhiteSpace(baseFileName))
                baseFileName = Path.GetFileNameWithoutExtension(sourceFilePath);

            return await Task.Run(() =>
            {
                using var inputDoc = PdfReader.Open(sourceFilePath, PdfDocumentOpenMode.Import);
                int totalPages = inputDoc.PageCount;
                if (totalPages == 0) return 0;

                int filesCreated = 0;

                switch (mode)
                {
                    case PdfSplitMode.PageRange:
                    {
                        var targetPageIndices = ParsePageRange(pageRange, totalPages);
                        if (targetPageIndices.Count == 0)
                            return 0;

                        using var outDoc = new PdfDocument();
                        for (int i = 0; i < targetPageIndices.Count; i++)
                        {
                            ct.ThrowIfCancellationRequested();
                            int pageIdx = targetPageIndices[i];
                            outDoc.AddPage(inputDoc.Pages[pageIdx]);
                            progress?.Report(((double)(i + 1) / targetPageIndices.Count) * 100.0);
                        }

                        string outPath = Path.Combine(outputDirectory, $"{baseFileName}_extracted.pdf");
                        outDoc.Save(outPath);
                        filesCreated = 1;
                        break;
                    }

                    case PdfSplitMode.SinglePages:
                    {
                        for (int i = 0; i < totalPages; i++)
                        {
                            ct.ThrowIfCancellationRequested();

                            using var outDoc = new PdfDocument();
                            outDoc.AddPage(inputDoc.Pages[i]);

                            string outPath = Path.Combine(outputDirectory, $"{baseFileName}_page_{i + 1}.pdf");
                            outDoc.Save(outPath);
                            filesCreated++;

                            progress?.Report(((double)(i + 1) / totalPages) * 100.0);
                        }
                        break;
                    }

                    case PdfSplitMode.EveryNPages:
                    {
                        int n = Math.Max(1, splitEveryN);
                        int partIndex = 1;

                        for (int i = 0; i < totalPages; i += n)
                        {
                            ct.ThrowIfCancellationRequested();

                            using var outDoc = new PdfDocument();
                            int end = Math.Min(i + n, totalPages);
                            for (int p = i; p < end; p++)
                            {
                                outDoc.AddPage(inputDoc.Pages[p]);
                            }

                            string outPath = Path.Combine(outputDirectory, $"{baseFileName}_part_{partIndex}.pdf");
                            outDoc.Save(outPath);
                            filesCreated++;
                            partIndex++;

                            progress?.Report(((double)end / totalPages) * 100.0);
                        }
                        break;
                    }
                }

                return filesCreated;
            }, ct);
        }

        public async Task<bool> ImagesToPdfAsync(
            IEnumerable<string> imagePaths,
            string outputFilePath,
            PageSizePreference pageSize = PageSizePreference.A4,
            PageOrientationPreference orientation = PageOrientationPreference.Auto,
            PageMarginPreference margin = PageMarginPreference.None,
            IProgress<double>? progress = null,
            CancellationToken ct = default)
        {
            var validImages = imagePaths?.Where(File.Exists).ToList();
            if (validImages == null || validImages.Count == 0)
                return false;

            return await Task.Run(() =>
            {
                EnsureDirectory(outputFilePath);

                using var doc = new PdfDocument();
                double total = validImages.Count;
                int current = 0;

                double marginPt = margin switch
                {
                    PageMarginPreference.Small => 20.0,
                    PageMarginPreference.Normal => 36.0,
                    _ => 0.0
                };

                foreach (var imgPath in validImages)
                {
                    ct.ThrowIfCancellationRequested();

                    using var image = XImage.FromFile(imgPath);
                    double imgWidthPt = image.PointWidth;
                    double imgHeightPt = image.PointHeight;

                    PdfPage page = doc.AddPage();

                    if (pageSize == PageSizePreference.FitImage)
                    {
                        page.Width = XUnit.FromPoint(imgWidthPt + (marginPt * 2));
                        page.Height = XUnit.FromPoint(imgHeightPt + (marginPt * 2));
                    }
                    else
                    {
                        // Base size: A4 or Letter
                        double baseWidth = pageSize == PageSizePreference.Letter ? 612.0 : 595.276;
                        double baseHeight = pageSize == PageSizePreference.Letter ? 792.0 : 841.890;

                        bool isLandscape = orientation switch
                        {
                            PageOrientationPreference.Landscape => true,
                            PageOrientationPreference.Portrait => false,
                            _ => imgWidthPt > imgHeightPt // Auto
                        };

                        page.Width = XUnit.FromPoint(isLandscape ? Math.Max(baseWidth, baseHeight) : Math.Min(baseWidth, baseHeight));
                        page.Height = XUnit.FromPoint(isLandscape ? Math.Min(baseWidth, baseHeight) : Math.Max(baseWidth, baseHeight));
                    }

                    using (var gfx = XGraphics.FromPdfPage(page))
                    {
                        double availWidth = Math.Max(10, page.Width.Point - (marginPt * 2));
                        double availHeight = Math.Max(10, page.Height.Point - (marginPt * 2));

                        // Scale image proportionally to fit inside available area
                        double scaleX = availWidth / imgWidthPt;
                        double scaleY = availHeight / imgHeightPt;
                        double scale = Math.Min(scaleX, scaleY);

                        double destWidth = imgWidthPt * scale;
                        double destHeight = imgHeightPt * scale;

                        double destX = marginPt + ((availWidth - destWidth) / 2.0);
                        double destY = marginPt + ((availHeight - destHeight) / 2.0);

                        gfx.DrawImage(image, destX, destY, destWidth, destHeight);
                    }

                    current++;
                    progress?.Report((current / total) * 100.0);
                }

                doc.Save(outputFilePath);
                return true;
            }, ct);
        }

        public async Task<bool> ProtectPdfAsync(
            string sourceFilePath,
            string outputFilePath,
            string userPassword,
            string? ownerPassword = null,
            string? title = null,
            string? author = null,
            string? subject = null,
            string? keywords = null,
            CancellationToken ct = default)
        {
            if (!File.Exists(sourceFilePath))
                return false;

            return await Task.Run(() =>
            {
                EnsureDirectory(outputFilePath);

                using var inDoc = PdfReader.Open(sourceFilePath, PdfDocumentOpenMode.Import);
                using var outDoc = new PdfDocument();

                for (int i = 0; i < inDoc.PageCount; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    outDoc.AddPage(inDoc.Pages[i]);
                }

                // Metadata
                outDoc.Info.Title = !string.IsNullOrWhiteSpace(title) ? title : inDoc.Info.Title;
                outDoc.Info.Author = !string.IsNullOrWhiteSpace(author) ? author : inDoc.Info.Author;
                outDoc.Info.Subject = !string.IsNullOrWhiteSpace(subject) ? subject : inDoc.Info.Subject;
                outDoc.Info.Keywords = !string.IsNullOrWhiteSpace(keywords) ? keywords : inDoc.Info.Keywords;

                // Passwords & Encryption
                if (!string.IsNullOrWhiteSpace(userPassword))
                {
                    outDoc.SecuritySettings.UserPassword = userPassword;
                    outDoc.SecuritySettings.OwnerPassword = !string.IsNullOrWhiteSpace(ownerPassword) ? ownerPassword : userPassword;
                }
                else if (!string.IsNullOrWhiteSpace(ownerPassword))
                {
                    outDoc.SecuritySettings.OwnerPassword = ownerPassword;
                }

                outDoc.Save(outputFilePath);
                return true;
            }, ct);
        }

        public static List<int> ParsePageRange(string rangeStr, int totalPages)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(rangeStr))
            {
                return Enumerable.Range(0, totalPages).ToList();
            }

            var parts = rangeStr.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                string p = part.Trim();
                if (p.Contains('-'))
                {
                    var dashParts = p.Split('-');
                    if (dashParts.Length == 2 &&
                        int.TryParse(dashParts[0].Trim(), out int start) &&
                        int.TryParse(dashParts[1].Trim(), out int end))
                    {
                        if (start > end)
                        {
                            (start, end) = (end, start);
                        }

                        start = Math.Max(1, start);
                        end = Math.Min(totalPages, end);

                        for (int i = start; i <= end; i++)
                        {
                            int idx = i - 1;
                            if (!result.Contains(idx))
                                result.Add(idx);
                        }
                    }
                }
                else if (int.TryParse(p, out int pageNum))
                {
                    if (pageNum >= 1 && pageNum <= totalPages)
                    {
                        int idx = pageNum - 1;
                        if (!result.Contains(idx))
                            result.Add(idx);
                    }
                }
            }

            return result;
        }

        public static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{(bytes / 1024.0).ToString("F1", CultureInfo.InvariantCulture)} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{(bytes / (1024.0 * 1024.0)).ToString("F2", CultureInfo.InvariantCulture)} MB";
            return $"{(bytes / (1024.0 * 1024.0 * 1024.0)).ToString("F2", CultureInfo.InvariantCulture)} GB";
        }

        private static void EnsureDirectory(string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
