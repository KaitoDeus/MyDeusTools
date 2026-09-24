using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyDeusTools.App.Services;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class ColorPickerServiceTests : IDisposable
    {
        private readonly string _testFilePath;

        public ColorPickerServiceTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"mydeustools_test_colors_{Guid.NewGuid():N}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                try { File.Delete(_testFilePath); } catch { }
            }
        }

        [Fact]
        public void AddColor_ShouldFormatHexAndAddAtBeginning()
        {
            // Arrange
            var service = new ColorPickerService(_testFilePath);

            // Act
            var item1 = service.AddColor(255, 0, 0);     // #FF0000
            var item2 = service.AddColor(0, 255, 128);   // #00FF80

            // Assert
            Assert.Equal(2, service.History.Count);
            Assert.Equal("#00FF80", service.History[0].Hex);
            Assert.Equal("#FF0000", service.History[1].Hex);
        }

        [Fact]
        public void AddColor_ShouldCalculateRgbHslHsvCorrectly()
        {
            // Arrange
            var service = new ColorPickerService(_testFilePath);

            // Act: Pure Red (255, 0, 0)
            var red = service.AddColor(255, 0, 0);

            // Assert
            Assert.Equal("#FF0000", red.Hex);
            Assert.Equal("rgb(255, 0, 0)", red.Rgb);
            Assert.Equal("hsl(0, 100%, 50%)", red.Hsl);
            Assert.Equal("hsv(0, 100%, 100%)", red.Hsv);

            // Act 2: Pure White (255, 255, 255)
            var white = service.AddColor(255, 255, 255);
            Assert.Equal("#FFFFFF", white.Hex);
            Assert.Equal("hsl(0, 0%, 100%)", white.Hsl);
        }

        [Fact]
        public void AddColor_ShouldAvoidImmediateDuplicate_AndPreservePin()
        {
            // Arrange
            var service = new ColorPickerService(_testFilePath);
            var colorA = service.AddColor(10, 20, 30);
            var colorB = service.AddColor(40, 50, 60);

            // Pin colorA
            service.TogglePin(colorA);
            Assert.True(colorA.IsPinned);

            // Act: Re-add colorA
            var reAdded = service.AddColor(10, 20, 30);

            // Assert
            Assert.Equal(2, service.History.Count);
            Assert.Equal(reAdded, service.History[0]);
            Assert.True(reAdded.IsPinned);
        }

        [Fact]
        public void AddColor_ShouldRespectMaxCapacity_PreservingPinnedColors()
        {
            // Arrange: Max 3 items
            var service = new ColorPickerService(_testFilePath, maxItems: 3);
            var c1 = service.AddColor(1, 1, 1);
            service.TogglePin(c1);

            service.AddColor(2, 2, 2);
            service.AddColor(3, 3, 3);
            service.AddColor(4, 4, 4);

            // Assert: capacity 3, c1 pinned remains
            Assert.Equal(3, service.History.Count);
            Assert.Contains(service.History, x => x.Hex == "#010101" && x.IsPinned);
            Assert.Contains(service.History, x => x.Hex == "#040404");
            Assert.Contains(service.History, x => x.Hex == "#030303");
            Assert.DoesNotContain(service.History, x => x.Hex == "#020202");
        }

        [Fact]
        public void ClearHistory_ShouldKeepPinnedColors_WhenKeepPinnedIsTrue()
        {
            // Arrange
            var service = new ColorPickerService(_testFilePath);
            var c1 = service.AddColor(100, 100, 100);
            var c2 = service.AddColor(200, 200, 200);
            service.TogglePin(c2);

            // Act
            service.ClearHistory(keepPinned: true);

            // Assert
            Assert.Single(service.History);
            Assert.Equal("#C8C8C8", service.History[0].Hex);
            Assert.True(service.History[0].IsPinned);
        }

        [Fact]
        public async Task SaveAndLoad_ShouldPersistColorsToJsonFile()
        {
            // Arrange
            var service1 = new ColorPickerService(_testFilePath);
            service1.AddColor(15, 30, 45);
            var c2 = service1.AddColor(70, 80, 90);
            service1.TogglePin(c2);
            await service1.SaveHistoryAsync();

            // Act
            var service2 = new ColorPickerService(_testFilePath);

            // Assert
            Assert.Equal(2, service2.History.Count);
            Assert.Equal("#46505A", service2.History[0].Hex);
            Assert.True(service2.History[0].IsPinned);
            Assert.Equal("#0F1E2D", service2.History[1].Hex);
            Assert.False(service2.History[1].IsPinned);
        }

        [Fact]
        public void ColorPickerViewModel_SelectColor_ShouldUpdateSelectedColor()
        {
            // Arrange
            var service = new ColorPickerService(_testFilePath);
            service.AddColor(10, 20, 30);
            service.AddColor(40, 50, 60);

            var vm = new ColorPickerViewModel(service);

            // Act
            vm.SelectColor(service.History[1]);

            // Assert
            Assert.Equal(service.History[1], vm.SelectedColor);
            Assert.True(vm.HasSelectedColor);
        }
    }
}
