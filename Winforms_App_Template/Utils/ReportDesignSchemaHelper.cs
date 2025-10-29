// ReportDesignSchemaHelper.cs
using System;
using System.Collections.Generic;               // HashSet<T>
using System.Data;                              // DataSet, DataTable
using DevExpress.XtraReports.UI;                // XtraReport, XRSubreport, XRControl, Band, ...
using System.Linq;                              // Enumerable helpers

public static class ReportDesignSchemaHelper
{
    /// <summary>
    /// Gắn schema thiết kế (DataSet/DataTable) cho report chính và mọi subreport bên trong.
    /// Mục tiêu:
    ///  - Cho Designer hiển thị Field List (kéo-thả Expression [Field]) mà KHÔNG serialize type custom.
    ///  - Tránh lỗi NonTrustedTypeDeserializationException khi LoadLayoutFromXml.
    /// 
    /// Cách dùng:
    ///  - Gọi trước khi mở End-User Designer:
    ///      ReportDesignSchemaHelper.AttachDesignSchema(rpt);
    ///  - Ở runtime in ấn: bạn vẫn gán DataSource thật (List<Catthoong_ReportRow>, List<Standard_Model>) như bình thường.
    /// </summary>
    public static void AttachDesignSchema(XtraReport mainReport)
    {
        if (mainReport == null) throw new ArgumentNullException(nameof(mainReport));

        // 1) Tạo DataSet schema cho phần MAIN + STANDARDS
        var dsMain = BuildDesignSchema(); // instance riêng cho report chính

        // 2) Đưa DataSet vào ComponentStorage của report chính để Designer quản lý vòng đời component
        mainReport.ComponentStorage.Add(dsMain);

        // 3) Gán cho report chính DataSource = design schema (bảng "Main")
        mainReport.DataSource = dsMain;     // Field List sẽ thấy các cột của bảng "Main"
        mainReport.DataMember = "Main";

        // 4) Duyệt toàn bộ XRSubreport nằm trong report chính (và cả lồng nhiều lớp)
        foreach (var sub in EnumerateSubreports(mainReport))
        {
            // Với mỗi XRSubreport, nếu có instance report con thì gắn schema riêng cho nó
            if (sub.ReportSource is XtraReport child)
            {
                // Tạo 1 DataSet schema mới cho từng subreport (không dùng chung cùng instance,
                // vì cùng 1 IComponent không thể Add vào 2 ComponentStorage khác nhau)
                var dsChild = BuildDesignSchema();

                // Cho subreport quản lý schema này
                child.ComponentStorage.Add(dsChild);

                // Gán DataSource cho subreport = bảng Standards (đây là thiết kế)
                child.DataSource = dsChild;
                child.DataMember = "Standards";
            }
        }
    }

