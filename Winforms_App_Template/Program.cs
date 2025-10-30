using Winforms_App_Template.Utils;
using Winforms_App_Template.Forms;
using System;
using System.Windows.Forms;
using DevExpress.Utils;                           // DeserializationSettings
using DevExpress.XtraReports.Configuration;      // Settings.Default.UserDesignerOptions
using Winforms_App_Template.Database.Model;      // Catthoong_ReportRow, Standard_Model

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
            // Trust assembly/Type trước khi LoadLayoutFromXml ======
            DeserializationSettings.RegisterTrustedAssembly(typeof(Program).Assembly);
            // Nếu dùng DataTable làm "schema design-time" cũng whitelist luôn:
            DeserializationSettings.RegisterTrustedClass(typeof(System.Data.DataTable));
            // bật chế độ binding hiện đại trong Designer
            //Settings.Default.UserDesignerOptions.DataBindingMode = DataBindingMode.ExpressionsAdvanced;
            // ==============================================================================

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
            Application.Run(new Catongtho());
            LogEx.Information("Ứng dụng thoát.");
            Logging.Shutdown();
        }
    }
}