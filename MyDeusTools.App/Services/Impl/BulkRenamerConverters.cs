using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MyDeusTools.App.Services.Impl
{
    public class RenameStatusToBrushConverter : IValueConverter
    {
        public static readonly RenameStatusToBrushConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RenameItemStatus status)
            {
                return status switch
                {
                    RenameItemStatus.Ready => new SolidColorBrush(Color.FromRgb(16, 124, 65)),         // Green
                    RenameItemStatus.Renamed => new SolidColorBrush(Color.FromRgb(0, 120, 212)),       // Blue
                    RenameItemStatus.Unchanged => new SolidColorBrush(Color.FromRgb(128, 128, 128)),    // Gray
                    RenameItemStatus.Reverted => new SolidColorBrush(Color.FromRgb(216, 114, 0)),      // Orange
                    RenameItemStatus.ConflictDuplicate or
                    RenameItemStatus.ConflictExists or
                    RenameItemStatus.InvalidCharacters or
                    RenameItemStatus.Failed => new SolidColorBrush(Color.FromRgb(209, 52, 56)),        // Red
                    _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))
                };
            }

            return new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    public class GreaterThanZeroToVisibilityConverter : IValueConverter
    {
        public static readonly GreaterThanZeroToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    public class BooleanInverseConverter : IValueConverter
    {
        public static readonly BooleanInverseConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            return false;
        }
    }
}
