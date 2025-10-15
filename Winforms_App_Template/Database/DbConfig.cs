using System;

namespace Winforms_App_Template.Database
{
    /// <summary>
    /// Lấy chuỗi kết nối từ ENV trước, sau đó mới đến fallback truyền vào.
    /// GIỮ: Encrypt=True; Application Name=YourApp; Connect Timeout=15 là tối thiểu.
    /// </summary>
    public static class DbConfig
    {
        /// <summary>
        /// Lấy chuỗi kêt nối SQL Server.
        /// </summary>
        /// <param name="fallback"></param>
        /// <returns>Connection String</returns>
        public static string GetConnectionString(string? fallback = null)
        {
            // 1) Ưu tiên ENV để dễ đổi ở máy thật / CI/CD
            var env = Environment.GetEnvironmentVariable("YOURAPP_SQL_CONN");
            if (!string.IsNullOrWhiteSpace(env))
                return env;

            // 2) Dùng fallback nếu có
            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback;

            // 3) Mặc định DEV (đổi lại cho phù hợp)
            // - TrustServerCertificate=False: nên cấu hình CA đúng; dev có thể tạm True
            // - Connect Timeout=15: kết nối không quá 15s
            // - Application Name: giúp DBA truy vết kết nối
            return "Server=localhost,1433;" +
                    "Database=TestDB;" +
                    "User Id=sa;" +
                    "Password=123456789;" +
                    "TrustServerCertificate=True;" +
                    "Connect Timeout=15;" +
                    "Application Name=Test";
        }
    }
}