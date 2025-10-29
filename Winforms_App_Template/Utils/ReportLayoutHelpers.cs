using DevExpress.XtraReports.UI;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using Winforms_App_Template.Database.Model;


/// <summary>
/// Các thao tác với Report
/// </summary>
public static class ReportLayoutHelpers
{

    private static string? GetFieldFromTag(object? tag, int fieldSegmentIndex = 2, char delimiter = '|')
    {
        var s = tag?.ToString();
        if (string.IsNullOrWhiteSpace(s)) return null;
        var parts = s.Split(delimiter);
        return parts.Length > fieldSegmentIndex ? parts[fieldSegmentIndex].Trim() : null;
    }

    /// <summary>
    /// Tìm kiếm các subreport trong report chính
    /// </summary>
    /// <param name="rpt"></param>
    /// <returns></returns>
    public static IEnumerable<XRSubreport> EnumerateSubreports(XtraReport rpt)
    {
        if (rpt == null)
            yield break;

        // Duyệt tất cả các band trong report chính
        foreach (Band b in rpt.Bands)
        {
            // Nếu ko có band nào thì bỏ qua 
            if (b == null) continue;

            // Duyệt các control con
            foreach (XRControl c in EnumerateControls(b.Controls))
            {
                // Nếu control này là XRSubReport thì trả về để thao tác đã
                if (c is XRSubreport s)
                    yield return s;

                // Nếu XRSubport có report con nữa, duyệt sâu (nếu cần)
                if (c is XRSubreport hasChild && hasChild.ReportSource is XtraReport child)
                {
                    foreach (var nested in EnumerateSubreports(child))
                        yield return nested;
                }
            }
        }
    }