    /// <summary>
    /// Xây dựng DataSet schema chứa 2 bảng:
    ///  - "Main": khớp các property public của Catthoong_ReportRow
    ///  - "Standards": khớp các property public của Standard_Model
    /// Có thêm quan hệ Main_Standards theo idInput (tùy chọn, hỗ trợ Field List dạng master-detail).
    /// </summary>
    private static DataSet BuildDesignSchema()
    {
        // Tạo DataSet đặt tên cho dễ nhận biết trong Designer
        var ds = new DataSet("DesignSchema");

        // === BẢNG MAIN (tương ứng Catthoong_ReportRow) ===
        var tMain = new DataTable("Main");

        // Các cột chính – dùng kiểu .NET cơ bản (int/string/DateTime) để Designer đọc
        tMain.Columns.Add("idInput", typeof(int));
        tMain.Columns.Add("MaKT", typeof(string));
        tMain.Columns.Add("StartTime", typeof(DateTime));
        tMain.Columns.Add("NguoiTT", typeof(string));
        tMain.Columns.Add("TenMay_Ban", typeof(string));

        // val1..val15: trong model là string?
        // -> tạo các cột string tương ứng để có thể bind [valX] trong Designer
        tMain.Columns.Add("val1", typeof(string));
        tMain.Columns.Add("val2", typeof(string));
        tMain.Columns.Add("val3", typeof(string));
        tMain.Columns.Add("val4", typeof(string));
        tMain.Columns.Add("val5", typeof(string));
        tMain.Columns.Add("val6", typeof(string));
        tMain.Columns.Add("val7", typeof(string));
        tMain.Columns.Add("val8", typeof(string));
        tMain.Columns.Add("val9", typeof(string));
        tMain.Columns.Add("val10", typeof(string));
        tMain.Columns.Add("val11", typeof(string));
        tMain.Columns.Add("val12", typeof(string));
        tMain.Columns.Add("val13", typeof(string));
        tMain.Columns.Add("val14", typeof(string));
        tMain.Columns.Add("val15", typeof(string));

        tMain.Columns.Add("SLSudung", typeof(int));
        tMain.Columns.Add("Remark", typeof(string));

        // 6 cột lỗi ngang
        tMain.Columns.Add("Bevel_Cut", typeof(int));
        tMain.Columns.Add("Flat", typeof(int));
        tMain.Columns.Add("Bavia", typeof(int));
        tMain.Columns.Add("Fall", typeof(int));
        tMain.Columns.Add("Beyond_The_Standard", typeof(int));
        tMain.Columns.Add("Other", typeof(int));

        ds.Tables.Add(tMain);

        // === BẢNG STANDARDS (tương ứng Standard_Model) ===
        var tStd = new DataTable("Standards");
        tStd.Columns.Add("idInput", typeof(int));
        tStd.Columns.Add("idStandard", typeof(int));
        tStd.Columns.Add("TenTieuChuan", typeof(string));
        tStd.Columns.Add("MaTieuChuan", typeof(string));
        tStd.Columns.Add("Loai_size", typeof(string));
        tStd.Columns.Add("Loai_kytu", typeof(string));
        tStd.Columns.Add("Loai_chieudai", typeof(string));
        tStd.Columns.Add("Loai_somay", typeof(string));
        tStd.Columns.Add("Loai_masp", typeof(string));
        tStd.Columns.Add("Loai_ten", typeof(string));
        // TCMin/TCMax trong model là string → để string (nếu muốn format khác có thể đổi kiểu)
        tStd.Columns.Add("TCMin", typeof(string));
        tStd.Columns.Add("TCMax", typeof(string));
        // Các cột khác nếu bạn muốn hiện trong Field List:
        tStd.Columns.Add("TenNVL", typeof(string));
        tStd.Columns.Add("MaNVL", typeof(string));
        tStd.Columns.Add("Fromdate", typeof(DateTime));
        tStd.Columns.Add("Todate", typeof(DateTime));

        ds.Tables.Add(tStd);

        // (Tuỳ chọn) Tạo quan hệ giữa Main và Standards theo idInput để Designer nhận biết master-detail
        ds.Relations.Add("Main_Standards",
            tMain.Columns["idInput"], tStd.Columns["idInput"], false);

        return ds;
    }

    /// <summary>
    /// Duyệt tất cả XRSubreport trong report, kể cả các subreport lồng nhiều tầng.
    /// Dùng HashSet<XtraReport> để tránh lặp vô hạn nếu có tham chiếu vòng.
    /// </summary>
    private static IEnumerable<XRSubreport> EnumerateSubreports(XtraReport report)
    {
        var visited = new HashSet<XtraReport>(ReferenceEqualityComparer<XtraReport>.Instance);
        return EnumerateReport(report, visited);
    }

    // Đệ quy: duyệt report → bands → controls → XRSubreport → (đệ quy vào ReportSource nếu là XtraReport)
    private static IEnumerable<XRSubreport> EnumerateReport(XtraReport report, HashSet<XtraReport> visited)
    {
        if (report == null) yield break;
        if (!visited.Add(report)) yield break; // tránh lặp khi gặp lại cùng instance

        foreach (Band band in report.Bands)
        {
            if (band == null) continue;

            foreach (var ctrl in EnumerateControls(band.Controls))
            {
                if (ctrl is XRSubreport sub)
                {
                    yield return sub;

                    // Nếu sub.ReportSource là 1 instance XtraReport → duyệt tiếp các sub-con
                    if (sub.ReportSource is XtraReport child)
                    {
                        foreach (var nested in EnumerateReport(child, visited))
                            yield return nested;
                    }
                }
            }
        }
    }

    // Duyệt toàn bộ cây control trong 1 band
    private static IEnumerable<XRControl> EnumerateControls(XRControlCollection controls)
    {
        foreach (XRControl c in controls)
        {
            yield return c;
            // đệ quy con
            foreach (var inner in EnumerateControls(c.Controls))
                yield return inner;
        }
    }

    /// <summary>
    /// So sánh tham chiếu (ReferenceEquals) cho HashSet tránh lẫn Equals override.
    /// </summary>
    private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ReferenceEqualityComparer<T> Instance = new();
        public bool Equals(T x, T y) => ReferenceEquals(x, y);
        public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
