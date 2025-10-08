using Serilog;                                  // Serilog core (ILogger, Log)
using System;                                   // Exception
using System.IO;                                // Path.GetFileName
using System.Runtime.CompilerServices;          // CallerFilePath/LineNumber/MemberName
using ILogger = Serilog.ILogger;                // Alias tên ILogger cho gọn, tránh trùng

namespace Winforms_App_Template.Utils
{
    /// <summary>
    /// Bọc Serilog để:
    /// 1) Tự gắn Caller Info (file/member/line) vào mỗi log event (giúp định vị nguồn log).
    /// 2) Giữ style "message template + params" (structured logging) của Serilog.
    /// 
    /// Lưu ý: Mọi API ở đây đều là static tiện dụng (utility), không cần khởi tạo instance.
    /// </summary>
    public static class LogEx
    {
        // Lấy tên file ngắn gọn từ đường dẫn đầy đủ. Nếu path lạ gây lỗi thì trả về nguyên chuỗi.
        private static string ShortFile(string path)
        {
            try { return Path.GetFileName(path); }
            catch { return path; }
        }

        /// <summary>
        /// Tạo một logger có kèm "Caller Info" bằng cách enrich 3 property:
        /// - {CallerFile}   : tên file nguồn (không gồm path)
        /// - {CallerMember} : tên method gọi
        /// - {CallerLine}   : số dòng lệnh gọi
        /// 
        /// Các tham số [Caller*] được compiler chèn tự động tại CHỖ GỌI, nên ở mỗi lần gọi LogEx.*
        /// ta đều có info chính xác về vị trí gọi log.
        /// </summary>
        private static ILogger WithCaller(
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            return Log.ForContext("CallerFile", ShortFile(file))   // {CallerFile}
                      .ForContext("CallerMember", member)            // {CallerMember}
                      .ForContext("CallerLine", line);             // {CallerLine}
        }

        // ====== VERBOSE ======
        public static void Verbose(string template, params object?[] args) =>
            WithCaller().Verbose(template, args);

        public static void Verbose(Exception ex, string template, params object?[] args) =>
            WithCaller().Verbose(ex, template, args);

        // ====== DEBUG ======
        public static void Debug(string template, params object?[] args) =>
            WithCaller().Debug(template, args);

        public static void Debug(Exception ex, string template, params object?[] args) =>
            WithCaller().Debug(ex, template, args);

        // ====== INFORMATION ======
        // Hàm không exception:
        public static void Information(string template, params object?[] args) =>
            WithCaller().Information(template, args);

        // Hàm CÓ exception nhưng cho phép nullable:
        // => Nếu ex == null, ta tự fallback sang overload không-exception để tránh cảnh báo nullable.
        public static void Information(Exception? ex, string template, params object?[] args)
        {
            var logger = WithCaller();
            if (ex is null)
                logger.Information(template, args);
            else
                logger.Information(ex, template, args);
        }

        // ====== WARNING ======
        public static void Warning(string template, params object?[] args) =>
            WithCaller().Warning(template, args);

        public static void Warning(Exception ex, string template, params object?[] args) =>
            WithCaller().Warning(ex, template, args);

        // ====== ERROR ======
        // (CHỈNH THEO YÊU CẦU)
        // Nhận Exception? (nullable). Nếu null -> gọi logger.Error(message, args)
        // Nếu có exception -> gọi logger.Error(exception, message, args)
        //
        // Mục tiêu: Khi nơi gọi (ví dụ Polly v8 OnOpened) cung cấp Exception? (có thể null),
        // ta KHÔNG bị cảnh báo "Possible null reference argument..." nữa, đồng thời vẫn log đúng.
        public static void Error(Exception? ex, string template, params object?[] args)
        {
            var logger = WithCaller();                // enrich caller info cho event log hiện tại
            if (ex is null)
            {
                // Trường hợp KHÔNG có exception: log message thuần
                logger.Error(template, args);
            }
            else
            {
                // Trường hợp CÓ exception: log kèm exception để thấy stack trace & thông tin lỗi đầy đủ
                logger.Error(ex, template, args);
            }
        }

        // Vẫn giữ overload "không exception" cho Error:
        public static void Error(string template, params object?[] args) =>
            WithCaller().Error(template, args);

        // ====== FATAL ======
        public static void Fatal(string template, params object?[] args) =>
            WithCaller().Fatal(template, args);

        public static void Fatal(Exception ex, string template, params object?[] args) =>
            WithCaller().Fatal(ex, template, args);

        // ====== (Tuỳ chọn) ALIAS NGẮN GỌN ======
        // Một số team thích tên ngắn: Info = Information, Warn = Warning, Err = Error
        public static void Info(string template, params object?[] args) => Information(template, args);
        public static void Warn(string template, params object?[] args) => Warning(template, args);
        public static void Err(string template, params object?[] args) => Error(template, args);
    }
}
