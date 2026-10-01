using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class BulkRenamerServiceTests : IDisposable
    {
        private readonly BulkRenamerService _service = new();
        private readonly string _tempDir;

        public BulkRenamerServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"mydeustools_renamer_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                try { Directory.Delete(_tempDir, true); } catch { }
            }
        }

        [Fact]
        public void ApplyRenameRule_SimpleReplace_ShouldReplaceText()
        {
            var config = new RenameConfig
            {
                FindText = "photo",
                ReplaceText = "image",
                MatchCase = false
            };

            string result = _service.ApplyRenameRule("photo_01.jpg", false, config, 0);
            Assert.Equal("image_01.jpg", result);
        }

        [Fact]
        public void ApplyRenameRule_RegexReplace_ShouldSupportPatternsAndGroups()
        {
            var config = new RenameConfig
            {
                FindText = @"img_(\d+)",
                ReplaceText = "picture_$1",
                UseRegex = true
            };

            string result = _service.ApplyRenameRule("img_999.png", false, config, 0);
            Assert.Equal("picture_999.png", result);
        }

        [Fact]
        public void ApplyRenameRule_InvalidRegex_ShouldNotThrowAndKeepOriginal()
        {
            var config = new RenameConfig
            {
                FindText = @"[unclosed_bracket",
                ReplaceText = "xyz",
                UseRegex = true
            };

            string result = _service.ApplyRenameRule("document.pdf", false, config, 0);
            Assert.Equal("document.pdf", result);
        }

        [Fact]
        public void ApplyRenameRule_PrefixAndSuffix_ShouldAddBoth()
        {
            var config = new RenameConfig
            {
                Prefix = "PRE_",
                Suffix = "_POST"
            };

            string result = _service.ApplyRenameRule("myfile.txt", false, config, 0);
            Assert.Equal("PRE_myfile_POST.txt", result);
        }

        [Fact]
        public void ApplyRenameRule_InsertAtPosition_ShouldInsertCorrectly()
        {
            var config = new RenameConfig
            {
                InsertText = "-TEST-",
                InsertPosition = 4 // after 3rd char
            };

            string result = _service.ApplyRenameRule("ABCDEF.txt", false, config, 0);
            Assert.Equal("ABC-TEST-DEF.txt", result);
        }

        [Fact]
        public void ApplyRenameRule_RemoveFirstAndLastN_ShouldTrimCharacters()
        {
            var config = new RenameConfig
            {
                RemoveFirstN = 3,
                RemoveLastN = 2
            };

            // "ABCDEFGHIJK" -> remove 3 first ("DEFGHIJK"), remove 2 last ("DEFGHI")
            string result = _service.ApplyRenameRule("ABCDEFGHIJK.txt", false, config, 0);
            Assert.Equal("DEFGHI.txt", result);
        }

        [Theory]
        [InlineData(CaseConversionMode.Upper, "my_file_name.txt", "MY_FILE_NAME.txt")]
        [InlineData(CaseConversionMode.Lower, "MY_FILE_NAME.TXT", "my_file_name.TXT")]
        [InlineData(CaseConversionMode.TitleCase, "hello world.txt", "Hello World.txt")]
        [InlineData(CaseConversionMode.SentenceCase, "hello world.txt", "Hello world.txt")]
        [InlineData(CaseConversionMode.CamelCase, "my_test_file.txt", "myTestFile.txt")]
        [InlineData(CaseConversionMode.PascalCase, "my_test_file.txt", "MyTestFile.txt")]
        [InlineData(CaseConversionMode.SnakeCase, "MyTestFile.txt", "my_test_file.txt")]
        [InlineData(CaseConversionMode.KebabCase, "MyTestFile.txt", "my-test-file.txt")]
        public void ApplyRenameRule_CaseConversions_ShouldFormatProperly(CaseConversionMode mode, string input, string expected)
        {
            var config = new RenameConfig { CaseMode = mode };
            string result = _service.ApplyRenameRule(input, false, config, 0);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ApplyRenameRule_NumberingSuffix_ShouldAppendPaddedNumber()
        {
            var config = new RenameConfig
            {
                EnableNumbering = true,
                NumberPlacement = NumberingPlacement.Suffix,
                StartNumber = 1,
                NumberStep = 5,
                ZeroPadding = 3,
                NumberSeparator = "_"
            };

            string result0 = _service.ApplyRenameRule("doc.pdf", false, config, 0);
            string result1 = _service.ApplyRenameRule("doc.pdf", false, config, 1);

            Assert.Equal("doc_001.pdf", result0);
            Assert.Equal("doc_006.pdf", result1);
        }

        [Fact]
        public void ApplyRenameRule_NumberingReplace_ShouldReplaceNameEntirely()
        {
            var config = new RenameConfig
            {
                EnableNumbering = true,
                NumberPlacement = NumberingPlacement.Replace,
                StartNumber = 10,
                ZeroPadding = 2
            };

            string result = _service.ApplyRenameRule("old_name.docx", false, config, 0);
            Assert.Equal("10.docx", result);
        }

        [Fact]
        public void ApplyRenameRule_IncludeExtension_ShouldModifyExtensionToo()
        {
            var config = new RenameConfig
            {
                IncludeExtension = true,
                FindText = ".jpeg",
                ReplaceText = ".jpg"
            };

            string result = _service.ApplyRenameRule("image.jpeg", false, config, 0);
            Assert.Equal("image.jpg", result);
        }

        [Fact]
        public void ApplyRenameRule_TargetTypeFilter_ShouldSkipMismatchedTypes()
        {
            var config = new RenameConfig
            {
                TargetType = RenameTargetType.FilesOnly,
                Prefix = "PREFIX_"
            };

            string fileResult = _service.ApplyRenameRule("doc.txt", false, config, 0);
            string folderResult = _service.ApplyRenameRule("MyFolder", true, config, 0);

            Assert.Equal("PREFIX_doc.txt", fileResult);
            Assert.Equal("MyFolder", folderResult); // Folder skipped
        }

        [Fact]
        public void UpdatePreviews_ConflictDetection_ShouldDetectDuplicatesInBatch()
        {
            var item1 = new RenameItemViewModel
            {
                DirectoryPath = _tempDir,
                OriginalName = "fileA.txt",
                IsDirectory = false
            };
            var item2 = new RenameItemViewModel
            {
                DirectoryPath = _tempDir,
                OriginalName = "fileB.txt",
                IsDirectory = false
            };

            var items = new List<RenameItemViewModel> { item1, item2 };
            var config = new RenameConfig
            {
                FindText = "fileB",
                ReplaceText = "fileA" // Will make item2 duplicate item1
            };

            _service.UpdatePreviews(items, config);

            Assert.Equal(RenameItemStatus.ConflictDuplicate, item1.Status);
            Assert.Equal(RenameItemStatus.ConflictDuplicate, item2.Status);
        }

        [Fact]
        public void UpdatePreviews_InvalidCharacters_ShouldBeDetected()
        {
            var item = new RenameItemViewModel
            {
                DirectoryPath = _tempDir,
                OriginalName = "file.txt",
                IsDirectory = false
            };

            var items = new List<RenameItemViewModel> { item };
            var config = new RenameConfig
            {
                FindText = "file",
                ReplaceText = "invalid*name?"
            };

            _service.UpdatePreviews(items, config);

            Assert.Equal(RenameItemStatus.InvalidCharacters, item.Status);
        }

        [Fact]
        public async Task ExecuteRenameAndUndo_ShouldSuccessfullyRenameAndRevert()
        {
            // Arrange
            string file1 = Path.Combine(_tempDir, "item1.txt");
            string file2 = Path.Combine(_tempDir, "item2.txt");
            await File.WriteAllTextAsync(file1, "content1");
            await File.WriteAllTextAsync(file2, "content2");

            var itemVm1 = RenameItemViewModel.FromPath(file1, 1);
            var itemVm2 = RenameItemViewModel.FromPath(file2, 2);
            var items = new List<RenameItemViewModel> { itemVm1, itemVm2 };

            var config = new RenameConfig
            {
                Prefix = "RENAMED_"
            };

            _service.UpdatePreviews(items, config);
            Assert.Equal(RenameItemStatus.Ready, itemVm1.Status);
            Assert.Equal(RenameItemStatus.Ready, itemVm2.Status);

            // Act 1: Execute Rename
            var result = await _service.ExecuteRenameAsync(items);

            // Assert 1: Files renamed
            Assert.Equal(2, result.SuccessCount);
            Assert.Equal(0, result.FailedCount);
            Assert.False(File.Exists(file1));
            Assert.False(File.Exists(file2));
            Assert.True(File.Exists(Path.Combine(_tempDir, "RENAMED_item1.txt")));
            Assert.True(File.Exists(Path.Combine(_tempDir, "RENAMED_item2.txt")));

            // Act 2: Undo Rename
            var undoResult = await _service.UndoRenameAsync(result.History);

            // Assert 2: Files restored
            Assert.Equal(2, undoResult.SuccessCount);
            Assert.True(File.Exists(file1));
            Assert.True(File.Exists(file2));
            Assert.False(File.Exists(Path.Combine(_tempDir, "RENAMED_item1.txt")));
            Assert.False(File.Exists(Path.Combine(_tempDir, "RENAMED_item2.txt")));
        }

        [Fact]
        public void ScanPaths_ShouldFindFilesAndFolders()
        {
            string subDir = Path.Combine(_tempDir, "Sub");
            Directory.CreateDirectory(subDir);
            string f1 = Path.Combine(_tempDir, "root.txt");
            string f2 = Path.Combine(subDir, "nested.txt");
            File.WriteAllText(f1, "1");
            File.WriteAllText(f2, "2");

            var scanned = _service.ScanPaths(new[] { _tempDir }, true, RenameTargetType.FilesAndFolders);

            Assert.Contains(f1, scanned);
            Assert.Contains(f2, scanned);
            Assert.Contains(subDir, scanned);
        }
    }
}
