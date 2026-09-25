using System;
using System.Collections.Generic;
using MyDeusTools.App.Services;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class WindowPinnerServiceTests
    {
        [Fact]
        public void WindowInfoModel_DefaultValues_AreCorrect()
        {
            var model = new WindowInfoModel();

            Assert.Equal(IntPtr.Zero, model.Handle);
            Assert.Equal(string.Empty, model.Title);
            Assert.Equal(string.Empty, model.ProcessName);
            Assert.Equal(0, model.ProcessId);
            Assert.False(model.IsTopMost);
            Assert.Equal((byte)255, model.Opacity);
            Assert.Equal(100, model.OpacityPercent);
            Assert.Equal(string.Empty, model.DisplayText);
        }

        [Theory]
        [InlineData((byte)255, 100)]
        [InlineData((byte)204, 80)]
        [InlineData((byte)153, 60)]
        [InlineData((byte)128, 50)]
        [InlineData((byte)0, 0)]
        public void WindowInfoModel_OpacityPercent_CalculatesCorrectly(byte opacity, int expectedPercent)
        {
            var model = new WindowInfoModel { Opacity = opacity };

            Assert.Equal(expectedPercent, model.OpacityPercent);
        }

        [Fact]
        public void WindowInfoModel_DisplayText_PrefersTitleOverProcessName()
        {
            var modelWithTitle = new WindowInfoModel
            {
                Title = "Visual Studio Code",
                ProcessName = "Code"
            };
            Assert.Equal("Visual Studio Code", modelWithTitle.DisplayText);

            var modelWithoutTitle = new WindowInfoModel
            {
                Title = "",
                ProcessName = "notepad"
            };
            Assert.Equal("notepad", modelWithoutTitle.DisplayText);
        }

        [Fact]
        public void WindowInfoModel_OnOpacityChanged_CallbackTriggered()
        {
            var model = new WindowInfoModel();
            WindowInfoModel? callbackParam = null;
            model.OnOpacityChanged = m => callbackParam = m;

            model.Opacity = 180;

            Assert.Same(model, callbackParam);
            Assert.Equal((byte)180, model.Opacity);
        }

        [Fact]
        public void WindowInfoModel_PropertyChanged_NotifiesOpacityAndPercent()
        {
            var model = new WindowInfoModel();
            var notifiedProperties = new List<string>();
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                {
                    notifiedProperties.Add(e.PropertyName);
                }
            };

            model.Opacity = 150;

            Assert.Contains(nameof(WindowInfoModel.Opacity), notifiedProperties);
            Assert.Contains(nameof(WindowInfoModel.OpacityPercent), notifiedProperties);
        }

        [Fact]
        public void WindowPinnerService_InvalidHandle_ReturnsExpectedDefaults()
        {
            var service = new WindowPinnerService();

            Assert.False(service.SetTopMost(IntPtr.Zero, true));
            Assert.False(service.ToggleTopMost(IntPtr.Zero));
            Assert.False(service.SetOpacity(IntPtr.Zero, 128));
            Assert.False(service.BringToFront(IntPtr.Zero));
            Assert.False(service.IsWindowTopMost(IntPtr.Zero));
            Assert.Equal((byte)255, service.GetWindowOpacity(IntPtr.Zero));
        }

        [Fact]
        public void WindowPinnerService_GetOpenWindows_ReturnsListWithoutExceptions()
        {
            var service = new WindowPinnerService();

            var windows = service.GetOpenWindows();

            Assert.NotNull(windows);
        }

        [Fact]
        public void WindowPinnerViewModel_Initialization_SetsUpCollections()
        {
            var mockService = new MockWindowPinnerService(new List<WindowInfoModel>
            {
                new WindowInfoModel { Handle = new IntPtr(100), Title = "Editor - Doc1", ProcessName = "notepad", IsTopMost = true },
                new WindowInfoModel { Handle = new IntPtr(200), Title = "Browser", ProcessName = "chrome", IsTopMost = false },
                new WindowInfoModel { Handle = new IntPtr(300), Title = "Calculator", ProcessName = "calc", IsTopMost = false }
            });

            var vm = new WindowPinnerViewModel(mockService);

            Assert.Equal(3, vm.TotalCount);
            Assert.Equal(1, vm.PinnedCount);
            Assert.True(vm.HasWindows);
            Assert.Equal(3, vm.FilteredWindows.Count);
        }

        [Fact]
        public void WindowPinnerViewModel_SearchFilter_FiltersCorrectly()
        {
            var mockService = new MockWindowPinnerService(new List<WindowInfoModel>
            {
                new WindowInfoModel { Handle = new IntPtr(100), Title = "Editor - Doc1", ProcessName = "notepad" },
                new WindowInfoModel { Handle = new IntPtr(200), Title = "Chrome Browser", ProcessName = "chrome" },
                new WindowInfoModel { Handle = new IntPtr(300), Title = "Calculator", ProcessName = "calc" }
            });

            var vm = new WindowPinnerViewModel(mockService);

            vm.SearchText = "chrome";
            Assert.Single(vm.FilteredWindows);
            Assert.Equal("Chrome Browser", vm.FilteredWindows[0].Title);

            vm.ClearSearchCommand.Execute(null);
            Assert.Equal(string.Empty, vm.SearchText);
            Assert.Equal(3, vm.FilteredWindows.Count);
        }

        [Fact]
        public void WindowPinnerViewModel_TogglePin_UpdatesStateAndCounts()
        {
            var win = new WindowInfoModel { Handle = new IntPtr(100), Title = "App", ProcessName = "test", IsTopMost = false };
            var mockService = new MockWindowPinnerService(new List<WindowInfoModel> { win });

            var vm = new WindowPinnerViewModel(mockService);
            Assert.Equal(0, vm.PinnedCount);

            vm.TogglePinCommand.Execute(win);

            Assert.True(win.IsTopMost);
            Assert.Equal(1, vm.PinnedCount);
        }

        [Fact]
        public void WindowPinnerViewModel_OpacityPresets_ModifyOpacity()
        {
            var win = new WindowInfoModel { Handle = new IntPtr(100), Title = "App", ProcessName = "test", Opacity = 255 };
            var mockService = new MockWindowPinnerService(new List<WindowInfoModel> { win });
            var vm = new WindowPinnerViewModel(mockService);

            vm.SetOpacity80Command.Execute(win);
            Assert.Equal((byte)204, win.Opacity);

            vm.SetOpacity60Command.Execute(win);
            Assert.Equal((byte)153, win.Opacity);

            vm.SetOpacity40Command.Execute(win);
            Assert.Equal((byte)102, win.Opacity);

            vm.ResetOpacityCommand.Execute(win);
            Assert.Equal((byte)255, win.Opacity);

            vm.SetOpacity100Command.Execute(win);
            Assert.Equal((byte)255, win.Opacity);
        }

        private class MockWindowPinnerService : IWindowPinnerService
        {
            private readonly List<WindowInfoModel> _windows;

            public MockWindowPinnerService(List<WindowInfoModel> windows)
            {
                _windows = windows;
            }

            public List<WindowInfoModel> GetOpenWindows() => new List<WindowInfoModel>(_windows);

            public bool SetTopMost(IntPtr hwnd, bool topMost)
            {
                var match = _windows.Find(w => w.Handle == hwnd);
                if (match != null)
                {
                    match.IsTopMost = topMost;
                    return true;
                }
                return false;
            }

            public bool ToggleTopMost(IntPtr hwnd)
            {
                var match = _windows.Find(w => w.Handle == hwnd);
                if (match != null)
                {
                    match.IsTopMost = !match.IsTopMost;
                    return true;
                }
                return false;
            }

            public bool SetOpacity(IntPtr hwnd, byte opacity)
            {
                var match = _windows.Find(w => w.Handle == hwnd);
                if (match != null)
                {
                    match.Opacity = opacity;
                    return true;
                }
                return false;
            }

            public bool BringToFront(IntPtr hwnd) => true;

            public IntPtr GetForegroundWindowHandle() => _windows.Count > 0 ? _windows[0].Handle : IntPtr.Zero;

            public bool IsWindowTopMost(IntPtr hwnd)
            {
                var match = _windows.Find(w => w.Handle == hwnd);
                return match?.IsTopMost ?? false;
            }

            public byte GetWindowOpacity(IntPtr hwnd)
            {
                var match = _windows.Find(w => w.Handle == hwnd);
                return match?.Opacity ?? (byte)255;
            }
        }
    }
}
