using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MyDeusTools.App.Services.Impl;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Xunit;

namespace MyDeusTools.Tests
{
    public class PdfToolkitServiceTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly PdfToolkitService _service;

        public PdfToolkitServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"mydeustools_pdftests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
            _service = new PdfToolkitService();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                try { Directory.Delete(_tempDir, true); } catch { }
            }
        }

        private string CreateSamplePdf(string name, int pages, string? title = null, string? author = null)
        {
            string path = Path.Combine(_tempDir, name);
            using (var doc = new PdfDocument())
            {
                if (!string.IsNullOrEmpty(title)) doc.Info.Title = title;
                if (!string.IsNullOrEmpty(author)) doc.Info.Author = author;

                for (int i = 0; i < pages; i++)
                {
                    doc.AddPage();
                }
                doc.Save(path);
            }
            return path;
        }

        private string CreateSampleImage(string name, int width = 100, int height = 100)
        {
            string path = Path.Combine(_tempDir, name);
            var rtb = new RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));
            using (var fs = File.Create(path))
            {
                enc.Save(fs);
            }
            return path;
        }

        [Fact]
        public void GetPdfInfo_ShouldReturnCorrectMetadata_WhenFileExists()
        {
            string pdfPath = CreateSamplePdf("info_sample.pdf", 4, "Test Document", "John Doe");

            var info = _service.GetPdfInfo(pdfPath);

            Assert.NotNull(info);
            Assert.Equal("info_sample.pdf", info.FileName);
            Assert.Equal(4, info.PageCount);
            Assert.Equal("Test Document", info.Title);
            Assert.Equal("John Doe", info.Author);
            Assert.True(info.FileSizeBytes > 0);
        }

        [Fact]
        public void GetPdfInfo_ShouldReturnNull_WhenFileDoesNotExist()
        {
            var info = _service.GetPdfInfo(Path.Combine(_tempDir, "non_existent.pdf"));
            Assert.Null(info);
        }

        [Fact]
        public async Task MergePdfsAsync_ShouldCombineMultiplePdfsIntoSingleDocument()
        {
            string f1 = CreateSamplePdf("merge_1.pdf", 2);
            string f2 = CreateSamplePdf("merge_2.pdf", 3);
            string outPdf = Path.Combine(_tempDir, "merged_output.pdf");

            bool success = await _service.MergePdfsAsync(new[] { f1, f2 }, outPdf);

            Assert.True(success);
            Assert.True(File.Exists(outPdf));

            using var readDoc = PdfReader.Open(outPdf, PdfDocumentOpenMode.Import);
            Assert.Equal(5, readDoc.PageCount);
        }

        [Fact]
        public async Task SplitPdfAsync_WithPageRange_ShouldExtractSpecifiedPages()
        {
            string source = CreateSamplePdf("split_range.pdf", 6);
            string outDir = Path.Combine(_tempDir, "split_range_out");

            int count = await _service.SplitPdfAsync(
                source,
                outDir,
                PdfSplitMode.PageRange,
                "1-2, 5",
                0,
                "sample_split");

            Assert.Equal(1, count);
            string extractedFile = Path.Combine(outDir, "sample_split_extracted.pdf");
            Assert.True(File.Exists(extractedFile));

            using var readDoc = PdfReader.Open(extractedFile, PdfDocumentOpenMode.Import);
            Assert.Equal(3, readDoc.PageCount);
        }

        [Fact]
        public async Task SplitPdfAsync_WithSinglePages_ShouldCreateFilePerEachPage()
        {
            string source = CreateSamplePdf("split_single.pdf", 3);
            string outDir = Path.Combine(_tempDir, "split_single_out");

            int count = await _service.SplitPdfAsync(
                source,
                outDir,
                PdfSplitMode.SinglePages,
                "",
                0,
                "split_page");

            Assert.Equal(3, count);
            Assert.True(File.Exists(Path.Combine(outDir, "split_page_page_1.pdf")));
            Assert.True(File.Exists(Path.Combine(outDir, "split_page_page_2.pdf")));
            Assert.True(File.Exists(Path.Combine(outDir, "split_page_page_3.pdf")));
        }

        [Fact]
        public async Task SplitPdfAsync_WithEveryNPages_ShouldChunkPagesCorrectly()
        {
            string source = CreateSamplePdf("split_chunks.pdf", 5);
            string outDir = Path.Combine(_tempDir, "split_chunks_out");

            int count = await _service.SplitPdfAsync(
                source,
                outDir,
                PdfSplitMode.EveryNPages,
                "",
                2,
                "part");

            // 5 pages split every 2 pages -> Part 1 (2 pages), Part 2 (2 pages), Part 3 (1 page) = 3 files
            Assert.Equal(3, count);
            Assert.True(File.Exists(Path.Combine(outDir, "part_part_1.pdf")));
            Assert.True(File.Exists(Path.Combine(outDir, "part_part_2.pdf")));
            Assert.True(File.Exists(Path.Combine(outDir, "part_part_3.pdf")));
        }

        [Fact]
        public async Task ImagesToPdfAsync_ShouldCreatePdfContainingAllImages()
        {
            string img1 = CreateSampleImage("img1.png", 200, 300);
            string img2 = CreateSampleImage("img2.png", 400, 200);
            string outPdf = Path.Combine(_tempDir, "images_output.pdf");

            bool success = await _service.ImagesToPdfAsync(
                new[] { img1, img2 },
                outPdf,
                PageSizePreference.A4,
                PageOrientationPreference.Auto,
                PageMarginPreference.Small);

            Assert.True(success);
            Assert.True(File.Exists(outPdf));

            using var readDoc = PdfReader.Open(outPdf, PdfDocumentOpenMode.Import);
            Assert.Equal(2, readDoc.PageCount);
        }

        [Fact]
        public async Task ProtectPdfAsync_ShouldSetPasswordAndMetadata()
        {
            string source = CreateSamplePdf("protect_src.pdf", 2);
            string outPdf = Path.Combine(_tempDir, "protect_output.pdf");

            bool success = await _service.ProtectPdfAsync(
                source,
                outPdf,
                userPassword: "secretPassword123",
                ownerPassword: "ownerSecret456",
                title: "Encrypted Doc",
                author: "Admin");

            Assert.True(success);
            Assert.True(File.Exists(outPdf));

            // Opening without password or with correct password
            using var readDoc = PdfReader.Open(outPdf, "secretPassword123", PdfDocumentOpenMode.Import);
            Assert.Equal(2, readDoc.PageCount);
            Assert.Equal("Encrypted Doc", readDoc.Info.Title);
            Assert.Equal("Admin", readDoc.Info.Author);
        }

        [Theory]
        [InlineData("1-3", 5, 3)]
        [InlineData("1, 3, 5", 5, 3)]
        [InlineData("4-2", 5, 3)] // inverted range 4-2 should normalize to 2-4
        [InlineData("1-10", 4, 4)] // clamped to max pages
        [InlineData("", 5, 5)]     // empty string returns all pages
        public void ParsePageRange_ShouldCorrectlyParseRanges(string input, int totalPages, int expectedCount)
        {
            var indices = PdfToolkitService.ParsePageRange(input, totalPages);
            Assert.Equal(expectedCount, indices.Count);
        }

        [Fact]
        public void FormatFileSize_ShouldFormatBytesAccurately()
        {
            Assert.Equal("500 B", PdfToolkitService.FormatFileSize(500));
            Assert.Equal("1.5 KB", PdfToolkitService.FormatFileSize(1536));
            Assert.Equal("2.00 MB", PdfToolkitService.FormatFileSize(2 * 1024 * 1024));
        }
    }
}
