using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class PdfToolkitViewModelTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly Mock<IPdfToolkitService> _mockPdfService;
        private readonly PdfToolkitViewModel _vm;

        public PdfToolkitViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"pdftk_vmtests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);

            _mockPdfService = new Mock<IPdfToolkitService>();
            _vm = new PdfToolkitViewModel(_mockPdfService.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                try { Directory.Delete(_tempDir, true); } catch { }
            }
        }

        private string CreateFile(string name)
        {
            string path = Path.Combine(_tempDir, name);
            File.WriteAllText(path, "dummy content");
            return path;
        }

        [Fact]
        public void AddMergeFilePaths_ShouldAddOnlyValidPdfsAndAvoidDuplicates()
        {
            string pdf1 = CreateFile("doc1.pdf");
            string pdf2 = CreateFile("doc2.pdf");
            string txt = CreateFile("notes.txt");

            _mockPdfService.Setup(s => s.GetPdfInfo(pdf1))
                .Returns(new PdfFileInfo(pdf1, "doc1.pdf", 100, "100 B", 2, "", "", "", ""));
            _mockPdfService.Setup(s => s.GetPdfInfo(pdf2))
                .Returns(new PdfFileInfo(pdf2, "doc2.pdf", 200, "200 B", 3, "", "", "", ""));

            _vm.AddMergeFilePaths(new[] { pdf1, pdf2, txt, pdf1 });

            Assert.Equal(2, _vm.MergeFiles.Count);
            Assert.Equal("doc1.pdf", _vm.MergeFiles[0].FileName);
            Assert.Equal(1, _vm.MergeFiles[0].OrderNumber);
            Assert.Equal("doc2.pdf", _vm.MergeFiles[1].FileName);
            Assert.Equal(2, _vm.MergeFiles[1].OrderNumber);
        }

        [Fact]
        public void MoveMergeItems_ShouldReorderCorrectly()
        {
            string pdf1 = CreateFile("a.pdf");
            string pdf2 = CreateFile("b.pdf");

            _vm.AddMergeFilePaths(new[] { pdf1, pdf2 });

            var itemA = _vm.MergeFiles[0];
            var itemB = _vm.MergeFiles[1];

            // Move itemB Up
            _vm.MoveMergeItemUp(itemB);
            Assert.Equal(itemB, _vm.MergeFiles[0]);
            Assert.Equal(1, _vm.MergeFiles[0].OrderNumber);
            Assert.Equal(itemA, _vm.MergeFiles[1]);
            Assert.Equal(2, _vm.MergeFiles[1].OrderNumber);

            // Move itemB Down
            _vm.MoveMergeItemDown(itemB);
            Assert.Equal(itemA, _vm.MergeFiles[0]);
            Assert.Equal(itemB, _vm.MergeFiles[1]);
        }

        [Fact]
        public void RemoveMergeItem_ShouldRemoveAndReindex()
        {
            string pdf1 = CreateFile("1.pdf");
            string pdf2 = CreateFile("2.pdf");
            string pdf3 = CreateFile("3.pdf");

            _vm.AddMergeFilePaths(new[] { pdf1, pdf2, pdf3 });

            _vm.RemoveMergeItem(_vm.MergeFiles[1]);

            Assert.Equal(2, _vm.MergeFiles.Count);
            Assert.Equal("1.pdf", _vm.MergeFiles[0].FileName);
            Assert.Equal(1, _vm.MergeFiles[0].OrderNumber);
            Assert.Equal("3.pdf", _vm.MergeFiles[1].FileName);
            Assert.Equal(2, _vm.MergeFiles[1].OrderNumber);
        }

        [Fact]
        public async Task StartMergeAsync_WhenUnderTwoFiles_ShouldSetErrorMessage()
        {
            string pdf1 = CreateFile("single.pdf");
            _vm.AddMergeFilePaths(new[] { pdf1 });

            await _vm.StartMergeAsync();

            Assert.Contains("ít nhất 2 tệp", _vm.StatusMessage);
            _mockPdfService.Verify(s => s.MergePdfsAsync(It.IsAny<System.Collections.Generic.IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task StartMergeAsync_WhenValid_ShouldCallService()
        {
            string pdf1 = CreateFile("m1.pdf");
            string pdf2 = CreateFile("m2.pdf");
            _vm.AddMergeFilePaths(new[] { pdf1, pdf2 });

            _mockPdfService.Setup(s => s.MergePdfsAsync(It.IsAny<System.Collections.Generic.IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _vm.StartMergeAsync();

            Assert.True(_vm.HasMergedResult);
            Assert.Contains("thành công", _vm.StatusMessage);
        }

        [Fact]
        public void LoadSplitSource_ShouldPopulateSourceInfoAndPageRange()
        {
            string srcPdf = CreateFile("split_source.pdf");
            _mockPdfService.Setup(s => s.GetPdfInfo(srcPdf))
                .Returns(new PdfFileInfo(srcPdf, "split_source.pdf", 1000, "1 KB", 10, "Book", "Author", "", ""));

            _vm.LoadSplitSource(srcPdf);

            Assert.Equal(srcPdf, _vm.SplitSourceFilePath);
            Assert.NotNull(_vm.SplitSourceInfo);
            Assert.Equal(10, _vm.SplitSourceInfo.PageCount);
            Assert.Equal("1-3", _vm.SplitPageRange);
        }

        [Fact]
        public async Task StartSplitAsync_WhenServiceSucceeds_ShouldSetSuccessStatus()
        {
            string srcPdf = CreateFile("split_ok.pdf");
            _mockPdfService.Setup(s => s.GetPdfInfo(srcPdf))
                .Returns(new PdfFileInfo(srcPdf, "split_ok.pdf", 1000, "1 KB", 5, "", "", "", ""));
            _mockPdfService.Setup(s => s.SplitPdfAsync(srcPdf, It.IsAny<string>(), It.IsAny<PdfSplitMode>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            _vm.LoadSplitSource(srcPdf);
            await _vm.StartSplitAsync();

            Assert.True(_vm.HasSplitResult);
            Assert.Contains("Tách PDF hoàn tất", _vm.StatusMessage);
        }

        [Fact]
        public void AddImageFilePaths_ShouldFilterValidImagesAndReorder()
        {
            string img1 = CreateFile("pic1.png");
            string img2 = CreateFile("pic2.jpg");
            string doc = CreateFile("doc.pdf");

            _vm.AddImageFilePaths(new[] { img1, img2, doc });

            Assert.Equal(2, _vm.ImageFiles.Count);
            Assert.Equal("pic1.png", _vm.ImageFiles[0].FileName);
            Assert.Equal(1, _vm.ImageFiles[0].OrderNumber);
            Assert.Equal("pic2.jpg", _vm.ImageFiles[1].FileName);
            Assert.Equal(2, _vm.ImageFiles[1].OrderNumber);
        }
    }
}