    // Thử đọc FieldName từ ExpressionBindings (BeforePrint, Text = "[Field]" hoặc biểu thức chứa [Field])
    private static string? GetFieldFromExpression(XRTableCell cell)
    {
        var eb = cell.ExpressionBindings?.FirstOrDefault(b =>
            string.Equals(b.EventName, "BeforePrint", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(b.PropertyName, "Text", StringComparison.OrdinalIgnoreCase));
        if (eb == null || string.IsNullOrWhiteSpace(eb.Expression)) return null;

        // Tìm token [Field] đầu tiên trong biểu thức
        var m = Regex.Match(eb.Expression, @"\[(?<f>[A-Za-z0-9_\.]+)\]");
        return m.Success ? m.Groups["f"].Value : null;
    }

    // Lấy FieldName cho 1 cell: ưu tiên Tag → Expression → null
    private static string? ResolveFieldName(XRTableCell valueCell, int fieldSegmentIndex, char delimiter)
    {
        return GetFieldFromTag(valueCell.Tag, fieldSegmentIndex, delimiter)
               ?? GetFieldFromExpression(valueCell);
    }

    // Lấy giá trị của field từ 1 "record" bất kỳ (POCO/DataRow/Dictionary)
    private static object? GetFieldValue(object record, string fieldName)
    {
        if (record is null) return null;

        switch (record)
        {
            case DataRow dr:
                return dr.Table.Columns.Contains(fieldName) ? dr[fieldName] : null;

            case DataRowView drv:
                return drv.Row?.Table.Columns.Contains(fieldName) == true ? drv.Row[fieldName] : null;

            case IDictionary<string, object?> dict:
                return dict.TryGetValue(fieldName, out var v) ? v : null;

            default:
                // POCO: dùng reflection (ignore case)
                var t = record.GetType();
                var pi = t.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                return pi?.GetValue(record);
        }
    }

    // Chuẩn hoá: đánh giá 1 giá trị "có dữ liệu"?
    private static bool HasData(object? v)
    {
        if (v == null || v == DBNull.Value) return false;

        switch (v)
        {
            case string s: return !string.IsNullOrWhiteSpace(s);
            case bool b: return b;
            case int i: return i != 0;
            case long l: return l != 0;
            case float f: return Math.Abs(f) > float.Epsilon;
            case double d: return Math.Abs(d) > double.Epsilon;
            case decimal m: return m != 0m;
            case DateTime dt: return dt > DateTime.MinValue.AddSeconds(1);
            default: return true;
        }
    }

    // Chuyển giá trị → chuỗi để đo text
    private static string ToDisplayString(object? v, string? dateFormat = null)
    {
        if (v == null || v == DBNull.Value) return string.Empty;
        if (v is DateTime dt)
            return string.IsNullOrWhiteSpace(dateFormat) ? dt.ToString("yyyy-MM-dd HH:mm") : dt.ToString(dateFormat);
        return Convert.ToString(v) ?? string.Empty;
    }

    // Đo chiều rộng pixel của text theo Font (dùng GDI TextRenderer → gần với hiển thị thực té)
    private static int MeasureTextPx(string text, Font font)
    {
        // TextRenderer luôn trả pixel theo DPI hệ thống (96dpi mặc định).
        if (string.IsNullOrEmpty(text)) text = " "; // đừng để rỗng => width=0
        var sz = TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        return Math.Max(1, sz.Width);
    }

    // ==========
    // 1) Hàm chính: Auto ẩn cột trống + tính width theo text lớn nhất & chia theo tỷ lệ
    // ==========
    /// <summary>
    /// Tự động:
    /// - Dò tất cả cột của XRTable (header row index=0, value row index=1).
    /// - Tìm FieldName cho từng cột (Tag hoặc Expression).
    /// - Quét toàn bộ datasource để biết: cột có dữ liệu không, và độ rộng text max (header vs value).
    /// - Ẩn cột không có dữ liệu. Cột còn lại chia WidthF theo tỷ lệ width text lớn nhất (có padding + min/max).
    /// </summary>
    public static void AutoHideAndResizeByMaxText(
        XRTable table,
        IEnumerable dataSource,                // dataset đã gán cho report/band
        int fieldSegmentIndex = 2,             // nếu dùng Tag "a|b|Field|..."
        char delimiter = '|',
        float minWidthF = 40f,                 // bề rộng tối thiểu cho cột hiển thị (report units)
        float maxWidthF = 0f,                  // 0 = không giới hạn; >0 = giới hạn upper bound
        int headerPadPx = 12,                // padding pixel cộng thêm cho header
        int cellPadPx = 18,                // padding pixel cộng thêm cho value
        string? dateFormat = null              // định dạng DateTime nếu có
    )
    {
        if (table == null) throw new ArgumentNullException(nameof(table));
        if (table.Rows.Count < 2) return; // cần 2 hàng: header + value

        var header = table.Rows[0];
        var value = table.Rows[1];
        int colCount = Math.Min(header.Cells.Count, value.Cells.Count);

        // 1) Thu thập mô tả cột: header text, fieldName, font
        var cols = new List<(XRTableCell headerCell, XRTableCell valueCell, string? fieldName, string headerText)>(colCount);
        for (int i = 0; i < colCount; i++)
        {
            var h = header.Cells[i];
            var v = value.Cells[i];

            var fieldName = ResolveFieldName(v, fieldSegmentIndex, delimiter); // ưu tiên Tag → Expression
            var headerText = h.Text ?? string.Empty;

            cols.Add((h, v, fieldName, headerText));
        }

        // 2) Duyệt toàn bộ dataset: tính max width pixel & check "has data" cho từng cột
        var maxPx = new int[colCount];      // độ rộng pixel lớn nhất (header/value)
        var hasAnyData = new bool[colCount];

        // Đo header trước (kể cả khi không có FieldName)
        for (int i = 0; i < colCount; i++)
        {
            var px = MeasureTextPx(cols[i].headerText, cols[i].headerCell.Font) + headerPadPx;
            maxPx[i] = Math.Max(maxPx[i], px);
        }

        // Duyệt value của từng record trong datasource
        foreach (var rec in dataSource)
        {
            for (int i = 0; i < colCount; i++)
            {
                var field = cols[i].fieldName;
                if (string.IsNullOrEmpty(field)) continue; // cột không bind field nào

                var raw = GetFieldValue(rec, field);
                if (HasData(raw)) hasAnyData[i] = true;

                var s = ToDisplayString(raw, dateFormat);
                var px = MeasureTextPx(s, cols[i].valueCell.Font) + cellPadPx;
                if (px > maxPx[i]) maxPx[i] = px;
            }
        }

        // 3) Ẩn/hiện cột theo dữ liệu & tính lại WidthF theo tỷ lệ
        //    → quy đổi maxPx (pixel) thành tỉ trọng, rồi nhân tổng WidthF của table.
        var visibleIndices = new List<int>();

        for (int i = 0; i < colCount; i++)
        {
            bool visible = hasAnyData[i]; // cột có data ở bất kỳ record nào?
            cols[i].headerCell.Visible = visible;
            cols[i].valueCell.Visible = visible;
            if (visible) visibleIndices.Add(i);
        }

        if (visibleIndices.Count == 0)
        {
            // Không cột nào có dữ liệu → ẩn cả bảng
            table.Visible = false;
            return;
        }
        table.Visible = true;

        // Nếu cột không có FieldName (thuần header) nhưng header có text → vẫn coi là có?
        // (tuỳ bạn: nếu muốn giữ cột header-only, thay hasAnyData[i] = true khi headerText != "")
        // Ở đây mình chỉ giữ cột có data thực sự.

        // Tổng pixel của cột visible (không để 0 → tránh chia 0)
        float sumPx = visibleIndices.Sum(i => Math.Max(1, maxPx[i]));
        if (sumPx <= 0) sumPx = visibleIndices.Count;

        // Tổng độ rộng bảng (report units)
        float totalW = table.WidthF;

        foreach (var i in visibleIndices)
        {
            var ratio = (float)Math.Max(1, maxPx[i]) / sumPx; // tỉ trọng theo px
            var w = totalW * ratio;                           // WidthF đề xuất theo tỉ trọng

            // Minh bạch: đảm bảo min/max
            if (w < minWidthF) w = minWidthF;
            if (maxWidthF > 0 && w > maxWidthF) w = maxWidthF;

            cols[i].headerCell.WidthF = w;
            cols[i].valueCell.WidthF = w;
        }

        // 4) Cột ẩn => WidthF=0
        foreach (var i in Enumerable.Range(0, colCount).Except(visibleIndices))
        {
            cols[i].headerCell.WidthF = 0f;
            cols[i].valueCell.WidthF = 0f;
        }

        // 5) (tuỳ chọn) căn giữa header/value sau khi co giãn
        // foreach (var i in visibleIndices) {
        //     cols[i].headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
        //     cols[i].valueCell.TextAlignment  = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
        // }
    }

    /// <summary>
    /// Tìm các Parameter của report, nếu tên Parameter trùng tên property của 'src' (hoặc trùng sau khi bỏ "p_")
    /// thì gán p.Value = giá trị property tương ứng.
    /// </summary>
    public static void ApplyParametersFromObject(XtraReport rpt, object src)
    {
        //  Kiểm tra đầu vào — tránh NullReferenceException
        if (rpt == null || src == null) return;

        //  Lấy kiểu của đối tượng nguồn (src)
        //  => dùng Reflection để duyệt danh sách các property public của nó
        var srcType = src.GetType();

        //  Tạo từ điển ánh xạ {tên property → PropertyInfo} để tra cứu nhanh (O(1)) thay vì duyệt vòng lặp mỗi lần
        // Dùng StringComparer.OrdinalIgnoreCase để không phân biệt hoa/thường
        var map = srcType.GetProperties().ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        // Duyệt toàn bộ các parameter trong report
        foreach (DevExpress.XtraReports.Parameters.Parameter p in rpt.Parameters)
        {
            // ⚠️ Tuỳ chọn: Nếu không muốn ghi đè giá trị người dùng nhập,
            // có thể bỏ qua các parameter có Visible = true (tức là hiển thị trong UI)
            // if (p.Visible) continue;

            // Thử khớp tên parameter với property 1:1
            if (!map.TryGetValue(p.Name, out var prop))
            {
                // Nếu không có, thử bỏ tiền tố "p_"
                // Ví dụ: Parameter "p_OKText" → so sánh với property "OKText"
                var name2 = p.Name.StartsWith("p_", StringComparison.OrdinalIgnoreCase) ? p.Name.Substring(2) : p.Name;

                // Nếu vẫn không khớp property nào → bỏ qua
                if (!map.TryGetValue(name2, out prop))
                    continue; 
            }
            // Lấy giá trị từ property tương ứng trong object nguồn
            var val = prop.GetValue(src, null);
            // Gán giá trị đó cho Parameter.Value
            // DevExpress sẽ tự động convert kiểu dữ liệu cơ bản (int → decimal, string → object, ...)
            p.Value = val; 
        }
    }

    /// <summary>
    /// Gắn dữ liệu runtime cho report chính (rows) và subreport (map idInput -> list Standards).
    /// </summary>
    public static void BindForRuntime(
        XtraReport rpt,
        IList<Catthoong_ReportRow> rows,
        IDictionary<int, List<Standard_Model>> stdMap,
        string idFieldName = "idInput")
    {
        // Gán null cho detail gốc, tránh lặp, đảm bảo không bị hiển thị lặp dữ liệu cũ (khi load layout từ DB).
        rpt.DataSource = null;   
        rpt.DataMember = null;

        // Gọi helper `BindDetailReport` để gắn datasource cho band trong 1 report
        SubreportWiring.BindDetailReport(rpt, "Catongtho_Report", rows);   // IEnumerable<Catthoong_ReportRow>
        //SubreportWiring.BindDetailReport(rpt, "DetailBand_Report2", listA); // IEnumerable<ModelA>
        //SubreportWiring.BindDetailReport(rpt, "DetailBand_Report3", listB); // IEnumerable<ModelB>

        // Band chứa subreport: "Catongtho_Report"
        // Subreport control name: "xrSubreport1"
        // Mục tiêu: khi mỗi dòng cha có `idInput = x`, ta lấy danh sách `Standard_Model`
        // tương ứng từ `stdMap[x]` và bơm vào report con.
        SubreportWiring.WireSubreportLookup<Standard_Model>(
            rpt,
            hostBandName: "Catongtho_Report",
            subreportName: "xrSubreport1",
            parentKeyField: "idInput",
            resolveChildren: key =>
            {
                // convert key to int một cách an toàn
                int id = 0;
                if (key != null) int.TryParse(key.ToString(), out id);

                // Nếu tồn tại key trong stdMap → trả về danh sách con (List<Standard_Model>)
                // Ngược lại → trả về danh sách rỗng (tránh null để không lỗi runtime)
                return (id != 0 && stdMap.TryGetValue(id, out var bucket) && bucket != null)
                    ? bucket
                    : Enumerable.Empty<Standard_Model>();
            });
    }

    /// <summary>
    /// Bơm giá trị header thực tế vào các parameter ?p_{PropName}
    /// Ví dụ: header.Name_Congdoan → ?p_Name_Congdoan
    /// </summary>
    public static void PushHeaderValues(XtraReport rpt, object header)
    {
        if (rpt == null) throw new ArgumentNullException(nameof(rpt));
        if (header == null) return;

        var t = header.GetType();
        foreach (var pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!pi.CanRead) continue;
            var param = rpt.Parameters["p_" + pi.Name];            // tên p_* đã tạo ở design-time
            if (param == null) continue;

            var val = pi.GetValue(header);
            param.Value = val ?? DBNull.Value;                     // gán giá trị thực tế để expression ?p_* hiển thị
        }
    }

