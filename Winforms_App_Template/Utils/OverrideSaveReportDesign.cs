// using cần có:
using DevExpress.XtraReports.UserDesigner;
using DevExpress.XtraReports.UI;
using System.IO;
using DevExpress.XtraEditors;

// Handler cho lệnh Save trong End-User Designer (Save, Save As, Save All)
sealed class SaveCommandHandler : ICommandHandler
{
    private readonly XRDesignPanel _panel;          // panel hiện tại (có thể là report chính hoặc subreport)
    private readonly ReportLayoutStore _store;      // store được khởi tạo THEO KEY của panel hiện tại
    private readonly string _reportKey;             // key hiển thị/lưu (DisplayName hoặc Type.Name)

    public SaveCommandHandler(XRDesignPanel panel, ReportLayoutStore store, string reportKey)
    {
        _panel = panel; _store = store; _reportKey = reportKey;
    }

    // Nói cho Designer biết handler này sẽ xử lý các lệnh nào
    public bool CanHandleCommand(ReportCommand command, ref bool useNextHandler)
    {
        // BẮT cả 3 lệnh: Save, Save As, Save All
        if (command == ReportCommand.SaveFile
         || command == ReportCommand.SaveFileAs
         || command == ReportCommand.SaveAll)
        {
            useNextHandler = false;   // ĐỪNG chuyển sang handler mặc định (để khỏi bật SaveDialog)
            return true;              // Tôi sẽ tự xử lý
        }
        useNextHandler = true;        // Lệnh khác → để handler tiếp theo xử lý
        return false;
    }

    // Thực thi lệnh Save: lưu AppData + lưu DB, không hiện hộp thoại
    public void HandleCommand(ReportCommand command, object[] args)
    {
        try
        {
            // 1) Lưu LOCAL (AppData) để máy này chạy offline/lần sau vẫn có layout mới
            var localPath = GetLocalLayoutPath(_reportKey);
            _panel.Report.SaveLayoutToXml(localPath);  // Hàm lưu vào local của DevXpress

            // 2) Lưu DB (tập trung) để các máy khác nạp được giao diện mới
            // ICommandHandler là sync → gọi helper async theo kiểu đồng bộ
            var newVersion = Task.Run(async () =>
                    await _store.SaveAsync(_panel.Report).ConfigureAwait(false)
                ).GetAwaiter().GetResult();

            // 3) Báo “đã lưu” để Designer tắt dấu *
            _panel.ReportState = ReportState.Saved;

            // (tuỳ chọn) thông báo
            XtraMessageBox.Show($"Đã lưu '{_reportKey}' v{newVersion}");
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show($"Lỗi khi lưu '{_reportKey}': {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Đường dẫn cache: %LOCALAPPDATA%\{AppName}\Reports\{reportName}.repx
    private static string GetLocalLayoutPath(string reportName)
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Application.ProductName, "Reports");
        Directory.CreateDirectory(baseDir);
        return Path.Combine(baseDir, $"{reportName}.repx");
    }
}
