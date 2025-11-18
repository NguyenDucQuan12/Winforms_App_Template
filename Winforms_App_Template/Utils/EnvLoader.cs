using System;
using System.IO;

namespace Winforms_App_Template.Utils
{
    public static class EnvLoader
    {
        public static void LoadFromDotEnv(string path = ".env")
        {
            try
            {
                if (!File.Exists(path))
                    return;

                var lines = File.ReadAllLines(path);
                foreach (var raw in lines)
                {
                    var line = raw.Trim();

                    // Bỏ qua dòng trống hoặc comment
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var idx = line.IndexOf('=', StringComparison.Ordinal);
                    if (idx <= 0) continue;

                    var key = line.Substring(0, idx).Trim();
                    var value = line.Substring(idx + 1).Trim();

                    if (!string.IsNullOrEmpty(key))
                    {
                        // Set vào process env
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
            }
            catch
            {
                // Có thể log nếu cần, ở đây tạm bỏ qua
            }
        }
    }
}
