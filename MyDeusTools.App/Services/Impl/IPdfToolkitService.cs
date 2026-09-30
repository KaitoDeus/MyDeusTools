using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyDeusTools.App.Services.Impl
{
    public record PdfFileInfo(
        string FilePath,
        string FileName,
        long FileSizeBytes,
        string FileSizeFormatted,
        int PageCount,
        string Title,
        string Author,
        string Subject,
        string Keywords
    );

    public enum PageOrientationPreference
    {
        Auto,
        Portrait,
        Landscape
    }

    public enum PageSizePreference
    {
        A4,
        Letter,
        FitImage
    }

    public enum PageMarginPreference
    {
        None,
        Small,
        Normal
    }

    public enum PdfSplitMode
    {
        PageRange,
        SinglePages,
        EveryNPages
    }

    public interface IPdfToolkitService
    {
        PdfFileInfo? GetPdfInfo(string filePath);

        Task<bool> MergePdfsAsync(
            IEnumerable<string> sourceFilePaths,
            string outputFilePath,
            IProgress<double>? progress = null,
            CancellationToken ct = default);

        Task<int> SplitPdfAsync(
            string sourceFilePath,
            string outputDirectory,
            PdfSplitMode mode,
            string pageRange,
            int splitEveryN,
            string baseFileName,
            IProgress<double>? progress = null,
            CancellationToken ct = default);

        Task<bool> ImagesToPdfAsync(
            IEnumerable<string> imagePaths,
            string outputFilePath,
            PageSizePreference pageSize = PageSizePreference.A4,
            PageOrientationPreference orientation = PageOrientationPreference.Auto,
            PageMarginPreference margin = PageMarginPreference.None,
            IProgress<double>? progress = null,
            CancellationToken ct = default);

        Task<bool> ProtectPdfAsync(
            string sourceFilePath,
            string outputFilePath,
            string userPassword,
            string? ownerPassword = null,
            string? title = null,
            string? author = null,
            string? subject = null,
            string? keywords = null,
            CancellationToken ct = default);
    }
}
