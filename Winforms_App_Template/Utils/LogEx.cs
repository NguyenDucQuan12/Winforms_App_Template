using System.IO;
using System.Runtime.CompilerServices;
using Serilog;

namespace Winforms_App_Template.Utils
{
    /// <summary>
    /// Helper cho Serilog để tự động nhúng Caller Info (file/line/member)
    /// vào mỗi log event, nhằm in ra "{CallerFile}:{CallerLine} ({CallerMember})".
    /// Không cần NuGet enricher ngoài.
    /// </summary>
    public static class LogEx
    {
        /// <summary>
        /// Lấy tên tệp (không gồm path) một cách an toàn.
        /// </summary>
        private static string ShortFile(string path)
        {
            try { return Path.GetFileName(path); } catch { return path; }
        }

        public static void Debug(
            string message,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            Log.ForContext("CallerFile", ShortFile(file))
               .ForContext("CallerLine", line)
               .ForContext("CallerMember", member)
               .Debug(message);
        }

        public static void Info(
            string message,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            Log.ForContext("CallerFile", ShortFile(file))
               .ForContext("CallerLine", line)
               .ForContext("CallerMember", member)
               .Information(message);
        }

        public static void Warn(
            string message,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            Log.ForContext("CallerFile", ShortFile(file))
               .ForContext("CallerLine", line)
               .ForContext("CallerMember", member)
               .Warning(message);
        }

        public static void Error(
            string message,
            System.Exception? ex = null,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            var logger = Log.ForContext("CallerFile", ShortFile(file))
                            .ForContext("CallerLine", line)
                            .ForContext("CallerMember", member);

            if (ex != null)
                logger.Error(ex, message);   // overload có Exception
            else
                logger.Error(message);
        }
    }
}
