using DevExpress.XtraReports.UI;                 // XtraReport, Bands, XRSubreport, XRControl...
using DevExpress.XtraReports.UserDesigner;       // XRDesignMdiController, XRDesignPanel
using System;
using System.Collections.Generic;
using System.Data;

namespace Winforms_App_Template.Report
{
    /// <summary>
    /// Gắn schema DESIGN-TIME cho từng DetailReportBand (không gắn toàn report),
    /// Chỉ phục vụ cho lựa chọn expression trong design report, còn khi in ra phải binding vào dữ liệu thật
    /// </summary>
    public static class DesignSchema
    {
        /// <summary>
        /// Gắn DataTable schema cho MỘT band (design-time): Field List của band đó sẽ có đúng cột whitelist.
        /// </summary>
        public static void AttachDesignSchemaToBand(DetailReportBand band, DataTable schema)
        {
            if (band == null) throw new ArgumentNullException(nameof(band));     // band không được null
            if (schema == null) throw new ArgumentNullException(nameof(schema)); // schema không được null

            band.DataSource = schema;                // chỉ band này có Field List dựa theo DataTable
            band.DataMember = null;
        }

        /// <summary>
        /// Gắn schema cho các band theo map {BandName → DataTable schema}.
        /// Band nào không có trong map thì bỏ qua.
        /// </summary>
        public static void AttachBandSchemas(XtraReport rpt, IDictionary<string, DataTable> bandSchemas)
        {
            if (rpt == null) throw new ArgumentNullException(nameof(rpt));                 // report không null
            if (bandSchemas == null) throw new ArgumentNullException(nameof(bandSchemas)); // map không null

            // Duyệt tất cả DetailReportBand trong report
            foreach (var band in EnumerateDetailReportBands(rpt))
            {
                // Nếu tên band có trong map → gắn schema
                DataTable? schema;
                if (!string.IsNullOrEmpty(band.Name) && bandSchemas.TryGetValue(band.Name, out schema))
                {
                    AttachDesignSchemaToBand(band, schema);  // gắn theo band (design-time)
                }
            }
        }

        /// <summary>
        /// Khi người dùng mở tab subreport (tạo XRDesignPanel cho report con),
        /// ta gắn schema cho CHÍNH subreport đó, chọn schema dựa vào subreport (ví dụ theo tên band cha).
        /// </summary>
        public static void WireSubreportSchemaOnDemandByBand(
    XRDesignMdiController controller,    // MDI của Designer
    XtraReport mainReport,               // report chính
    Func<XRSubreport, DataTable> subSchemaFactory // chọn schema theo sub (theo band, theo tên, ...)
)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (mainReport == null) throw new ArgumentNullException(nameof(mainReport));
            if (subSchemaFactory == null) throw new ArgumentNullException(nameof(subSchemaFactory));

            // ❶ Chuẩn bị map UID một lần trước khi user mở bất kỳ tab nào
            var uid2Sub = BuildSubreportUidMap(mainReport);

            // ❷ Nghe khi một panel được tạo xong
            controller.DesignPanelLoaded += (sender, e) =>
            {
                // Lấy XRDesignPanel theo cách "an toàn" cho nhiều version DevExpress
                var panel = (XRDesignPanel)sender;                                           // fallback
                if (panel == null) return;

                var opened = panel.Report;                 // Report clone đang mở trong tab
                if (ReferenceEquals(opened, mainReport))
                    return;                                // Đây là tab report chính → bỏ qua (schema band đã gắn trước đó)

                // Đọc UID từ clone (đã được serialize)
                var p = opened.Parameters["p_DesignUID"];
                var uid = p?.Value as string ?? p?.Value?.ToString();
                if (string.IsNullOrWhiteSpace(uid)) return;   // Không có UID → không thể map

                // Map ngược: UID → XRSubreport trong cây main
                if (!uid2Sub.TryGetValue(uid, out var sub)) return;

                // Xin schema phù hợp cho sub này (theo band/ theo tên sub, ...)
                var schema = subSchemaFactory(sub);
                if (schema == null) return;

                // GẮN SCHEMA TRÊN BẢN CLONE (panel.Report), KHÔNG phải child cũ
                opened.DataSource = schema;
                opened.DataMember = null; // ← QUAN TRỌNG: để Field List hiện đúng
            };
        }


        /// <summary>
        /// Tìm band chứa một XRSubreport (để biết sub này thuộc band nào).
        /// </summary>
        public static DetailReportBand? FindOwningDetailReportBand(XtraReport rpt, XRSubreport sub)
        {
            if (rpt == null || sub == null) return null;           // Validation giá trị truyền vào
            // Duyệt toàn bộ band trong report
            foreach (var band in EnumerateDetailReportBands(rpt))
            {
                // Kiểm tra xem sub này có nằm trong cây controls của band không
                if (ContainsControlRecursive(band, sub))
                    return band;                                   // tìm được band cha
            }
            return null;                                           // không tìm thấy
        }

