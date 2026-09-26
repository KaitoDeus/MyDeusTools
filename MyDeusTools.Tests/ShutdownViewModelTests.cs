using System;
using Moq;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class ShutdownViewModelTests
    {
        [WpfFact]
        public void Schedule_ShouldCalculateTotalSecondsAndCallScheduleAction()
        {
            // Arrange
            var mockService = new Mock<ISystemService>();
            var vm = new ShutdownViewModel(mockService.Object)
            {
                Hours = 1,
                Minutes = 30,
                Seconds = 15,
                SelectedMode = ShutdownMode.Shutdown
            };

            // Act
            vm.ScheduleCommand.Execute(null);

            // Assert
            mockService.Verify(s => s.ScheduleAction(ShutdownMode.Shutdown, 5415), Times.Once);
            Assert.True(vm.IsScheduled);
            Assert.Equal("01:30:15", vm.CountdownText);
            Assert.Contains("Đã hẹn giờ", vm.StatusMessage);
            Assert.Contains("tắt máy", vm.StatusMessage);
        }

        [WpfFact]
        public void Schedule_WhenCalledSecondTime_ShouldRescheduleWithNewTimeAndCallScheduleActionAgain()
        {
            // Arrange
            var mockService = new Mock<ISystemService>();
            var vm = new ShutdownViewModel(mockService.Object)
            {
                Hours = 0,
                Minutes = 30,
                Seconds = 0,
                SelectedMode = ShutdownMode.Shutdown
            };

            // Act 1: Lần đầu tiên hẹn giờ 30 phút
            vm.ScheduleCommand.Execute(null);
            mockService.Verify(s => s.ScheduleAction(ShutdownMode.Shutdown, 1800), Times.Once);
            Assert.True(vm.IsScheduled);
            Assert.Equal("00:30:00", vm.CountdownText);
            Assert.Contains("Đã hẹn giờ", vm.StatusMessage);

            // Act 2: Người dùng sửa giờ thành 45 phút và bấm lần 2
            vm.Minutes = 45;
            vm.ScheduleCommand.Execute(null);

            // Assert: Hệ thống phải nhận lệnh mới 2700s và cập nhật giao diện
            mockService.Verify(s => s.ScheduleAction(ShutdownMode.Shutdown, 2700), Times.Once);
            Assert.True(vm.IsScheduled);
            Assert.Equal("00:45:00", vm.CountdownText);
            Assert.Contains("Đã cập nhật hẹn giờ", vm.StatusMessage);
            Assert.Contains("45m", vm.StatusMessage);
        }

        [WpfFact]
        public void Schedule_WhenTotalSecondsIsZeroOrNegative_ShouldNotCallScheduleAction()
        {
            // Arrange
            var mockService = new Mock<ISystemService>();
            var vm = new ShutdownViewModel(mockService.Object)
            {
                Hours = 0,
                Minutes = 0,
                Seconds = 0
            };

            // Act
            vm.ScheduleCommand.Execute(null);

            // Assert
            mockService.Verify(s => s.ScheduleAction(It.IsAny<ShutdownMode>(), It.IsAny<int>()), Times.Never);
            Assert.False(vm.IsScheduled);
            Assert.Equal("Vui lòng nhập thời gian lớn hơn 0!", vm.StatusMessage);
        }

        [WpfFact]
        public void Cancel_ShouldStopCountdownAndCallCancelShutdown()
        {
            // Arrange
            var mockService = new Mock<ISystemService>();
            var vm = new ShutdownViewModel(mockService.Object)
            {
                Hours = 1,
                Minutes = 0,
                Seconds = 0
            };
            vm.ScheduleCommand.Execute(null);
            Assert.True(vm.IsScheduled);

            // Act
            vm.CancelCommand.Execute(null);

            // Assert
            mockService.Verify(s => s.CancelShutdown(), Times.Once);
            Assert.False(vm.IsScheduled);
            Assert.Equal(string.Empty, vm.CountdownText);
            Assert.Equal("Đã hủy lệnh hẹn giờ", vm.StatusMessage);
        }

        [WpfFact]
        public void Schedule_WithRestartMode_ShouldPassModeCorrectly()
        {
            // Arrange
            var mockService = new Mock<ISystemService>();
            var vm = new ShutdownViewModel(mockService.Object)
            {
                Hours = 0,
                Minutes = 10,
                Seconds = 0,
                SelectedMode = ShutdownMode.Restart
            };

            // Act
            vm.ScheduleCommand.Execute(null);

            // Assert
            mockService.Verify(s => s.ScheduleAction(ShutdownMode.Restart, 600), Times.Once);
            Assert.True(vm.IsScheduled);
            Assert.Contains("khởi động lại", vm.StatusMessage);
        }

        [WpfFact]
        public void Schedule_WithHibernateMode_ShouldPassModeCorrectly()
        {
            // Arrange
            var mockService = new Mock<ISystemService>();
            var vm = new ShutdownViewModel(mockService.Object)
            {
                Hours = 0,
                Minutes = 15,
                Seconds = 0,
                SelectedMode = ShutdownMode.Hibernate
            };

            // Act
            vm.ScheduleCommand.Execute(null);

            // Assert
            mockService.Verify(s => s.ScheduleAction(ShutdownMode.Hibernate, 900), Times.Once);
            Assert.True(vm.IsScheduled);
            Assert.Contains("ngủ đông", vm.StatusMessage);
        }
    }
}
