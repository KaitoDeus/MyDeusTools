using System;

    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public enum ClickType
    {
        Single,
        Double
    }

    public struct MousePoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public interface IAutoClickService
    {
        bool IsRunning { get; }
        int Interval { get; }
        List<MousePoint> RecordedPoints { get; }
        
        void Start(int intervalMs, MouseButton button = MouseButton.Left, ClickType clickType = ClickType.Single, int repeatCount = 0);
        void Stop();
        
        void ClearRecordedPoints();
    }