    /// <summary>
    /// Quét tất cả ExpressionBindings trong report (bao gồm cả subreport),
    /// trích các token dạng [Field] và đối chiếu với property của modelType.
    /// Trả về danh sách các field không tồn tại trong model.
    /// </summary>
    public static HashSet<string> CollectInvalidFields(XtraReport rpt, Type modelType)
    {
        // Tạo 1 HashSet để chứa tên các field "không hợp lệ" (không tồn tại trong model).
        // Dùng StringComparer.OrdinalIgnoreCase để so sánh không phân biệt hoa thường.
        var invalid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Lấy danh sách property hợp lệ từ modelType (tên property).
        // Tạo HashSet để tra nhanh O(1) và cũng không phân biệt hoa thường.
        var props = new HashSet<string>(
            modelType.GetProperties().Select(p => p.Name),
            StringComparer.OrdinalIgnoreCase);

        // Định nghĩa regex để bắt các token trong biểu thức dạng [something].
        // (?<name>...) đặt tên cho nhóm (group) là "name".
        // Cho phép chữ A-Z a-z số 0-9 dấu gạch dưới _ và dấu chấm .
        // RegexOptions.Compiled compile regex để chạy nhanh hơn (đổi chi phí khi tạo).
        var rx = new Regex(@"\[(?<name>[A-Za-z0-9_\.]+)\]", RegexOptions.Compiled);

        // Duyệt qua mọi Band trong report (Band thường là Header/Detail/Footer,...).
        foreach (Band b in rpt.Bands)
        {
            // Duyệt tất cả control(bao gồm control con) trong band bằng helper EnumerateControls.
            foreach (XRControl c in EnumerateControls(b.Controls))
            {
                // Duyệt tất cả expression binding của control (mỗi binding có thuộc tính Expression).
                foreach (var eb in c.ExpressionBindings)
                {
                    // Nếu eb.Expression null thì dùng chuỗi rỗng (tránh NullReferenceException).
                    var expr = eb.Expression ?? string.Empty;

                    // Dùng regex để tìm mọi token [name] trong expression.
                    foreach (Match m in rx.Matches(expr))
                    {
                        // Lấy giá trị của group "name" (chuỗi giữa dấu ngoặc vuông). [name]
                        var token = m.Groups["name"]?.Value ?? "";
                        // Nếu chuỗi là rỗng thì bỏ qua
                        if (string.IsNullOrWhiteSpace(token))
                            continue;

                        // Bỏ qua các token không phải field dữ liệu:
                        // - Token bắt đầu "Parameters." (tham số)
                        // - Token chứa '.' (ví dụ [DataSource.CurrentRowIndex], [ReportItems.Text], …) → bỏ qua
                        // - Hàm, toán tử… (regex này không bắt)
                        if (token.StartsWith("Parameters.", StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Bỏ qua token chứa dấu chấm (ví dụ hệ thống hoặc nested như DataSource.CurrentRowIndex).
                        if (token.Contains('.'))
                            continue;

                        // Nếu token không tồn tại trong props thì thêm vào tập invalid
                        if (!props.Contains(token))
                            invalid.Add(token);
                    }
                }
            }
        }
        // Trả về tập tên field "không hợp lệ" (không trùng nhau do HashSet).
        return invalid;
    }

    public static IEnumerable<XRControl> EnumerateControls(XRControlCollection controls)
    {
        // Với mỗi control trong collection truyền vào
        foreach (XRControl c in controls)
        {
            // Trả về control hiện tại (yield return cho phép duyệt lazy/tiếp tục)
            yield return c;

            // Gọi đệ quy: nếu control có Controls (control lồng control), duyệt tiếp các control con
            foreach (var inner in EnumerateControls(c.Controls))
                yield return inner;
        }
    }

}
