using DevExpress.XtraReports.UI;                 // XtraReport, Bands, XRSubreport, XRControl...
using DevExpress.XtraReports.UserDesigner;       // XRDesignMdiController, XRDesignPanel
using System;
using System.Collections.Generic;
using DevExpress.XtraReports.Parameters;   // Parameter
using System.Data;

namespace Winforms_App_Template.Report
{

    /// <summary>
    /// Mô tả 1 parameter cần tạo: Name (logic), Type, Label (tên hiển thị), DefaultValue (tuỳ chọn)
    /// </summary>
    public sealed class ParameterSpec
    {
        public string Name;             // tên logic (ví dụ "Category_Code")
        public Type Type;               // typeof(string)/typeof(int)/...
        public string Label;            // nhãn hiển thị trong Field List (tuỳ chọn)
        public object DefaultValue;     // giá trị mặc định (tuỳ chọn)

        public ParameterSpec(string name, Type type, string? label = null, object defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Parameter name is required.", nameof(name));

            this.Name = name;
            this.Type = type ?? typeof(string);
            this.Label = label ?? name;
            this.DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// Gắn schema DESIGN-TIME cho từng DetailReportBand (không gắn toàn report),
    /// Chỉ phục vụ cho lựa chọn expression trong design report, còn khi in ra phải binding vào dữ liệu thật
    /// </summary>
    public static class DesignSchema
    {
        /// <summary>
        /// Gắn một parameter theo danh sách specs cho MỘT band cụ thể trong report.
        /// </summary>
        /// <param name="rpt"></param>
        /// <param name="bandName"></param>
        /// <param name="specs"></param>
        /// <param name="visible"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void EnsureParametersForBand(
    XtraReport rpt,                 // report chính
    string bandName,                // tên band (ví dụ "Catongtho_Report")
    IEnumerable<ParameterSpec> specs,
    bool visible = false            // để false: không bật dialog parameter mặc định
)
        {
            if (rpt == null)
                throw new ArgumentNullException(nameof(rpt));  // report không được null

            if (string.IsNullOrWhiteSpace(bandName))
                throw new ArgumentException("bandName required.", nameof(bandName));

            if (specs == null)
                return; // không có gì để tạo

            // 1. Tìm DetailReportBand theo tên bandName
            var band = DesignSchema.FindDetailReportBandByName(rpt, bandName);
            if (band == null)
                throw new InvalidOperationException($"Không tìm thấy DetailReportBand '{bandName}'.");

            // 2. Tìm ReportHeaderBand bên trong band này
            //    - DetailReportBand có collection Bands chứa các band con:
            //      DetailBand, GroupHeaderBand, GroupFooterBand, ReportHeaderBand, ...
            var headerBand = band.Bands
                                 .OfType<ReportHeaderBand>()
                                 .FirstOrDefault();

            // 3. Nếu KHÔNG có ReportHeaderBand → bỏ qua, không tạo parameter cho band này
            if (headerBand == null)
            {
                // Không ném exception, chỉ đơn giản là không tạo param cho band này
                return;
            }

            // 4. Kiểm tra tên của headerBand có chứa từ "header" hay không
            //    - Không phân biệt hoa/thường: dùng StringComparison.OrdinalIgnoreCase
            var headerName = headerBand.Name ?? string.Empty;

            // Nếu tên header KHÔNG chứa từ "header" → cũng bỏ qua, không tạo parameter
            if (!headerName.Contains("header", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // 5. Chỉ đến đây nếu:
            //    - Tồn tại ReportHeaderBand
            //    - Và tên của ReportHeaderBand có chứa "header"
            //    → Bắt đầu tạo parameters cho band này
            foreach (var spec in specs)
            {
                if (spec == null)
                    continue;

                // Tạo tên đầy đủ: p_{bandName}_{ParamName}
                // Ví dụ: bandName = "Catongtho_Report", spec.Name = "FromDate"
                // fullName = "p_Catongtho_Report_FromDate"
                var fullName = $"p_{bandName}_{spec.Name}";

                // Nếu đã có parameter trùng tên trên report → bỏ qua, không tạo lại
                var existing = rpt.Parameters[fullName];
                if (existing != null)
                    continue;

                // Tạo mới parameter
                var p = new Parameter
                {
                    Name = fullName,                  // tên param đầy đủ
                    Type = spec.Type,                 // kiểu .NET (typeof(int), typeof(DateTime), ...)
                    Description = string.IsNullOrWhiteSpace(spec.Label)
                                ? $"{bandName}.{spec.Name}"  // gợi ý: Band.ParamName
                                : spec.Label,               // nhãn hiển thị trong Field List
                    Visible = visible                  // thường để false để không hiện dialog mặc định
                };

                // Nếu có giá trị mặc định thì gán
                if (spec.DefaultValue != null)
                    p.Value = spec.DefaultValue;

                // Thêm parameter vào collection của report
                rpt.Parameters.Add(p);
            }
        }

        public static void Attach_allparameter_report(XtraReport rpt, IEnumerable<ParameterSpec> specs)
        {
            if (rpt == null) throw new ArgumentNullException(nameof(rpt));                 // report không null
            if (specs == null) return;                                           // không có gì để tạo

            // Duyệt tất cả các band có thể có trong report
            foreach (var band in EnumerateDetailReportBands(rpt))
            {
                // Gọi hàm tạo parameter cho từng band
                EnsureParametersForBand(rpt, band.Name ?? string.Empty, specs);
            }

        }

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
            XRDesignMdiController controller,    // MDI của Designer, mỗi tab là 1 XRDesignPanel
            XtraReport mainReport,               // report gốc chứa các XRsubreport
            Func<XRSubreport, DataTable> subSchemaFactory //nhận vào 1 hàm có tham số đầu vào là XRSubreport, trả về DataTable schema tương ứng
        )
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (mainReport == null) throw new ArgumentNullException(nameof(mainReport));
            if (subSchemaFactory == null) throw new ArgumentNullException(nameof(subSchemaFactory));

            // Chuẩn bị map UID một lần trước khi user mở bất kỳ tab nào
            var uid2Sub = BuildSubreportUidMap(mainReport);

            // Bắt sự kiện khi một panel được mở
            controller.DesignPanelLoaded += (sender, e) =>
            {
                // Sender là panel đang mở (theo tài liệu chính thức từ DevXpress)
                var panel = (XRDesignPanel)sender;                                           
                if (panel == null) return;   // Trong trường hợp ko ép kiểu được panel thì bỏ qua

                var opened = panel.Report;                 // Report clone đang mở trong tab (DevExpress tự clone)
                if (ReferenceEquals(opened, mainReport))   // Kiểm tra xem nếu panel đang mở là report chính thì return luôn
                    return;                                // Vì report chính ta đã gắn schema từ trước

                // Đọc UID từ clone (đã được serialize) và ép kiểu sang string
                var p = opened.Parameters["p_DesignUID"];
                var uid = p?.Value as string ?? p?.Value?.ToString();
                if (string.IsNullOrWhiteSpace(uid)) return;   // Không có UID → không thể biết clone này thuộc subreport nào trong main --> bỏ qua

                // Dùng uid của report clone để xác minh subreport gốc trong mainReport, nếu không tìm thấy thì bỏ qua
                // Nếu có thì ta có được XRSubreport gốc trong mainReport lưu vào biến sub
                if (!uid2Sub.TryGetValue(uid, out var sub)) return;

                // Sử dụng hàm subSchemaFactory để lấy schema tương ứng cho subreport này
                var schema = subSchemaFactory(sub);
                if (schema == null) return;

                // GẮN SCHEMA TRÊN BẢN CLONE (panel.Report), Không được gắn lên sub.ReportSource gốc, vì Designer làm việc với clone.
                opened.DataSource = schema;
                opened.DataMember = null; 
            };
        }

        public static void WireSubreportSchemaOnDemandBySubName(
            XRDesignMdiController controller,             // MDI designer
            XtraReport mainReport,                        // report chính
            IDictionary<string, DataTable> schemasByName, // danh sách schema cho từng subreport theo tên
            DataTable defaultSchema                       // schema mặc định nếu không khớp tên
        )
        {
            // Kiểm tra tham số để tránh null-reference lỗi runtime
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (mainReport == null) throw new ArgumentNullException(nameof(mainReport));
            if (schemasByName == null) throw new ArgumentNullException(nameof(schemasByName));
            if (defaultSchema == null) throw new ArgumentNullException(nameof(defaultSchema));

            //  Gọi lại hàm tổng quát WireSubreportSchemaOnDemandByBand và truyền vào một "chiến lược" chọn schema dựa theo tên subreport.
            WireSubreportSchemaOnDemandByBand(
            controller,   // truyền controller đang dùng
            mainReport,   // truyền report chính
            sub =>        // đây là Func<XRSubreport, DataTable> (lambda)
            {
                // Nếu vì lý do gì sub là null → trả về defaultSchema (để ko gây lỗi).
                if (sub == null)
                    return defaultSchema;

                // Lấy Name của XRSubreport trong layout (ví dụ: "SR_Standards_68")
                var subName = sub.Name;

                // Nếu tên hợp lệ và có trong dictionary schemasByName
                if (!string.IsNullOrWhiteSpace(subName) &&           // có tên
                    schemasByName.TryGetValue(subName, out var schemaFromMap) && // tra map
                    schemaFromMap != null)                           // có DataTable hợp lệ
                {
                    // Trả về schema tương ứng → Field List của tab subreport sẽ dùng schema này
                    return schemaFromMap;
                }

                // Ngược lại (không có trong map / null / tên rỗng):
                // Trả về schema mặc định để vẫn có Field List dùng được.
                return defaultSchema;
            });
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
        /// Duyệt tất cả XRSubreport (kể cả lồng nhau).
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

        /// <summary>
        /// Tạo 1 parameter p_DesignUID (Mã định danh duy nhất cho mỗi report con) giữ giá trị duy nhất cho mỗi XtraReport con (ReportSource của XRSubreport).
        /// Khi DevExpress clone report con ra, UID cũng được clone, đọc p_DesignUID → biết nó là bản sao của thằng nào trong main.
        /// Lưu ý: Design trong XRDesignMdiController sẽ làm việc với bản clone của report con, không phải report con gốc trong main.
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private static string EnsureDesignUid(XtraReport report)
        {
            // Tạo cố định tên parameter để lưu UID và sử dụng lại
            const string ParamName = "p_DesignUID";

            // Lấy giá trị UID thông qua Parameter
            var p = report.Parameters[ParamName];

            // Nếu report con đã có UID thì trả về giá trị đấy, ko tạo mới nữa tránh thay đổi
            if (p != null && p.Value is string s && !string.IsNullOrWhiteSpace(s))
                return s;

            // Nếu chưa có → tạo mới
            p = new DevExpress.XtraReports.Parameters.Parameter
            {
                Name = ParamName,
                Type = typeof(string),
                Visible = false,
                Value = Guid.NewGuid().ToString("N")    // Tạo 1 GUID, dạng 32 ký tự hexa không dấu gạch, gần như sẽ là duy nhất
            };

            // Thêm vào Parameters của report con
            report.Parameters.Add(p);
            return (string)p.Value;
        }

        // Duyệt mọi XRSubreport trong main và build map: UID → XRSubreport
        private static Dictionary<string, XRSubreport> BuildSubreportUidMap(XtraReport mainReport)
        {
            // Tạo dictionary để lưu map
            var map = new Dictionary<string, XRSubreport>(StringComparer.Ordinal); // StringComparer.Ordinal: so sánh string theo byte/ASCII, chính xác, phân biệt hoa/thường.

            // Duyệt toàn bộ XRSubreport trong mainReport
            foreach (var sub in EnumerateAllSubreports(mainReport))
            {
                // Chỉ quan tâm đến sub có ReportSource là XtraReport (report con đầy đủ)
                // Nếu ReportSource là dạng khác (vd string path, object custom) thì bỏ qua
                if (sub.ReportSource is XtraReport child)
                {
                    var uid = EnsureDesignUid(child);      // gắn/lấy UID trên report con
                    map[uid] = sub;                        // lưu map: UID này tương ứng sub trong mainReport (XRSubreport)
                }
            }
            return map;
        }

    }
}
