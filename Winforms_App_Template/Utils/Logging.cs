using System;
using System.IO;
// using System.Threading; // Có thể giữ hoặc bỏ. Ta sẽ dùng fully qualified để tránh nhầm.
using Serilog;
using Serilog.Exceptions;
using Serilog.Enrichers;

namespace Winforms_App_Template.Utils
{
    /// <summary>
    /// Quản lý cấu hình Serilog:
    /// - Ghi log theo ngày vào %LOCALAPPDATA%\{AppName}\{dd-MM-yyyy}\log\log-<date>.txt
    /// - Tự động rollover sang thư mục ngày mới lúc 00:00 (không cần restart)
    /// - Dọn thư mục log cũ theo số ngày giữ
    /// </summary>
    public static class Logging
    {
        // Khóa đồng bộ để tránh race-condition khi thao tác với tệp tin đồng thời
        // lock (_sync) đảm bảo chỉ 1 luồng vào đoạn “tối quan trọng” tại một thời điểm.
        private static readonly object _sync = new();

        // Tên app để xây đường dẫn
        private static string _appName = "App";

        // Số ngày giữ log (thư mục ngày cũ hơn giá trị này sẽ bị xóa)
        private static int _retainedDays = 14;

        // !!! Dùng fully-qualified tên lớp Timer để tránh mập mờ giữa WinForms.Timer và Threading.Timer
        private static System.Threading.Timer? _midnightTimer;

        // Thư mục log của NGÀY ĐANG DÙNG (luôn đồng bộ với logger hiện tại)
        private static string _todayDir = string.Empty;

        /// <summary>
        /// Cho phép Form/Code khác mở thư mục log hiện tại (đang ghi vào).
        /// Dùng lock để đọc biến an toàn.
        /// </summary>
        public static string TodayLogDirectory
        {
            get { lock (_sync) return _todayDir; }
        }

        /// <summary>
        /// Khởi tạo logger. Gọi sớm trong Program.Main().
        /// </summary>
        /// <param name="appName">Tên phần mềm</param>
        /// <param name="retainedDays">Số ngày giữ thư mục log (>=0)</param>
        public static void Initialize(string appName, int retainedDays)
        {
            lock (_sync)
            {
                // Chuẩn hóa tham số
                _appName = string.IsNullOrWhiteSpace(appName) ? "App" : appName.Trim();
                _retainedDays = Math.Max(0, retainedDays);

                // Cấu hình logger cho NGÀY hiện tại
                ConfigureForToday_NoLock(DateTime.Now);

                // Đặt lịch rollover vào 00:00 của ngày tiếp theo
                ScheduleMidnightRollover_NoLock();

                // Dọn thư mục log cũ nếu cần
                CleanupOld_NoLock();
            }
        }

        /// <summary>
        /// Tắt logger khi thoát app (flush buffer + hủy timer).
        /// </summary>
        public static void Shutdown()
        {
            lock (_sync)
            {
                try { _midnightTimer?.Dispose(); } catch { /* bỏ qua lỗi */ }
                _midnightTimer = null;
            }
            try { Log.CloseAndFlush(); } catch { /* bỏ qua lỗi */ }
        }

        /// <summary>
        /// Tạo thư mục ngày + cấu hình Serilog cho NGÀY tương ứng.
        /// KHÔNG được gọi ngoài lock (_sync).
        /// </summary>
        private static void ConfigureForToday_NoLock(DateTime now)
        {
            // Lấy format ngày từ Constants (ví dụ "dd-MM-yyyy")
            var dateFolder = now.ToString(Constants.DATE_FOLDER_FORMAT);

            // Xây đường dẫn: %LOCALAPPDATA%\{AppName}\{dd-MM-yyyy}\log
            _todayDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                _appName, dateFolder, "log");

            // Tạo thư mục nếu chưa có
            Directory.CreateDirectory(_todayDir);

            // File log template (Serilog tự thêm ngày vì RollingInterval.Day)
            var logPath = Path.Combine(_todayDir, Constants.LOG_FILE_TEMPLATE);

