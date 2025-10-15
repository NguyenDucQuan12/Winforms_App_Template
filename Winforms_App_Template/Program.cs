using Microsoft.VisualBasic.Logging;
using Winforms_App_Template.Utils;
using Winforms_App_Template.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            // Tạo Generic Host cho WinForms: có DI, Configuration, Logging
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    // Xóa các nguồn mặc định nếu muốn, hoặc giữ nguyên
                    // cfg.Sources.Clear();

                    // Base path = thư mục exe (bin/Debug/net8.0-windows)
                    cfg.SetBasePath(AppContext.BaseDirectory);

                    // Nạp file ở đường dẫn tương đối "Database/appsettings.json"
                    // Vì csproj đã copy file này ra bin, runtime sẽ đọc được.
                    cfg.AddJsonFile("Database/appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((ctx, services) =>
                {
                    // Đăng ký DbContext dùng SqlServer, lấy chuỗi từ config đã nạp
                    services.AddDbContext<AppDbContext>(opt =>
                    {
                        var cs = ctx.Configuration.GetConnectionString("SqlServer");
                        opt.UseSqlServer(cs, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null));

                        // Dev: bật log chi tiết (Serilog của bạn sẽ nhận những log này)
                        opt.EnableDetailedErrors();
                        opt.EnableSensitiveDataLogging();
                    });

                    // Đăng ký Form chính
                    services.AddScoped<Form1>();
                })
                .Build();

            // Log.Information("Ứng dụng khởi động. LogDir={LogDir}", Logging.TodayLogDirectory);
            LogEx.Info("User bấm nút Xin chào.");
            // Tạo một scope sống "trọn vòng đời form"
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                // LẤY FORM1 từ DI (DbContext sẽ được inject tự động)
                var form = services.GetRequiredService<Form1>();

                // Chạy ứng dụng với form đã resolve
                Application.Run(form);
            }
            LogEx.Information("Ứng dụng thoát.");
            Logging.Shutdown();
        }
    }
}