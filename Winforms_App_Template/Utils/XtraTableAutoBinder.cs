using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Winforms_App_Template.Utils
{
    public static class XtraTableAutoBinder
    {
        /// <summary>
        /// TỰ ĐỘNG bind cho TẤT CẢ XRTable bên trong một DetailReportBand
        /// - Duyệt mọi band con (ReportHeader/Detail/GroupHeader/GroupFooter/ReportFooter...) của DR.
        /// - Với mỗi XRTable: duyệt mọi cell, đọc meta theo dạng a|b|c|d|e từ Tag (hoặc Name).
        /// - Lấy 'c' làm field name và bind Expression: Text = [c].
        /// 
        /// Lưu ý:
        /// - DataSource phải được gán ở cấp DetailReportBand (dr.DataSource = rows).
        /// - Hàm này hợp cho các bảng/ô cần bind từ dataset của band (thường là trong Detail/Footers).
        /// - Với bảng ở Header có nguồn khác dataset → dùng Parameter thay vì [Field].
        /// </summary>
        public static void AutoBindAllTablesInDetailReportByTag(
            DetailReportBand dr,              // band con có DataSource riêng
            bool readFromTag = true,          // true: đọc meta từ Tag; false: đọc từ Name
            char delimiter = '|',             // ký tự ngăn cách trong a|b|c|d|e
            int fieldSegmentIndex = 2,        // vị trí 'c' trong a|b|c|d|e → 2
            int minSegmentCount = 3,          // cho phép tối thiểu 3 phần (a|b|c)
            bool strictMeta = false,          // true: meta sai cấu trúc → ném lỗi; false: cảnh báo & bỏ qua để tiếp tục phần còn lại
            bool checkFieldExists = false     // true: kiểm tra field 'c' có tồn tại trong kiểu phần tử rows không (nếu xác định được)
        )
        {
            // Kiểm tra xem detail report có được truyền vào không
            if (dr == null)
                throw new ArgumentNullException(nameof(dr), "DetailReportBand không được null.");

            // 2) Gợi ý: assert DataSource để debug
            AssertBandDataSource(dr);

            // Duyệt tất cả các band con (ReportHeader/Detail/GroupHeader/GroupFooter/ReportFooter...) của DR
            foreach (Band childBand in dr.Bands)
            {
                if (childBand == null) continue;

                // Lấy tất cả XRTable trong band (kể cả nằm trong panel/nested control)
                var tables = FindAllTablesRecursive(childBand);
                 
                // Sau đó duyệt từng bảng trong danh sách
                foreach (var table in tables)
                {
                    // Bind dữ liệu từng bảng theo Tag hoặc Name cho từng cell thỏa mãn cấu trúc a|b|c|d|e
                    AutoBindSingleTableByTag(
                        table: table,
                        readFromTag: readFromTag,
                        delimiter: delimiter,
                        fieldSegmentIndex: fieldSegmentIndex,
                        minSegmentCount: minSegmentCount,
                        strictMeta: strictMeta,
                        checkFieldExists: checkFieldExists,
                        dataSourceForCheck: dr.DataSource    // dùng để check field tồn tại (nếu bật)
                    );
                }
            }
        }

        /// <summary>
        /// TỰ ĐỘNG bind cho MỘT XRTable:
        /// - Duyệt mọi cell của bảng.
        /// - Đọc meta theo a|b|c|d|e từ Tag/Name, lấy 'c' làm field name.
        /// - Gán ExpressionBinding: BeforePrint, Text = [c].
        /// - Kiểm tra field có tồn tại trong kiểu phần tử của DataSource không (nếu biết).
        /// </summary>
        /// <summary>
        /// TỰ ĐỘNG bind cho MỘT XRTable:
        /// - Duyệt mọi cell của bảng.
        /// - Đọc meta theo a|b|c|d|e từ Tag/Name, lấy 'c' làm field name.
        /// - Gán ExpressionBinding: BeforePrint, Text = [c].
        /// - Tuỳ chọn: kiểm tra field có tồn tại trong kiểu phần tử của DataSource không (nếu biết).
        /// </summary>
        public static void AutoBindSingleTableByTag(
            XRTable table,                     // bảng cần bind
            bool readFromTag = true,           // True: đọc meta từ Tag; false: đọc từ Name
            char delimiter = '|',              // Ký hiệu phân tách giữa các trường
            int fieldSegmentIndex = 2,         // Vị trí lấy trường làm FieldName
            int minSegmentCount = 3,           // Số lượng trường tối thiểu phải có trong tag hoặc name: a|b|c
            bool strictMeta = false,           // Fasle: Gặp lỗi hoặc cell ko đúng định dạng thì bỏ qua, tiếp tục các cell khác
            bool checkFieldExists = false,     // True: kiểm tra field 'c' có tồn tại trong kiểu phần tử rows không
            object? dataSourceForCheck = null  // nếu bật check, truyền dr.DataSource để xác minh property tồn tại
        )
        {
            // Kiểm tra đầu vào
            if (table == null)
                throw new ArgumentNullException(nameof(table), "XRTable không được null.");
            if (fieldSegmentIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(fieldSegmentIndex), "fieldSegmentIndex phải >= 0.");
            if (minSegmentCount < 1)
                throw new ArgumentOutOfRangeException(nameof(minSegmentCount), "minSegmentCount phải >= 1.");

            // Xác định kiểu phần tử của DataSource (Dành cho kiểm tra field tồn tại)
            HashSet<string>? fieldSetIgnoreCase = null; // chứa danh sách property name (ignore case)
            if (checkFieldExists && dataSourceForCheck != null)
            {
                // Lấy model được gán vào datasource: ví dụ datasource = new List<Catthoong_Model> thì lấy ra đối tượng Catthoong_Model
                var elementType = ProbeEnumerableElementType(dataSourceForCheck);
                if (elementType != null)
                {
                    // Tạo danh sách các đối tượng lưu trữ
                    fieldSetIgnoreCase = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    // Danh sách các trường trong đối tượng elemmenttype
                    var props = elementType.GetProperties();
                    // Duyệt qua từng đối tượng trong danh sách
                    foreach (var pi in props)
                    {
                        // Nếu có thể đọc được thì thêm vào danh sách (có nghĩa là đối tượng có thuộc tính khai báo public, không phải private)
                        if (pi.CanRead)
                            fieldSetIgnoreCase.Add(pi.Name);
                    }
                }
            }

            // Duyệt từng hàng & cell trong bảng
            foreach (XRTableRow row in table.Rows)
            {
                foreach (XRTableCell cell in row.Cells)
                {
                    // Lấy meta từ Tag hoặc Name
                    string? meta = null;
                    if (readFromTag)
                    {
                        meta = cell.Tag == null ? null : Convert.ToString(cell.Tag);
                    }
                    else
                    {
                        meta = cell.Name; // lưu ý: Designer không cho '|' trong Name → nên ưu tiên Tag hoặc thay đổi cấu trúc phân tách
                    }

                    // Không có meta thì bỏ qua cell này
                    if (string.IsNullOrWhiteSpace(meta))
                        continue;

                    // Tách meta theo delimiter
                    string[] parts = meta.Split(new char[] { delimiter }, StringSplitOptions.None);

                    // Kiểm tra số phần tách ra (ít nhất minSegmentCount, là 3 phần a|b|c thì mới chứa c)
                    if (parts.Length < minSegmentCount)
                    {
                        if (strictMeta)
                            throw new InvalidOperationException($"XRTable '{table.Name}' - Cell '{cell.Name}' meta='{meta}' không hợp lệ (ít hơn {minSegmentCount} phần).");

                        // Hiển thị ra Log
                        LogEx.Warning($"Cấu trúc sau khi tách meta không thỏa mãn, bỏ qua cell '{cell.Name}' meta='{meta}'.");
                        // Hiển thị ra Consolog ở tab Debug
                        System.Diagnostics.Debug.WriteLine($"Bỏ qua cell '{cell.Name}' meta='{meta}' (ít hơn {minSegmentCount} phần).");
                        continue;
                    }

                    // Lấy phần 'c' = tên field tại vị trí fieldSegmentIndex và loại bỏ các khoảng trắng xung quanh
                    string? fieldName = parts[fieldSegmentIndex]?.Trim();
                    // Kiểm tra xem trường này có kí tự không hay là chuỗi rỗng
                    if (string.IsNullOrEmpty(fieldName))
                    {
                        if (strictMeta)
                        {
                            LogEx.Error($"Không có field ở vị trí {fieldSegmentIndex}, bỏ qua cell '{cell.Name}' meta='{meta}'.");
                            throw new InvalidOperationException($"XRTable '{table.Name}' - Cell '{cell.Name}' meta='{meta}' không có field ở vị trí {fieldSegmentIndex}.");
                        }    
                            
                        // Ghi log
                        LogEx.Warning($"Không tìm thấy fieldname tương ứng, bỏ qua cell '{cell.Name}' meta='{meta}'.");
                        System.Diagnostics.Debug.WriteLine($"WARN meta: Bỏ qua cell '{cell.Name}' meta='{meta}' (field rỗng).");
                        continue;
                    }

                    // Kiểm tra field có tồn tại trong kiểu phần tử DataSource không
                    if (fieldSetIgnoreCase != null && fieldSetIgnoreCase.Count > 0)
                    {
                        // Nếu FieldName được tìm thấy trong cell mà không có FieldName nào phù hợp từ Model thì thông báo lỗi
                        if (!fieldSetIgnoreCase.Contains(fieldName))
                        {
                            if (strictMeta)
                            {
                                LogEx.Error($"Field '{fieldName}' không tồn tại trong kiểu phần tử của DataSource (XRTable '{table.Name}' - Cell '{cell.Name}').");
                                throw new InvalidOperationException($"Field '{fieldName}' không tồn tại trong kiểu phần tử của DataSource (XRTable '{table.Name}' - Cell '{cell.Name}').");
                            }

                            // Ghi log
                            LogEx.Warning($"Field '{fieldName}' không tồn tại trong kiểu phần tử của DataSource (XRTable '{table.Name}' - Cell '{cell.Name}').");
                            System.Diagnostics.Debug.WriteLine($"Field '{fieldName}' không tồn tại trong kiểu phần tử của DataSource (XRTable '{table.Name}' - Cell '{cell.Name}').");
                        }
                    }

                    // Lấy transform keyword (phần thứ 4: index 3) để xác định có đổi kiểu dữ liệu từ True/False thành OK/NG ko
                    string? transform = (parts.Length > 3)
                        ? parts[3]?.Trim()
                        : null;

                    // Nếu transform == "OK" (không phân biệt hoa thường) → bind theo OK/NG
                    if (!string.IsNullOrEmpty(transform) &&
                        string.Equals(transform, "OK", StringComparison.OrdinalIgnoreCase))
                    {
                        BindOkNgTextAndNgBackground(cell, fieldName);
                    }
                    else
                    {
                        // Gán binding: BeforePrint, Text = [FieldName]
                        cell.ExpressionBindings.Clear(); // xoá binding cũ để tránh chồng
                        // Binding mới được khuyên trong XtraReport, trước khi gọi hàm Print thì gán expression vào trường Text của cell này
                        cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", $"[{fieldName}]"));
                    };

                    // nếu muốn auto multiline
                    // cell.Multiline = true;
                    // cell.CanGrow = true;
                }
            }
        }

        // Xây biểu thức OK/NG theo field
        //    - Dùng In(Lower([field]), 'true','1','y','yes') → OK
        //    - Rỗng/null → in rỗng
        //    - Còn lại → NG
        // =========================
        private static string BuildOkNgExpression(string fieldName, string okParamName = "p_OKText", string ngParamName = "p_NGText")
        {
            // Ép về chuỗi: ToStr([field])
            // Sau đó Lower(...) + In(...)
            var f = $"Lower(ToStr([{fieldName}]))";
            return
                $"Iif(IsNullOrEmpty(ToStr([{fieldName}])), '', " +
                $"Iif({f} == 'true' Or {f} == '1' Or {f} == 'y' Or {f} == 'yes', Parameters.{okParamName}, Parameters.{ngParamName}))";

        }

        // Điều kiện “truthy” sau khi ép về chuỗi và lower-case
        private static string BuildTruthyCondition(string fieldName)
        {
            var f = $"Lower(ToStr([{fieldName}]))";
            return $"{f} == 'true' Or {f} == '1' Or {f} == 'y' Or {f} == 'yes'";
        }

        // Biểu thức BackColor: NG thì tô nền, còn lại trong suốt
        // Argb(a,r,g,b): a=255 là đậm, a=0 là trong suốt.
        // Ví dụ tô MistyRose (#FFEFE5E5 ~ 255,239,229,229)
        private static string BuildNgBackColorExpression(string fieldName)
        {
            var hasVal = $"Not IsNullOrEmpty(ToStr([{fieldName}]))";
            var truthy = BuildTruthyCondition(fieldName);
            var isNg = $"{hasVal} And Not ({truthy})";

            // NG → MistyRose; khác → Transparent
            return $"Iif({isNg}, Argb(255,239,229,229), Argb(0,0,0,0))";
        }

        // =========================
        // Bind theo OK/NG (transform)
        // =========================
        private static void BindOkNg(XRTableCell cell, string fieldName, string okParamName = "p_OKText", string ngParamName = "p_NGText")
        {
            cell.ExpressionBindings.Clear();
            cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", BuildOkNgExpression(fieldName, okParamName, ngParamName)));
        }

        /// <summary>
        /// Tạo binding vừa bind dữ liệu vừa bind màu nền
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="fieldName"></param>
        private static void BindOkNgTextAndNgBackground(XRTableCell cell, string fieldName)
        {
            cell.ExpressionBindings.Clear();
            cell.ExpressionBindings.AddRange(new[]
            {
                // 1) Bind Text → OK/NG
                new ExpressionBinding("BeforePrint", "Text",      BuildOkNgExpression(fieldName)),

                // 2) Bind BackColor → tô nền khi NG
                new ExpressionBinding("BeforePrint", "BackColor", BuildNgBackColorExpression(fieldName)),
            });

            // (tuỳ chọn) chữ đậm khi NG:
            // cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Font.Bold", 
            //     $"Iif(Not IsNullOrEmpty(ToStr([{fieldName}])) And Not ({BuildTruthyCondition(fieldName)}), True, False)"));
        }

        /// <summary>
        /// Tìm tất cả XRTable trong một band (đệ quy trong cây control).
        /// </summary>
        private static List<XRTable> FindAllTablesRecursive(Band band)
        {
            // Tạo 1 list chứa danh sách các XRtable tìm được
            var list = new List<XRTable>();
            if (band == null) return list;

            // Duyệt danh sách các control trong band
            foreach (XRControl ctrl in band.Controls)
            {
                // Tìm kiếm các control là Xrtable và thêm vào danh sách
                CollectTables(ctrl, list);
            }
            return list;
        }

        /// <summary>
        /// Đệ quy đi qua mọi control con để gom XRTable.
        /// </summary>
        private static void CollectTables(XRControl ctrl, List<XRTable> acc)
        {
            if (ctrl == null) return;

            // Ép kiểu cho control truyền vào là XRtable xem được không
            var asTable = ctrl as XRTable;
            if (asTable != null)
                acc.Add(asTable);  // Nếu là XRtable thì thêm vào danh sách

            // Nếu là container (XRPanel, XRTableCell có Controls, StackPanel...), vì những control này chứa các control khác
            if (ctrl.Controls != null && ctrl.Controls.Count > 0)
            {
                foreach (XRControl child in ctrl.Controls)
                {
                    // Đệ quy duyệt tìm XRtable
                    CollectTables(child, acc);
                }
            }
        }

        /// <summary>
        /// In ra log về DataSource của DetailReportBand (để chắc chắn band đang có dữ liệu).
        /// </summary>
        public static void AssertBandDataSource(DetailReportBand dr)
        {
            if (dr.DataSource == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ASSERT] '{dr.Name}' chưa có DataSource.");
                return;
            }
            // Lấy ra danh sách các bản ghi trong datasource
            var ie = dr.DataSource as IEnumerable;
            int count = 0;
            if (ie != null)
            {   
                // Đếm xem có tổng bao nhiêu bản ghi trong datasource
                foreach (var _ in ie) { count++; if (count > 100000) break; }
            }

            // Ghi log
            LogEx.Information($"DetaiReport {dr.Name} có Datasource kiểu {dr.DataSource.GetType().Name}, với số lượng bản ghi {count}.");
            System.Diagnostics.Debug.WriteLine($"[ASSERT] '{dr.Name}' DataSource={dr.DataSource.GetType().Name}, approx count={count}");
        }

        /// <summary>
        /// Cố gắng xác định kiểu phần tử T của IEnumerable/Tập hợp để kiểm tra property có tồn tại không.
        /// </summary>
        private static Type? ProbeEnumerableElementType(object dataSource)
        {
            if (dataSource == null) return null;

            // Nếu là IEnumerable<object> / List<T> / array...
            var type = dataSource.GetType();

            // Trường hợp là IEnumerable<Something>
            var ienumT = type.GetInterfaces()
                             .Concat(new[] { type })
                             .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (ienumT != null)
            {
                return ienumT.GetGenericArguments()[0]; // trả về T
            }

            // Không xác định được
            return null;
        }

        /// <summary>
        /// Hàm dump để kiểm tra xem sau khi bind xong đã có ExpressionBindings chưa
        /// </summary>
        /// <param name="table"></param>
        /// <param name="tableName"></param>
        public static void DumpBindings(XRTable table, string? tableName = null)
        {
            // Lấy tên bảng
            string tn = tableName ?? table?.Name ?? "(unknown table)";
            if (table == null)
            {
                Console.WriteLine("Không có binding trong bảng");
                return;
            }
            // Duyệt qua từng hàng trong bảng, từng cell trong 1 hàng
            foreach (XRTableRow r in table.Rows)
            {
                foreach (XRTableCell c in r.Cells)
                {
                    // Sau đó duyệt từng event binding trong cell đó (nếu tồn tại thì count >0)
                    foreach (var eb in c.ExpressionBindings)
                    {
                        // Ghi ra log
                        LogEx.Information($"Cell {tn}.{c.Name} có sự kiện binding: {eb.EventName}.{eb.PropertyName} = {eb.Expression}.");
                        System.Diagnostics.Debug.WriteLine($"[BIND] {tn}.{c.Name} -> {eb.EventName}.{eb.PropertyName} = {eb.Expression}");
                    }
                }
            }
        }
    }
}