            // CẤU HÌNH SERILOG
            Log.Logger = new LoggerConfiguration()
                // ===== Enrichers =====
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                // ===== Level mặc định =====
                .MinimumLevel.Information()
                // ===== Sink: ghi file (async để không block UI) =====
                .WriteTo.Async(a => a.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day,      // tách file theo ngày
                    retainedFileCountLimit: 31,                // Serilog giữ tối đa X file trong thư mục; ta còn dọn bằng CleanupOld_NoLock()
                    shared: true,                              // cho phép process khác đọc khi cần
                    // Output template có thêm các biến CallerFile/CallerLine/CallerMember (do LogEx đính vào mỗi log)
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {SourceContext} | " +
                        "{CallerFile}:{CallerLine} ({CallerMember}) | {Message:lj}{NewLine}{Exception}"
                ))
                .CreateLogger();
        }

        /// <summary>
        /// Lên lịch 1 timer chạy đúng 00:00 hôm sau để reconfigure logger sang thư mục ngày mới.
        /// KHÔNG được gọi ngoài lock (_sync).
        /// </summary>
        private static void ScheduleMidnightRollover_NoLock()
        {
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1); // 00:00 ngày mai
            var due = nextMidnight - now;           // thời gian còn lại tới 00:00

            // Hủy timer cũ nếu có
            _midnightTimer?.Dispose();

            // Tạo timer MỚI (dùng System.Threading.Timer — fully-qualified)
            _midnightTimer = new System.Threading.Timer(_ =>
            {
                lock (_sync)
                {
                    try
                    {
                        // Ghi 1 dòng thông báo trước khi chuyển
                        Log.Information("Rollover: chuẩn bị chuyển logger sang thư mục ngày mới.");

                        // Nhả file cũ (rất quan trọng trước khi tạo logger mới)
                        Log.CloseAndFlush();

                        // Cấu hình lại logger cho ngày mới (tạo thư mục ngày mới + file mới)
                        ConfigureForToday_NoLock(DateTime.Now);

                        // Ghi 1 dòng xác nhận đã chuyển xong + đường dẫn mới
                        Log.Information("Rollover: đã chuyển logger. LogDir={LogDir}", _todayDir);

                        // Lên lịch cho 00:00 ngày kế tiếp
                        ScheduleMidnightRollover_NoLock();

                        // Dọn thư mục log cũ
                        CleanupOld_NoLock();
                    }
                    catch (Exception ex)
                    {
                        // Trong trường hợp hiếm gặp (cấu hình thất bại), tạo 1 logger fallback để không mất log
                        try
                        {
                            Log.Logger = new LoggerConfiguration()
                                .WriteTo.File(Path.Combine(_todayDir, "fallback-.txt"),
                                              rollingInterval: RollingInterval.Day)
                                .CreateLogger();

                            Log.Error(ex, "Rollover: lỗi khi tái cấu hình logger nửa đêm");
                        }
                        catch { /* Không để logger làm crash app */ }
                    }
                }
            },
            state: null,
            dueTime: due,                    // chạy lần đầu vào đúng 00:00
            period: System.Threading.Timeout.InfiniteTimeSpan // chỉ chạy 1 lần; ta sẽ tự đặt lại sau khi chuyển xong
            );
        }

        /// <summary>
        /// Xóa các thư mục ngày cũ hơn _retainedDays.
        /// KHÔNG được gọi ngoài lock (_sync).
        /// </summary>
        private static void CleanupOld_NoLock()
        {
            if (_retainedDays <= 0) return;

            try
            {
                var baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    _appName);

                if (!Directory.Exists(baseDir)) return;

                var cutoff = DateTime.Today.AddDays(-_retainedDays);

                foreach (var dayDir in Directory.EnumerateDirectories(baseDir))
                {
                    var name = Path.GetFileName(dayDir);

                    // Parse tên thư mục theo format trong Constants (vd "dd-MM-yyyy")
                    if (DateTime.TryParseExact(name, Constants.DATE_FOLDER_FORMAT,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var dt))
                    {
                        // Nếu thư mục cũ hơn ngưỡng thì xóa cả cây
                        if (dt.Date < cutoff.Date)
                        {
                            try { Directory.Delete(dayDir, true); } catch { /* bỏ qua lỗi xóa */ }
                        }
                    }
                }
            }
            catch
            {
                // Không để việc dọn dẹp làm gián đoạn app
            }
        }
    }
}
