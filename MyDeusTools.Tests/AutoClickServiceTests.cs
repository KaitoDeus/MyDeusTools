using MyDeusTools.App.Services.Impl;
using Xunit;

namespace MyDeusTools.Tests
{
    public class AutoClickServiceTests
    {
        [Fact]
        public void Start_ShouldSetRunningStateAndInterval()
        {
            // Arrange
            var service = new AutoClickService();
            int testInterval = 500;

            // Act
            service.Start(testInterval);

            // Assert
            Assert.True(service.IsRunning);
            Assert.Equal(500, service.Interval);
        }

        [Fact]
        public void Start_ShouldClampMinimumIntervalTo10ms()
        {
            // Arrange
            var service = new AutoClickService();
            int lowInterval = 5;

            // Act
            service.Start(lowInterval);

            // Assert
            Assert.Equal(10, service.Interval);
        }

        [Fact]
        public void Stop_ShouldResetRunningState()
        {
            // Arrange
            var service = new AutoClickService();
            service.Start(100);

            // Act
            service.Stop();

            // Assert
            Assert.False(service.IsRunning);
        }
    }
}
