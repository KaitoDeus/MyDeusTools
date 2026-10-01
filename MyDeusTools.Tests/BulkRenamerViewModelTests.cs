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
    public class BulkRenamerViewModelTests : IDisposable
    {
        private readonly BulkRenamerService _service = new();
        private readonly string _tempDir;

        public BulkRenamerViewModelTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"mydeustools_vm_test_{Guid.NewGuid():N}");
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
        public void InitialState_ShouldBeEmptyWithDefaultValues()
        {
            var vm = new BulkRenamerViewModel(_service);

            Assert.Empty(vm.Items);
            Assert.Equal(0, vm.TotalCount);
            Assert.Equal(0, vm.ReadyCount);
            Assert.False(vm.HasItems);
            Assert.False(vm.HasReadyItems);
            Assert.False(vm.CanUndo);
            Assert.Equal(RenameTargetType.FilesAndFolders, vm.TargetType);
            Assert.Equal(CaseConversionMode.None, vm.SelectedCaseMode);
        }

        [Fact]
        public void AddPaths_ShouldPopulateItemsAndTriggerPreview()
        {
            string f1 = Path.Combine(_tempDir, "file1.txt");
            string f2 = Path.Combine(_tempDir, "file2.txt");
            File.WriteAllText(f1, "1");
            File.WriteAllText(f2, "2");

            var vm = new BulkRenamerViewModel(_service);
            vm.Prefix = "PRE_";
            vm.AddPaths(new[] { f1, f2 });

            Assert.Equal(2, vm.TotalCount);
            Assert.Equal(2, vm.ReadyCount);
            Assert.True(vm.HasItems);
            Assert.True(vm.HasReadyItems);
            Assert.Equal("PRE_file1.txt", vm.Items[0].NewName);
            Assert.Equal("PRE_file2.txt", vm.Items[1].NewName);
        }

        [Fact]
        public void RulePropertyChange_ShouldUpdatePreviewInRealTime()
        {
            string f1 = Path.Combine(_tempDir, "sample.jpg");
            File.WriteAllText(f1, "test");

            var vm = new BulkRenamerViewModel(_service);
            vm.AddPaths(new[] { f1 });

            // Initially unchanged
            Assert.Equal(RenameItemStatus.Unchanged, vm.Items[0].Status);

            // Change Find/Replace
            vm.FindText = "sample";
            vm.ReplaceText = "vacation";

            Assert.Equal("vacation.jpg", vm.Items[0].NewName);
            Assert.Equal(RenameItemStatus.Ready, vm.Items[0].Status);
            Assert.Equal(1, vm.ReadyCount);

            // Change Case Mode
            vm.SelectedCaseMode = CaseConversionMode.Upper;
            Assert.Equal("VACATION.jpg", vm.Items[0].NewName);

            // Enable Numbering
            vm.EnableNumbering = true;
            vm.NumberPlacement = NumberingPlacement.Prefix;
            vm.ZeroPadding = 2;
            vm.StartNumber = 5;

            Assert.Equal("05_VACATION.jpg", vm.Items[0].NewName);
        }

        [Fact]
        public void ClearList_ShouldResetStats()
        {
            string f1 = Path.Combine(_tempDir, "sample.txt");
            File.WriteAllText(f1, "test");

            var vm = new BulkRenamerViewModel(_service);
            vm.AddPaths(new[] { f1 });
            Assert.Equal(1, vm.TotalCount);

            vm.ClearList();

            Assert.Equal(0, vm.TotalCount);
            Assert.Empty(vm.Items);
            Assert.False(vm.HasItems);
        }

        [Fact]
        public void RemoveSelected_ShouldRemoveOnlyMarkedItems()
        {
            string f1 = Path.Combine(_tempDir, "item1.txt");
            string f2 = Path.Combine(_tempDir, "item2.txt");
            string f3 = Path.Combine(_tempDir, "item3.txt");
            File.WriteAllText(f1, "1");
            File.WriteAllText(f2, "2");
            File.WriteAllText(f3, "3");

            var vm = new BulkRenamerViewModel(_service);
            vm.AddPaths(new[] { f1, f2, f3 });

            vm.Items[1].IsSelected = true; // Select item2

            vm.RemoveSelected();

            Assert.Equal(2, vm.TotalCount);
            Assert.DoesNotContain(vm.Items, i => i.OriginalName == "item2.txt");
        }

        [Fact]
        public void SelectAllAndDeselectAll_ShouldToggleSelection()
        {
            string f1 = Path.Combine(_tempDir, "a.txt");
            string f2 = Path.Combine(_tempDir, "b.txt");
            File.WriteAllText(f1, "1");
            File.WriteAllText(f2, "2");

            var vm = new BulkRenamerViewModel(_service);
            vm.AddPaths(new[] { f1, f2 });

            vm.SelectAll();
            Assert.All(vm.Items, i => Assert.True(i.IsSelected));

            vm.DeselectAll();
            Assert.All(vm.Items, i => Assert.False(i.IsSelected));
        }

        [Fact]
        public async Task ApplyRenameAndUndo_FlowShouldSucceed()
        {
            string f1 = Path.Combine(_tempDir, "photo.png");
            await File.WriteAllTextAsync(f1, "image data");

            var vm = new BulkRenamerViewModel(_service);
            vm.AddPaths(new[] { f1 });
            vm.Prefix = "NEW_";

            Assert.True(vm.HasReadyItems);
            Assert.False(vm.CanUndo);

            // Apply
            await vm.ApplyRenameAsync();

            Assert.False(File.Exists(f1));
            Assert.True(File.Exists(Path.Combine(_tempDir, "NEW_photo.png")));
            Assert.True(vm.CanUndo);
            Assert.Single(vm.LastHistory);

            // Undo
            await vm.UndoAsync();

            Assert.True(File.Exists(f1));
            Assert.False(File.Exists(Path.Combine(_tempDir, "NEW_photo.png")));
            Assert.False(vm.CanUndo);
        }
    }
}
