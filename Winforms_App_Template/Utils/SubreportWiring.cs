using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public static class SubreportWiring
{
    /// <summary>
    /// Nối subreport (XRSubreport) trong 1 DetailReportBand để:
    /// - Lấy khóa hiện tại từ bối cảnh band qua sub.GetCurrentColumnValue(parentKeyField)
    /// - Từ khóa → tra từ điển → bơm list con vào child.ReportSource.DataSource
    /// - Ẩn sub nếu không có dữ liệu
    ///
    /// Tham số:
    /// - hostBandName: tên DetailReportBand chứa subreport này
    /// - subreportName: Name của control XRSubreport ở trong host band
    /// - parentKeyField: tên field khóa ở model cha (vd: "idInput")
    /// - resolveChildren: hàm nhận object key → trả IEnumerable<TChild> (ví dụ tra stdMap)
    /// </summary>
    public static void WireSubreportLookup<TChild>(
    XtraReport rpt,
    string hostBandName,
    string subreportName,
    string parentKeyField,
    Func<object?, IEnumerable<TChild>> resolveChildren)
    {
        if (rpt == null) throw new ArgumentNullException(nameof(rpt));
        if (resolveChildren == null) throw new ArgumentNullException(nameof(resolveChildren));

        // 1) Tìm đúng band host
        var host = FindDetailReportBand(rpt, hostBandName);

        // 2) Tìm đúng control XRSubreport theo tên
        var sub = host.FindControl(subreportName, ignoreCase: true) as XRSubreport
                  ?? throw new InvalidOperationException($"Không tìm thấy XRSubreport '{subreportName}' trong band '{hostBandName}'.");

        // 3) Đảm bảo có ReportSource
        if (sub.ReportSource == null)
            throw new InvalidOperationException($"XRSubreport '{subreportName}' chưa có ReportSource (report con).");

        var child = (XtraReport)sub.ReportSource;

        // 4) Dọn sẵn mọi binding cũ (nếu lỡ set DataMember ở design-time)
        child.DataSource = null;
        child.DataMember = null;

        // 5) Tránh gắn trùng handler khi gọi nhiều lần
        sub.BeforePrint -= OnSubBeforePrint;
        sub.BeforePrint += OnSubBeforePrint;

        // 6) Handler: ngay trước khi in mỗi dòng của host band
        void OnSubBeforePrint(object? sender, CancelEventArgs e)
        {
            // Lấy khóa hiện tại từ bối cảnh band *đúng cách*: dùng sub.Report.GetCurrentColumnValue(...)
            // sub.Report sẽ trả về đối tượng XtraReport cha mà subreport này đang nằm trên đó.
            object? key = sub.Report.GetCurrentColumnValue(parentKeyField);

            // Tra danh sách con theo khóa (nếu null → trả Enumerable.Empty<TChild>())
            var list = resolveChildren(key) ?? Enumerable.Empty<TChild>();

            // Bơm data vào report con
            child.DataSource = list;
            child.DataMember = null;

            // Ẩn sub nếu rỗng để không chiếm chỗ
            sub.Visible = list.Any();
        }
    }

    /// <summary>
    /// Tìm DetailReportBand theo Name (ví dụ: "Catongtho_Report")
    /// </summary>
    public static DetailReportBand FindDetailReportBand(XtraReport rpt, string bandName)
    {
        if (rpt == null) throw new ArgumentNullException(nameof(rpt));
        foreach (Band b in rpt.Bands)
        {
            if (b is DetailReportBand dr && string.Equals(dr.Name, bandName, StringComparison.Ordinal))
                return dr;
        }
        throw new InvalidOperationException($"Không tìm thấy DetailReportBand '{bandName}'.");
    }

    /// <summary>
    /// Gán datasource cho 1 DetailReportBand theo tên.
    /// - Chỉ band đó nhận data, các band khác độc lập.
    /// - Trong band, mọi [Field] sẽ bind theo kiểu của T.
    /// </summary>
    public static void BindDetailReport<T>(XtraReport rpt, string bandName, IEnumerable<T> data)
    {
        var dr = FindDetailReportBand(rpt, bandName);
        dr.DataSource = data; // Chỉ gán vào band
        dr.DataMember = null; // IEnumerable<T> không cần DataMember

        // (Tuỳ chọn) nếu band có GroupHeader/Detail con, bạn không cần làm gì thêm,
        // các XRTable/XRLabel trong đó dùng [Field] sẽ đọc theo dr.DataSource.
    }
}
