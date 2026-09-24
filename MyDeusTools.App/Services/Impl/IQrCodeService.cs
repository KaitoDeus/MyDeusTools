using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MyDeusTools.App.Services.Impl
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Tạo mã QR từ văn bản dưới dạng BitmapSource hiển thị trên WPF
        /// </summary>
        BitmapSource GenerateQrCode(string content, int pixelsPerModule = 15);

        /// <summary>
        /// Tạo mã QR dưới dạng mảng byte PNG để lưu file hoặc copy vào clipboard
        /// </summary>
        byte[] GenerateQrCodePng(string content, int pixelsPerModule = 15);

        /// <summary>
        /// Quét và giải mã QR từ hình ảnh BitmapSource
        /// </summary>
        string? DecodeQrCode(BitmapSource bitmapSource);

        /// <summary>
        /// Quét và giải mã QR từ đường dẫn file hình ảnh
        /// </summary>
        string? DecodeQrCodeFromFile(string filePath);

        /// <summary>
        /// Chụp một vùng màn hình và giải mã QR
        /// </summary>
        string? DecodeFromScreenRegion(int x, int y, int width, int height);
    }
}
