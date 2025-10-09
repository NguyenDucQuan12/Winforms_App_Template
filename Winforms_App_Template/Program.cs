using Microsoft.VisualBasic.Logging;
using Winforms_App_Template.Utils;

namespace Winforms_App_Template
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Khởi tạo logger để tự động sinh file ngày mới và xóa file quá hạn
            Logging.Initialize(Constants.APP_NAME, Constants.LOG_RETAIN_DAYS);

            Application.ThreadException += (s, e) =>
            {
                LogEx.Error(e.Exception, "Unhandled UI exception");
                MessageBox.Show("Đã xảy ra lỗi chưa xử lý. Vui lòng xem log.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                LogEx.Error(e.ExceptionObject as Exception, "Unhandled non-UI exception");
            };

            // Log.Information("Ứng dụng khởi động. LogDir={LogDir}", Logging.TodayLogDirectory);
            LogEx.Info("User bấm nút Xin chào.");
            Application.Run(new Main_Form());
            LogEx.Information("Ứng dụng thoát.");
            Logging.Shutdown();
        }
    }
}