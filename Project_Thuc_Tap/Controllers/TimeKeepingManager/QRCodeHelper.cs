using QRCoder;

namespace Project_Thuc_Tap.Controllers.TimeKeeping
{
    public class QRCodeHelper
    {
        public static string GenerateQRCode(string userId)
        {
            try
            {
                // Khởi tạo QR code generator
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(userId, QRCodeGenerator.ECCLevel.Q);

                // Tạo đồ họa QR code dưới dạng byte
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20);

                // Định nghĩa đường dẫn lưu ảnh trong thư mục wwwroot/Images
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", $"{userId}.png");

                // Tạo thư mục nếu chưa có
                string? dirPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath!);
                }

                // Lưu ảnh QR code
                File.WriteAllBytes(filePath, qrCodeImage);

                // Trả về đường dẫn ảnh để hiển thị trên web
                return $"/Images/{userId}.png";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo QR code: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
