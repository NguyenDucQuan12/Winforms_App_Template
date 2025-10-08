using System.Diagnostics;
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
    }
}
