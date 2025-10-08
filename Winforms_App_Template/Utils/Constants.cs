// Utils/Constants.cs
namespace Winforms_App_Template.Utils
{
    /// <summary>
    /// Lưu các hằng số (constant) dùng toàn dự án.
    /// - Đặt ở 1 chỗ để dễ sửa đổi, đồng bộ.
    /// - Nên đặt readonly/const để không bị chỉnh nhầm lúc chạy.
    /// </summary>
    public static class Constants
    {
        // Tên ứng dụng để xây đường dẫn log: %LOCALAPPDATA%\{APP_NAME}\dd-MM-yyyy\log
        public const string APP_NAME = "My_App";

        // Số ngày giữ thư mục log (dọn cũ hơn giá trị này)
        public const int LOG_RETAIN_DAYS = 14;

        // (Tuỳ chọn) Bạn có thể lưu các giá trị khác như:
        public const string DATE_FOLDER_FORMAT = "dd-MM-yyyy";  // định dạng thư mục ngày
        public const string LOG_FILE_TEMPLATE = "log-.txt";     // mẫu tên file log Serilog (tự thêm ngày)
    }
}
