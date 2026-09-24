using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyDeusTools.App.Services;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class ClipboardServiceTests : IDisposable
    {
        private readonly string _testFilePath;

        public ClipboardServiceTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"mydeustools_test_clipboard_{Guid.NewGuid():N}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                try
                {
                    File.Delete(_testFilePath);
                }
                catch { }
            }
        }

        [Fact]
        public void AddItem_ShouldInsertItemAtBeginning()
        {
            // Arrange
            var service = new ClipboardService(_testFilePath);

            // Act
            service.AddItem("First item");
            service.AddItem("Second item");

            // Assert
            Assert.Equal(2, service.Items.Count);
            Assert.Equal("Second item", service.Items[0].Content);
            Assert.Equal("First item", service.Items[1].Content);
        }

        [Fact]
        public void AddItem_ShouldAvoidImmediateConsecutiveDuplicates()
        {
            // Arrange
            var service = new ClipboardService(_testFilePath);

            // Act
            service.AddItem("Duplicate test");
            service.AddItem("Duplicate test");

            // Assert
            Assert.Single(service.Items);
            Assert.Equal("Duplicate test", service.Items[0].Content);
        }

        [Fact]
        public void AddItem_ShouldMoveExistingDuplicateToTop_AndPreservePinState()
        {
            // Arrange
            var service = new ClipboardService(_testFilePath);
            service.AddItem("Old item");
            service.AddItem("Middle item");
            service.TogglePin(service.Items[1]); // Pin "Old item"

            // Act: re-copy "Old item"
            service.AddItem("Old item");

            // Assert
            Assert.Equal(2, service.Items.Count);
            Assert.Equal("Old item", service.Items[0].Content);
            Assert.True(service.Items[0].IsPinned);
        }

        [Fact]
        public void AddItem_ShouldRespectMaxItemsLimit_PreservingPinnedItems()
        {
            // Arrange: Max 3 items
            var service = new ClipboardService(_testFilePath, maxItems: 3);
            service.AddItem("Item 1");
            service.TogglePin(service.Items[0]); // Pin Item 1
            service.AddItem("Item 2");
            service.AddItem("Item 3");
            service.AddItem("Item 4");

            // Assert
            Assert.Equal(3, service.Items.Count);
            Assert.Contains(service.Items, x => x.Content == "Item 1" && x.IsPinned);
            Assert.Contains(service.Items, x => x.Content == "Item 4");
            Assert.Contains(service.Items, x => x.Content == "Item 3");
            Assert.DoesNotContain(service.Items, x => x.Content == "Item 2");
        }

        [Fact]
        public void TogglePin_ShouldToggleIsPinnedState()
        {
            // Arrange
            var service = new ClipboardService(_testFilePath);
            service.AddItem("Pin test");
            var item = service.Items[0];

            // Act 1
            service.TogglePin(item);
            Assert.True(item.IsPinned);

            // Act 2
            service.TogglePin(item);
            Assert.False(item.IsPinned);
        }

        [Fact]
        public void ClearHistory_ShouldKeepPinnedItems_WhenKeepPinnedIsTrue()
        {
            // Arrange
            var service = new ClipboardService(_testFilePath);
            service.AddItem("Normal 1");
            service.AddItem("Pinned Item");
            service.TogglePin(service.Items[0]); // Pin "Pinned Item"
            service.AddItem("Normal 2");

            // Act
            service.ClearHistory(keepPinned: true);

            // Assert
            Assert.Single(service.Items);
            Assert.Equal("Pinned Item", service.Items[0].Content);
            Assert.True(service.Items[0].IsPinned);
        }

        [Fact]
        public void ClearHistory_ShouldClearAll_WhenKeepPinnedIsFalse()
        {
            // Arrange
            var service = new ClipboardService(_testFilePath);
            service.AddItem("Pinned Item");
            service.TogglePin(service.Items[0]);
            service.AddItem("Normal Item");

            // Act
            service.ClearHistory(keepPinned: false);

            // Assert
            Assert.Empty(service.Items);
        }

        [Fact]
        public async Task SaveAndLoad_ShouldPersistItemsToJsonFile()
        {
            // Arrange
            var service1 = new ClipboardService(_testFilePath);
            service1.AddItem("Saved text 1");
            service1.AddItem("Saved text 2");
            service1.TogglePin(service1.Items[0]);
            await service1.SaveHistoryAsync();

            // Act
            var service2 = new ClipboardService(_testFilePath);

            // Assert
            Assert.Equal(2, service2.Items.Count);
            Assert.Equal("Saved text 2", service2.Items[0].Content);
            Assert.True(service2.Items[0].IsPinned);
            Assert.Equal("Saved text 1", service2.Items[1].Content);
            Assert.False(service2.Items[1].IsPinned);
        }

        [Fact]
        public void ClipboardViewModel_ApplyFilter_ShouldFilterItemsBySearchText()
        {
            // Arrange
            var service = new ClipboardService(_testFilePath);
            service.AddItem("Apple Banana");
            service.AddItem("Orange Peach");
            service.AddItem("Banana Grape");

            var viewModel = new ClipboardViewModel(service);

            // Act 1: Search "Banana"
            viewModel.SearchText = "Banana";

            // Assert 1
            Assert.Equal(2, viewModel.FilteredItems.Count);
            Assert.All(viewModel.FilteredItems, x => Assert.Contains("Banana", x.Content));

            // Act 2: Clear search
            viewModel.ClearSearchCommand.Execute(null);

            // Assert 2
            Assert.Equal(3, viewModel.FilteredItems.Count);
            Assert.Equal(string.Empty, viewModel.SearchText);
        }
    }
}
