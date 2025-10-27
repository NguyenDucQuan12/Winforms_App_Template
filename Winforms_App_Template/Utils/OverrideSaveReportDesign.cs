// using cần có:
using DevExpress.XtraReports.UserDesigner;
using DevExpress.XtraReports.UI;
using System.IO;

// Handler cho lệnh Save trong End-User Designer (Save, Save As, Save All)
sealed class SaveCommandHandler : ICommandHandler
{
    private readonly XRDesignPanel _panel;          // Panel hiện đang edit report
    private readonly ReportLayoutStore _store;      // Helper lưu/đọc DB (class của bạn)
    private readonly string _reportName;            // Key logic dùng đặt tên cache AppData

    public SaveCommandHandler(XRDesignPanel panel, ReportLayoutStore store, string reportName)
    {
        _panel = panel;
        _store = store;
        _reportName = reportName;
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
        // 1) Lưu LOCAL (AppData) để máy này chạy offline/lần sau vẫn có layout mới
        var localPath = GetLocalLayoutPath(_reportName);
        _panel.Report.SaveLayoutToXml(localPath);  // Hàm lưu vào local của DevXpress

        // 2) Lưu DB (tập trung) để các máy khác nạp được giao diện mới
        // ICommandHandler là sync → gọi helper async theo kiểu đồng bộ
        _store.SaveAsync(_panel.Report).GetAwaiter().GetResult();

        // 3) Báo “đã lưu” để Designer tắt dấu *
        _panel.ReportState = ReportState.Saved;
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