        /// <summary>
        /// Duyệt mọi DetailReportBand (kể cả lồng).
        /// </summary>
        public static IEnumerable<DetailReportBand> EnumerateDetailReportBands(XtraReport rpt)
        {
            foreach (Band b in rpt.Bands)
            {
                var dr = b as DetailReportBand;   // nếu band là DetailReportBand
                if (dr == null) continue;          // không phải thì bỏ qua

                yield return dr;                  // trả ra band này để thao tác

                // Nếu band có report con (LevelDown), duyệt đệ quy lần nữa cho chính nó
                var nested = GetNestedReport(dr);
                if (nested != null)
                {
                    foreach (var inner in EnumerateDetailReportBands(nested))
                        yield return inner;       // yield các band con
                }
            }
        }

        /// <summary>
        /// Duyệt tất cả XRSubreport (kể cả lồng).
        /// </summary>
        public static IEnumerable<XRSubreport> EnumerateAllSubreports(XtraReport rpt)
        {
            foreach (Band b in rpt.Bands)
            {
                if (b == null) continue;                          // Bỏ qua band nào là null
                // Duyệt tất cả control trong từng band
                foreach (XRControl c in EnumerateControls(b.Controls))
                {
                    var s = c as XRSubreport;                     // nếu control là subreport
                    if (s != null) yield return s;                // trả subreport

                    // Nếu subreport có report con → tiếp tục duyệt subreport lồng bên trong
                    var child = s?.ReportSource as XtraReport;
                    if (child != null)
                    {
                        foreach (var inner in EnumerateAllSubreports(child))
                            yield return inner;                   // yield subreport lồng
                    }
                }
            }
        }

        /// <summary>
        /// Tìm một DetailReportBand theo Name (tiện cho map).
        /// </summary>
        public static DetailReportBand? FindDetailReportBandByName(XtraReport rpt, string bandName)
        {
            if (rpt == null || string.IsNullOrWhiteSpace(bandName)) return null; // Validation đầu vào

            // Duyệt mọi DetailReportBand trong 1 report
            foreach (var band in EnumerateDetailReportBands(rpt))
                // So sánh tên band (có yêu cầu so sánh viết hoa hoặc thường, VD: catongtho_report khác Catongtho_Report)   
                if (string.Equals(band.Name, bandName, StringComparison.Ordinal))
                    return band;                                                 // trả band khớp tên
            return null;                                                         // không thấy
        }

        /// <summary>
        /// Liệt kê toàn bộ control trong một collection theo chiều sâu.
        /// </summary>
        private static IEnumerable<XRControl> EnumerateControls(XRControlCollection controls)
        {
            foreach (XRControl c in controls)
            {
                yield return c;                               // yield control hiện tại
                foreach (var inner in EnumerateControls(c.Controls))
                    yield return inner;                       // yield control con
            }
        }

        /// <summary>
        /// Kiểm tra một control (target) có nằm trong cây con của container (band/control) hay không.
        /// </summary>
        private static bool ContainsControlRecursive(XRControl container, XRControl target)
        {
            if (container == null || target == null) return false;      // an toàn
            if (ReferenceEquals(container, target)) return true;        // trùng chính nó

            foreach (XRControl child in container.Controls)
            {
                if (ReferenceEquals(child, target)) return true;        // con trực tiếp
                if (ContainsControlRecursive(child, target)) return true; // con sâu hơn
            }
            return false;                                               // không thuộc
        }

        /// <summary>
        /// Lấy report con (LevelDown) của một DetailReportBand (nếu có).
        /// </summary>
        private static XtraReport? GetNestedReport(DetailReportBand dr)
        {
            // Với XtraReports, DetailReportBand có thể chứa một DataMember khác hoặc
            // là "band container" – trong trường hợp report lồng cấp 2/3 ta thường đi qua XRSubreport.
            // Ở đây để đơn giản, ta không tự suy ra nested report; khi cần duyệt đệ quy,
            // ta nên duyệt qua EnumerateAllSubreports thay thế.
            return null;
        }

        // Tạo/đảm bảo Parameter p_DesignUID trong report con, trả về UID (string)
        private static string EnsureDesignUid(XtraReport report)
        {
            const string ParamName = "p_DesignUID";

            // Nếu đã có → dùng lại
            var p = report.Parameters[ParamName];
            if (p != null && p.Value is string s && !string.IsNullOrWhiteSpace(s))
                return s;

            // Chưa có → tạo mới
            p = new DevExpress.XtraReports.Parameters.Parameter
            {
                Name = ParamName,
                Type = typeof(string),
                Visible = false,
                Value = Guid.NewGuid().ToString("N")    // UID duy nhất
            };
            report.Parameters.Add(p);
            return (string)p.Value;
        }

        // Duyệt mọi XRSubreport trong main và build map: UID → XRSubreport
        private static Dictionary<string, XRSubreport> BuildSubreportUidMap(XtraReport mainReport)
        {
            var map = new Dictionary<string, XRSubreport>(StringComparer.Ordinal);
            foreach (var sub in DesignSchema.EnumerateAllSubreports(mainReport))
            {
                if (sub.ReportSource is XtraReport child)
                {
                    var uid = EnsureDesignUid(child);      // gắn/lấy UID ngay trên report con
                    map[uid] = sub;                        // lưu map: UID → sub (control)
                }
            }
            return map;
        }

    }
}
