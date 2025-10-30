using DevExpress.XtraReports.Parameters;   // Parameter
using DevExpress.XtraReports.UI;           // XtraReport, Bands, XRControl
using System;
using System.Collections.Generic;
using System.Linq;

namespace Winforms_App_Template.Report
{
    /// <summary>
    /// Mô tả 1 parameter cần tạo: Name (logic), Type, Label (tên hiển thị), DefaultValue (tuỳ chọn)
    /// </summary>
    public sealed class ParameterSpec
    {
        public string Name;             // tên logic (ví dụ "Category_Code")
        public Type Type;             // typeof(string)/typeof(int)/...
        public string Label;            // nhãn hiển thị trong Field List (tuỳ chọn)
        public object DefaultValue;     // giá trị mặc định (tuỳ chọn)

        public ParameterSpec(string name, Type type, string label = null, object defaultValue = null)
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
    /// Helper tạo parameter theo band:
    /// - Thực tế Parameter trong DevExpress là cấp REPORT → ta đặt quy ước tên:
    ///   p_{BandName}_{ParamName} (ví dụ: p_Catongtho_Report_Category_Code)
    /// - Người thiết kế kéo thả ?p_... vào band tương ứng.
    /// </summary>
    public static class BandParameterHelper
    {
        /// <summary>
        /// Tạo danh sách Parameter ở cấp REPORT nhưng “namespaced” theo band:
        ///   p_{bandName}_{spec.Name}
        /// - Nếu đã có thì không tạo lại.
        /// </summary>
        public static void EnsureParametersForBand(
            XtraReport rpt,                 // report chính
            string bandName,                // tên band (ví dụ "Catongtho_Report")
            IEnumerable<ParameterSpec> specs,
            bool visible = false            // để false: không bật dialog parameter mặc định
        )
        {
            if (rpt == null) throw new ArgumentNullException(nameof(rpt));       // report không null
            if (string.IsNullOrWhiteSpace(bandName)) throw new ArgumentException("bandName required.", nameof(bandName));
            if (specs == null) return;                                           // không có gì để tạo

            foreach (var spec in specs)
            {
                // Tạo tên đầy đủ: p_{ParamName}
                var full = $"p_{spec.Name}";

                // Nếu đã có parameter trùng tên → bỏ qua
                var existing = rpt.Parameters[full];
                if (existing != null) continue;

                // Tạo mới
                var p = new Parameter
                {
                    Name = full,                                             // tên param
                    Type = spec.Type,                                        // kiểu .NET
                    Description = spec.Label,                                   // nhãn gợi ý (hiện ở Field List)
                    Visible = visible                                           // thường để false
                };

                // Giá trị mặc định (nếu có)
                if (spec.DefaultValue != null)
                    p.Value = spec.DefaultValue;

                // Thêm vào report
                rpt.Parameters.Add(p);
            }
        }

        /// <summary>
        /// Bind Text của 1 control trong BAND sang 1 parameter theo quy ước p_{band}_{param}.
        /// - Tự tìm band theo tên
        /// - Tự tìm control theo Name trong band
        /// - Gắn ExpressionBindings: BeforePrint/Text = ?p_{band}_{param}
        /// </summary>
        public static void BindControlTextToBandParameter(
            XtraReport rpt,            // report
            string bandName,           // ví dụ "Catongtho_Report"
            string controlName,        // ví dụ "lblCategory"
            string paramLogicalName    // ví dụ "Category_Code"  (sẽ thành p_{bandName}_Category_Code)
        )
        {
            if (rpt == null) throw new ArgumentNullException(nameof(rpt));                 // an toàn

            // Tìm band theo tên
            var band = DesignSchema.FindDetailReportBandByName(rpt, bandName);
            if (band == null) throw new InvalidOperationException($"Không tìm thấy DetailReportBand '{bandName}'.");

            // Tìm control theo Name trong cây control của band
            var ctrl = FindControlRecursive(band, controlName);
            if (ctrl == null) throw new InvalidOperationException($"Không tìm thấy control '{controlName}' trong band '{bandName}'.");

            // Tạo expression "?p_{bandName}_{paramLogicalName}"
            var full = $"?p_{bandName}_{paramLogicalName}";

            // Gỡ binding cũ (nếu có) để tránh chồng lấp
            ctrl.ExpressionBindings.Clear();

            // Gắn binding chuẩn: BeforePrint / Text
            ctrl.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", full));
        }

        /// <summary>
        /// Tìm control theo Name từ một container (band/control) theo chiều sâu.
        /// </summary>
        private static XRControl FindControlRecursive(XRControl container, string name)
        {
            if (container == null || string.IsNullOrWhiteSpace(name)) return null; // an toàn
            foreach (XRControl c in container.Controls)
            {
                if (string.Equals(c.Name, name, StringComparison.Ordinal)) return c; // trùng tên
                var found = FindControlRecursive(c, name);                           // tìm sâu hơn
                if (found != null) return found;                                     // thấy thì trả
            }
            return null;                                                             // không thấy
        }
    }
}
