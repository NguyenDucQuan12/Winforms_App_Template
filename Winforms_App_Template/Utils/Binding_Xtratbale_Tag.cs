using DevExpress.XtraReports.UI;
using System;
using DevExpress.XtraReports.Parameters;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Utils
{
    public class Binding_Xtratbale_Tag
    {
        /// <summary>
        /// Quét tất cả cell trong 1 XRTable, cell nào có meta dạng a|b|c|d|e
        /// thì lấy c làm tên field và bind: Text = [c].
        /// Mặc định đọc meta từ Name. Nếu set readFromTag = true, sẽ đọc từ Tag.
        /// </summary>
        public static void AutoBindCellsByDelimitedMeta(
            XRTable table,              // bảng cần quét
            bool readFromTag,           // true: lấy meta từ Tag; false: lấy từ Name
            char delimiter = '|',       // ký tự phân tách, mặc định '|'
            int fieldSegmentIndex = 2,  // vị trí c trong a|b|c|d|e (bắt đầu từ 0) → 2
            int requiredSegmentCount = 5,   // yêu cầu đủ 5 đoạn (a,b,c,d,e)
            bool strict = false          // true: thiếu/không hợp lệ → ném lỗi; false: bỏ qua, chỉ cảnh báo
        )
        {
            // Validation đầu vào
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table), "XRTable không được null.");
            }
            if (fieldSegmentIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldSegmentIndex), "fieldSegmentIndex phải >= 0.");
            }
            if (requiredSegmentCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredSegmentCount), "requiredSegmentCount phải >= 1.");
            }

            // Duyệt từng hàng của bảng
            foreach (XRTableRow row in table.Rows)
            {
                // Duyệt từng cell trong 1 hàng để bắt đầu đọc meta tương ứng
                foreach (XRTableCell cell in row.Cells)
                {
                    // Lấy chuỗi "meta" từ Name hoặc Tag
                    string? meta = null;
                    if (readFromTag)
                    {
                        meta = cell.Tag == null ? null : Convert.ToString(cell.Tag);
                    }
                    else
                    {
                        meta = cell.Name; // chú ý: Name có thể không chứa '|', tuỳ Designer
                    }

                    // 3.2) Không có meta thì bỏ qua
                    if (string.IsNullOrEmpty(meta))
                    {
                        continue;
                    }

                    // 3.3) Tách các phần theo delimiter
                    string[] parts = meta.Split(new char[] { delimiter }, StringSplitOptions.None);

                    // 3.4) Kiểm tra cấu trúc: cần đúng (hoặc tối thiểu) số phần
                    if (parts.Length < requiredSegmentCount)
                    {
                        if (strict)
                        {
                            throw new InvalidOperationException(
                                $"Cell '{cell.Name}' meta='{meta}' không hợp lệ (ít hơn {requiredSegmentCount} phần).");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"WARN: Bỏ qua cell '{cell.Name}' do meta='{meta}' không đủ phần.");
                            continue;
                        }
                    }

                    // 3.5) Lấy phần c (tên field) tại vị trí fieldSegmentIndex
                    string? fieldName = parts[fieldSegmentIndex] == null ? null : parts[fieldSegmentIndex].Trim();

                    // 3.6) Kiểm tra tên field
                    if (string.IsNullOrEmpty(fieldName))
                    {
                        if (strict)
                        {
                            throw new InvalidOperationException(
                                $"Cell '{cell.Name}' meta='{meta}' không có tên field ở vị trí {fieldSegmentIndex}.");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"WARN: Bỏ qua cell '{cell.Name}' do field rỗng (meta='{meta}').");
                            continue;
                        }
                    }

                    // 3.7) Xây expression cơ bản: Text = [fieldName]
                    string expression = "[" + fieldName + "]";

                    // 3.8) Gán binding (xoá cũ để tránh chồng chéo)
                    cell.ExpressionBindings.Clear();
                    cell.ExpressionBindings.Add(
                        new ExpressionBinding("BeforePrint", "Text", expression)
                    );

                    // 3.9) Tuỳ chọn: nếu cell có thể multiline (vd. field chứa "\n")
                    // cell.Multiline = true; cell.CanGrow = true;
                }
            }
        }

        public static Parameter CreateOrUpdateParameter(XtraReport report, string name, object value)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Parameter name required", nameof(name));

            // Try indexer first (Parameters[name]) — nếu null, thêm mới
            Parameter? p = report.Parameters[name];
            if (p == null)
            {
                p = new DevExpress.XtraReports.Parameters.Parameter();
                p.Name = name;
                p.Visible = false;    // không bật dialog hỏi tham số
                p.Value = value;
                report.Parameters.Add(p);
            }
            else
            {
                p.Value = value;
            }
            return p;
        }


        /// <summary>
        /// Hiển thị tất cả expressionbinding của 1 bảng
        /// </summary>
        /// <param name="table">Biến chứa bảng cần theo dõi</param>
        /// <param name="tableName"> Tên bảng</param>
        public static void DumpBindings(XRTable table, string tableName)
        {
            foreach (XRTableRow r in table.Rows)
            {
                foreach (XRTableCell c in r.Cells)
                {
                    foreach (var eb in c.ExpressionBindings)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[BIND] {tableName}.{c.Name}: {eb.EventName}.{eb.PropertyName} = {eb.Expression}");
                    }
                }
            }
        }

    }
}
