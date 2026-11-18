using Moq;
using MyDeusTools.Agent.Services;

namespace MyDeusTools.Tests.Services;

public class OsAutoClickEngineTests
{
    [Fact]
    public void Configure_ClampsIntervalToMinimum10ms()
    {
        var mockMouse = new Mock<IMouseInterop>();
        using var engine = new OsAutoClickEngine(mockMouse.Object);

        engine.Configure(intervalMs: 1, button: OsMouseButton.Right, clickType: OsClickType.Double, repeatCount: 5);

        Assert.Equal(OsAutoClickEngine.MinIntervalMs, engine.IntervalMs);
        Assert.Equal(OsMouseButton.Right, engine.Button);
        Assert.Equal(OsClickType.Double, engine.ClickType);
        Assert.Equal(5, engine.RepeatCount);
    }

    [Fact]
    public void Configure_NegativeRepeatCountClampsToZero()
    {
        var mockMouse = new Mock<IMouseInterop>();
        using var engine = new OsAutoClickEngine(mockMouse.Object);

        engine.Configure(intervalMs: 100, repeatCount: -10);

        Assert.Equal(0, engine.RepeatCount);
    }

    [Fact]
    public void CaptureCursorPoint_AddsPointAndUpdatesStatus()
    {
        var mockMouse = new Mock<IMouseInterop>();
        mockMouse.Setup(m => m.GetCursorPosition()).Returns(new CursorPoint(320, 480));
        using var engine = new OsAutoClickEngine(mockMouse.Object);

        var captured = engine.CaptureCursorPoint();
        var status = engine.GetStatus();

        Assert.Equal(new CursorPoint(320, 480), captured);
        Assert.Single(status.Points);
        Assert.Equal(320, status.CursorX);
        Assert.Equal(480, status.CursorY);
    }

    [Fact]
    public void ClearRecordedPoints_RemovesAllCapturedPoints()
    {
        var mockMouse = new Mock<IMouseInterop>();
        mockMouse.Setup(m => m.GetCursorPosition()).Returns(new CursorPoint(100, 200));
        using var engine = new OsAutoClickEngine(mockMouse.Object);

        engine.CaptureCursorPoint();
        engine.CaptureCursorPoint();
        engine.ClearRecordedPoints();

        Assert.Empty(engine.GetStatus().Points);
    }

    [Fact]
    public void ExecuteSingleTick_DoubleClickInvokesClickTwice()
    {
        var mockMouse = new Mock<IMouseInterop>();
        using var engine = new OsAutoClickEngine(mockMouse.Object);

        engine.Configure(intervalMs: 50, button: OsMouseButton.Middle, clickType: OsClickType.Double);
        engine.SetRunningForTest(true);

        bool continued = engine.ExecuteSingleTick();

        Assert.True(continued);
        Assert.Equal(1, engine.ClicksDone);
        mockMouse.Verify(m => m.Click(OsMouseButton.Middle), Times.Exactly(2));
    }

    [Fact]
    public void ExecuteSingleTick_WithRecordedPointsCyclesSequentially()
    {
        var mockMouse = new Mock<IMouseInterop>();
        var points = new[] { new CursorPoint(10, 20), new CursorPoint(300, 400) };
        using var engine = new OsAutoClickEngine(mockMouse.Object);

        engine.Configure(intervalMs: 50, useRecordedPoints: true, replacePoints: points);
        engine.SetRunningForTest(true);

        engine.ExecuteSingleTick();
        engine.ExecuteSingleTick();
        engine.ExecuteSingleTick();

        mockMouse.Verify(m => m.SetCursorPosition(10, 20), Times.Exactly(2));
        mockMouse.Verify(m => m.SetCursorPosition(300, 400), Times.Once);
        mockMouse.Verify(m => m.Click(OsMouseButton.Left), Times.Exactly(3));
    }

    [Fact]
    public void ExecuteSingleTick_StopsAfterReachingRepeatCount()
    {
        var mockMouse = new Mock<IMouseInterop>();
        using var engine = new OsAutoClickEngine(mockMouse.Object);

        engine.Configure(intervalMs: 50, repeatCount: 2);
        engine.SetRunningForTest(true);

        Assert.True(engine.ExecuteSingleTick());
        Assert.True(engine.IsRunning);

        Assert.False(engine.ExecuteSingleTick());
        Assert.False(engine.IsRunning);
        Assert.Equal(2, engine.ClicksDone);
    }

    [Fact]
    public void Toggle_SwitchesRunningStateCleanly()
    {
        var mockMouse = new Mock<IMouseInterop>();
        using var engine = new OsAutoClickEngine(mockMouse.Object);
        engine.Configure(intervalMs: 500);

        engine.Toggle();
        Assert.True(engine.IsRunning);

        engine.Toggle();
        Assert.False(engine.IsRunning);
    }
}
