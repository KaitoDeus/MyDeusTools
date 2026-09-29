using System;
using System.Globalization;
using System.Windows.Data;

namespace MyDeusTools.App.Services.Impl
{
    public class ImageFormatConverter : IValueConverter
    {
        public static readonly ImageFormatConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ImageTargetFormat format && parameter is ImageTargetFormat target)
            {
                return format == target;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is ImageTargetFormat target)
            {
                return target;
            }
            return Binding.DoNothing;
        }
    }

    public class ResizeModeIndexConverter : IValueConverter
    {
        public static readonly ResizeModeIndexConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ImageResizeMode mode)
            {
                return (int)mode;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index && Enum.IsDefined(typeof(ImageResizeMode), index))
            {
                return (ImageResizeMode)index;
            }
            return ImageResizeMode.Original;
        }
    }
}
