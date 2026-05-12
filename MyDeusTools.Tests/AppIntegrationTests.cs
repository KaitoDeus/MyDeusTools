using Microsoft.Extensions.DependencyInjection;
using MyDeusTools.App;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using MyDeusTools.App.Views.Pages;
using Wpf.Ui;
using Xunit;


namespace MyDeusTools.Tests
{
    public class AppIntegrationTests
    {
        [WpfTheory]
        [InlineData(typeof(INavigationService))]
        [InlineData(typeof(IPageService))]
        [InlineData(typeof(IAutoClickService))]
        [InlineData(typeof(ISystemService))]
        [InlineData(typeof(IStickyNoteService))]
        [InlineData(typeof(MainWindowViewModel))]
        [InlineData(typeof(AutoClickViewModel))]
        [InlineData(typeof(ShutdownViewModel))]
        [InlineData(typeof(StickyNoteViewModel))]
        [InlineData(typeof(AutoClickPage))]
        [InlineData(typeof(ShutdownPage))]
        [InlineData(typeof(StickyNotePage))]
        public void ServiceProvider_ShouldResolveDependencies(Type serviceType)
        {
            // Arrange
            var serviceProvider = MyDeusTools.App.App.ConfigureServices();

            // Act
            var service = serviceProvider.GetService(serviceType);

            // Assert
            Assert.NotNull(service);
        }

        // Ghi chú: MainWindow yêu cầu tài nguyên hệ thống (ico, xaml) nên việc khởi tạo trong Unit Test
        // thường gặp lỗi đường dẫn. Chúng ta đã kiểm tra DI của các thành phần con ở trên là đủ.
    }
}
