using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms; // để dùng TextRenderer.MeasureText
using System.Drawing;

public static class ReportLayoutHelpers
{
    // ==========
    // 0) Utils
    // ==========
    private static string? GetFieldFromTag(object? tag, int fieldSegmentIndex = 2, char delimiter = '|')
    {
        var s = tag?.ToString();
        if (string.IsNullOrWhiteSpace(s)) return null;
        var parts = s.Split(delimiter);
        return parts.Length > fieldSegmentIndex ? parts[fieldSegmentIndex].Trim() : null;
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

    // Duyệt tất cả XRSubreport trong một report
    public static IEnumerable<XRSubreport> EnumerateSubreports(XtraReport rpt)
    {
        foreach (Band b in rpt.Bands)
        {
            foreach (XRControl c in EnumerateControls(b))
                if (c is XRSubreport sr) yield return sr;
        }

        static IEnumerable<XRControl> EnumerateControls(XRControl parent)
        {
            foreach (XRControl c in parent.Controls)
            {
                yield return c;
                foreach (var child in EnumerateControls(c))
                    yield return child;
            }
        }
    }

}
