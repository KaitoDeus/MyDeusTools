using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MyDeusTools.App.Services.Impl;
using QRCoder;
using ZXing;
using ZXing.Common;

namespace MyDeusTools.App.Services
{
    public class QrCodeService : IQrCodeService
    {
        public BitmapSource GenerateQrCode(string content, int pixelsPerModule = 15)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Nội dung tạo mã QR không được để trống.", nameof(content));
            }

            byte[] pngBytes = GenerateQrCodePng(content, pixelsPerModule);
            using var ms = new MemoryStream(pngBytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        public byte[] GenerateQrCodePng(string content, int pixelsPerModule = 15)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Nội dung tạo mã QR không được để trống.", nameof(content));
            }

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule);
        }

        public string? DecodeQrCode(BitmapSource bitmapSource)
        {
            if (bitmapSource == null) return null;

            try
            {
                var formatted = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0);
                int width = formatted.PixelWidth;
                int height = formatted.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                formatted.CopyPixels(pixels, stride, 0);

                var luminanceSource = new RGBLuminanceSource(pixels, width, height, RGBLuminanceSource.BitmapFormat.BGRA32);
                var binarizer = new HybridBinarizer(luminanceSource);
                var binBitmap = new BinaryBitmap(binarizer);
                var reader = new MultiFormatReader();

                var hints = new Dictionary<DecodeHintType, object>
                {
                    { DecodeHintType.TRY_HARDER, true },
                    { DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat> { BarcodeFormat.QR_CODE } }
                };

                var result = reader.decode(binBitmap, hints);
                return result?.Text;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi giải mã QR: {ex.Message}");
                return null;
            }
        }

        public string? DecodeQrCodeFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            try
            {
                var uri = new Uri(Path.GetFullPath(filePath));
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = uri;
                bitmap.EndInit();
                bitmap.Freeze();

                return DecodeQrCode(bitmap);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc file hình ảnh QR: {ex.Message}");
                return null;
            }
        }

        public string? DecodeFromScreenRegion(int x, int y, int width, int height)
        {
            if (width <= 0 || height <= 0) return null;

            try
            {
                using var bmp = new System.Drawing.Bitmap(width, height);
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
                }

                var rect = new System.Drawing.Rectangle(0, 0, width, height);
                var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                try
                {
                    int length = bmpData.Stride * height;
                    byte[] bytes = new byte[length];
                    Marshal.Copy(bmpData.Scan0, bytes, 0, length);

                    var luminanceSource = new RGBLuminanceSource(bytes, width, height, RGBLuminanceSource.BitmapFormat.BGRA32);
                    var binarizer = new HybridBinarizer(luminanceSource);
                    var binBitmap = new BinaryBitmap(binarizer);
                    var reader = new MultiFormatReader();

                    var hints = new Dictionary<DecodeHintType, object>
                    {
                        { DecodeHintType.TRY_HARDER, true },
                        { DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat> { BarcodeFormat.QR_CODE } }
                    };

                    var result = reader.decode(binBitmap, hints);
                    return result?.Text;
                }
                finally
                {
                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi chụp màn hình giải mã QR: {ex.Message}");
                return null;
            }
        }
    }
}
