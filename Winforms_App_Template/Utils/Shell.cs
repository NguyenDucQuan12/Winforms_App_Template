using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Winforms_App_Template.Utils
{
    public static class Shell
    {
        public static void OpenFolder(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
            }
        }

        // Lấy đường dẫn AppData\{AppName}\Reports\{reportName}.repx để lưu local
        public static string GetLocalLayoutPath(string reportName)
        {
            string? appName = Application.ProductName ; // ví dụ "Winforms_App_Template"
            if (string.IsNullOrWhiteSpace(appName))
                appName = Constants.APP_NAME; // fallback

            // %LOCALAPPDATA%\{Tên ứng dụng}\Reports
            var baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,                  
                "Reports");

            Directory.CreateDirectory(baseDir);           // Đảm bảo thư mục tồn tại
            return Path.Combine(baseDir, $"{reportName}.repx"); // VD: Testreport.repx
        }
    }
}
