using MyDeusTools.App.Services.Impl;
using Xunit;

namespace MyDeusTools.Tests
{
    public class AutoStartServiceTests
    {
        [Fact]
        public void IsEnabled_ShouldNotThrowException()
        {
            // Arrange
            var service = new AutoStartService("MyDeusTools_Test");

            // Act & Assert
            var record = Record.Exception(() => service.IsEnabled());
            Assert.Null(record);
        }

        [Fact]
        public void SetEnabled_ShouldNotThrowException()
        {
            // Arrange
            var service = new AutoStartService("MyDeusTools_Test");

            // Act & Assert
            var record = Record.Exception(() => service.SetEnabled(false));
            Assert.Null(record);
        }
    }
}
