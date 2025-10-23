using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Utils;

namespace Winforms_App_Template.Forms
{
    public partial class Testreport : XtraReport
    {

        private readonly Parameter pOK = new Parameter
        {
            Name = "p_OKText",          // Tên dùng trong Expression: Parameters.p_OKText
            Type = typeof(string),
            Value = "OK",               // Giá trị mặc định
            Visible = false             // Ẩn khỏi UI
        };

        private readonly Parameter pNG = new Parameter
        {
            Name = "p_NGText",
            Type = typeof(string),
            Value = "NG",
            Visible = false
        };

        public Testreport()
        {
            InitializeComponent();

            // KHÔNG gán DataSource ở cấp Report → tránh lặp tất cả controls trong Detail của Report.
            this.DataSource = null;
            this.DataMember = null;

            // Không hiện UI hỏi tham số khi in/xem
            this.RequestParameters = false;

            // GẮN tham số vào report để có thể sử dụng
            this.Parameters.AddRange(new[] { pOK, pNG });
        }

        // Khi giá trị trong trường (fieldName) là True, False, Y, N thì tự động đổi sang value của parameter tương ứng mà ta đã khai báo bên trên
        private static void BindYesNo(XRTableCell cell, string fieldName)
        {
            // In(value, a, b, c, ...) trả true nếu value thuộc 1 trong các giá trị liệt kê.
            // Tương tự (Xuống dòng thì sử dụng dấu +):
            // "Iif(Lower([val5]) == 'true' Or [val5] == '1' Or Lower([val5]) == 'y' Or Lower([val5]) == 'yes', " +
            // "Parameters.p_OKText, Parameters.p_NGText))"
            cell.ExpressionBindings.Clear();
            cell.ExpressionBindings.Add(new ExpressionBinding(
                "BeforePrint",                                                     // chạy trước khi in/hiển thị
                "Text",                                                            // gán vào thuộc tính Text của cell
                $"Iif(IsNullOrEmpty([{fieldName}]), '', " +    // Nếu giá trị fieldName là rỗng hoặc null thì trả về Text là "" (chuỗi trồng)
                $"Iif(In(Lower([{fieldName}]), 'true','1','y','yes'), Parameters.p_OKText, Parameters.p_NGText))"  // Nếu giá trị là 1 trong những từ kia thì lấy value Ok, ko thì lấy value NH
            ));

            // Hoặc có thể bind data bằng CalculatedField để tạo hàm ExpressionBindings tương đồng khi bind bằng "[fieldname]"
            //var cfVal5 = new CalculatedField
            //{
            //    Name = "calc_val5",
            //    FieldType = FieldType.String,
            //    // Có thể dùng biểu thức In(...) như trên
            //    Expression = "Iif(IsNullOrEmpty([val5]), '', Iif(In(Lower([val5]), 'true','1','y','yes'), Parameters.p_OKText, Parameters.p_NGText))"
            //};
            //this.CalculatedFields.Add(cfVal5);

            //// Cell chỉ cần bind "[calc_val5]" đã khai báo Name
            //cellVal5.ExpressionBindings.Clear();
            //cellVal5.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[calc_val5]"));
        }

        // Hàm công khai để form bên ngoài đổi text OK/NG động (ví dụ: “Đạt/Không đạt, Xuyêm/Không xuyên”)
        public void SetOkNgText(string okText, string ngText)
        {
            this.Parameters["p_OKText"].Value = okText;
            this.Parameters["p_NGText"].Value = ngText;
        }

        /// <summary>
        /// GÁN DataSource = rows CHO CHÍNH DetailReportBand "Catongtho_Report".
        /// Bind header (in 1 lần) từ "headerData".
        /// Bind 2 bảng lặp trong Detail: Catthoong_Table, Check_Table.
        /// Note_Richtext: mặc định lặp theo từng record
        /// </summary>
        /// <param name="rows">Danh sách bản ghi chi tiết (sẽ lặp)</param>
        /// <param name="headerData">Dữ liệu in 1 lần cho header</param>
        /// <param name="notePrintOnlyOnce">true => Note_Richtext chỉ in ở record đầu</param>
        public void ConfigureLayoutForCatongtho(
            IList<Catthoong_ReportRow> rows,
            Catongtho_HeaderModel headerData,
            bool notePrintOnlyOnce = false)
        {
            // Kiểm tra có dữ liệu đầu vào
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows), "rows null: không có dữ liệu để lặp.");
            }

            // Lấy đúng DetailReportBand (dr) theo Name = "Catongtho_Report"
            DetailReportBand dr = this.FindDetailReportBandByName("Catongtho_Report");

            // Gán DataSource cho DR (KHÔNG gán cho report, chỉ mỗi detail mà thôi)
            dr.DataSource = rows;
            dr.DataMember = null; // List<T> không cần DataMember

            // Hiển thị ra datasource đã gắn vào có bao nhiêu bản ghi
            XtraTableAutoBinder.AssertBandDataSource(dr: dr);

            // Lấy ReportHeader của Detail Report theo Name = "Catongtho_Header" (Header chỉ in 1 lần)
            ReportHeaderBand drHeader = this.FindChildReportHeader(dr, "Catongtho_Header");
            // Lấy header table cần bind dữ liệu
            XRTable? header_table = drHeader.FindControl("Header_Table", true) as XRTable;
            // Bind dữ liệu từ tag với cấu trúc a|b|c|d|e, c sẽ là FiledName trong DB
            this.AutoBindHeaderTable_ByTag_UsingParameters(header_table, headerData);
            // kiểm tra xem bảng hearder đã bind được dữ liệu hay chưa
            //XtraTableAutoBinder.DumpBindings(header_table, "Header_Table");

            // Lấy Detail con đầu tiên trong Deatil Report cha
            DetailBand drDetail = this.FindChildDetail(dr);
            if (drDetail == null)
            {
                throw new InvalidOperationException("Catongtho_Report thiếu Detail (band con).");
            }

            // Tìm kiếm 2 bảng và 1 richtext trong detail con
            XRTable? tblA = drDetail.FindControl("Catthoong_Table", true) as XRTable;
            XRTable? tblB = drDetail.FindControl("Check_Table", true) as XRTable;
            XRRichText? noteRtf = drDetail.FindControl("Note_Richtext", true) as XRRichText;

            if (tblA == null) throw new InvalidOperationException("Thiếu XRTable: Catthoong_Table trong Catongtho_Report.Detail.");
            if (tblB == null) throw new InvalidOperationException("Thiếu XRTable: Check_Table trong Catongtho_Report.Detail.");
            if (noteRtf == null) { /* không bắt buộc, nhưng cảnh báo nhẹ */ }

            // Gọi auto-bind cho tất cả bảng thuộc DR (đọc meta từ Tag, yêu cầu tối thiểu 3 phần a|b|c):
            //XtraTableAutoBinder.AutoBindAllTablesInDetailReportByTag(
            //    dr,
            //    readFromTag: true,
            //    delimiter: '|',
            //    fieldSegmentIndex: 2,   // lấy c
            //    minSegmentCount: 3,
            //    strictMeta: false,      // true nếu muốn fail-fast khi meta sai
            //    checkFieldExists: true  // bật để cảnh báo field không tồn tại trong model
            //);

            // Hoặc bind cụ thể 1 bảng
            XtraTableAutoBinder.AutoBindSingleTableByTag(
                    table: tblA,
                    checkFieldExists: true,
                    dataSourceForCheck: rows
                );

            // Kiểm tra 1 bảng cụ thể các binding trong từng cell
            XtraTableAutoBinder.DumpBindings(tblA, "Catthoong_Table");

            // Catthoong_Table: dùng map cell -> biểu thức theo Catthoong_Model
            //this.BindCatthoongTableCells(tblA);

            // Check_Table
            //this.BindCheckTableCells(tblB);

            //  Note_Richtext:
            if (noteRtf != null)
            {
                //  - Nếu dữ liệu là plain text: bind "Text"
                //  - Nếu dữ liệu là chuỗi RTF:   bind "Rtf" (hoặc SerializableRtfString)
                noteRtf.ExpressionBindings.Clear();
                noteRtf.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Note]"));

                // Trường hợp chỉ in ghi chú ở record đầu:
                if (notePrintOnlyOnce)
                {
                    bool printedOnce = false; // cờ đóng trên instance report
                    drDetail.BeforePrint += (s, e) =>
                    {
                        if (!printedOnce)
                        {
                            noteRtf.Visible = true;
                            noteRtf.CanShrink = true;
                            printedOnce = true;
                        }
                        else
                        {
                            // Ẩn và co chiều cao tránh khoảng trắng
                            noteRtf.Visible = false;
                            noteRtf.CanShrink = true;
                            noteRtf.HeightF = 0f;
                        }
                    };
                }
            }

            // 8) (Tuỳ chọn) đảm bảo band cao vừa đủ
            //drDetail.HeightF = this.ComputeBandHeight(drDetail, 4f);
            //drHeader.HeightF = this.ComputeBandHeight(drHeader, 6f);
        }

        /// <summary>
        /// Bind cell trong bảng Catthoong_Table theo Catthoong_Model (map đầy đủ bạn đưa trước đó).
        /// </summary>
        private void BindCatthoongTableCells(XRTable tableScope)
        {
            // Tạo 1 dictionary chứa danh sách tên của ô nhập trong design và filed sẽ lấy từ DB
            Dictionary<string, string> map = new Dictionary<string, string>();

            map.Add("Reason_Check", "[Reason]");
            map.Add("Time_Check", "[TimeAndWorker]");
            map.Add("Person_Check", "''"); // Chưa có field riêng → để trống
            map.Add("Machine_number", "[Machine]");

            map.Add("Number_Of_Use", "[QtyUsed]");
            map.Add("Number_Of_Cut_Pipes", "[QtyCut]");

            map.Add("Thickness_Gauge_Code", "[ThickGauge]");
            map.Add("Outer_Diameter", "[OuterDiameter]");
            map.Add("Outer_Diameter_Check", "Iif(Not IsNullOrEmpty([OuterDiameter]), 'OK', 'NG')");

            map.Add("Pingauge_Code", "[Pin098]");
            map.Add("Cut_Check", "[CutState]");
            map.Add("Cutting_Length", "[CutLengths]");

            map.Add("Cutting_Length_Number_1", "''"); // nếu có 3 field riêng, đổi sang [CutLen1]
            map.Add("Cutting_Length_Number_2", "''");
            map.Add("Cutting_Length_Number_3", "''");

            map.Add("Cutting_Length_Check", "[CutState]");

            map.Add("Bevel_Cut", "[NG_CatVat]");
            map.Add("Flat", "[NG_Bep]");
            map.Add("Bavia", "[NG_Bavia]");
            map.Add("Fall", "[NG_Roi]");
            map.Add("Beyond_The_Standard", "[NG_LengthOut]");
            map.Add("Other", "[NG_Khac]");

            map.Add("Check_Inventory", "[Acceptance]");

            this.BindCellsByMap(tableScope, map, /*strict*/ true);
        }

        /// <summary>
        /// Bind cell trong bảng Check_Table (bạn đổi tên cell theo đúng Designer).
        /// Mình để ví dụ các cell thường thấy; nếu thiếu cell → ném lỗi (strict=true).
        /// </summary>
        private void BindCheckTableCells(XRTable tableScope)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            map.Add("Outer_Diameter_Item", "[CutState]");                 
            map.Add("Outer_Diameter_Size", "[OuterDiameter]");       
            map.Add("Outer_Diameter_Pingauge_Through", "Iif(Not IsNullOrEmpty([OuterDiameter]), 'OK', 'NG')");
            map.Add("Outer_Diameter_Pingauge_Not_Through", "[Pin098]");
            map.Add("Outer_Diameter_Criterion", "[InnerJudge]");
            map.Add("Number_Cut_Item", "[Acceptance]");
            map.Add("Number_Cut_Size", "[CutLengths]");
            map.Add("Number_Cut_Pingauge_Through", "[CutLengths]");
            map.Add("Number_Cut_Pingauge_Not_Through", "[CutLengths]");
            map.Add("Number_Cut_Criterion", "[CutLengths]");
            map.Add("Cut_Length_Item", "[CutLengths]");
            map.Add("Cut_Length_Size", "[CutLengths]");
            map.Add("Cut_Length_Pingauge_Through", "[CutLengths]");
            map.Add("Cut_Length_Pingauge_Not_Through", "[CutLengths]");
            map.Add("Cut_Length_Criterion", "[CutLengths]");

            this.BindCellsByMap(tableScope, map, /*strict*/ false); // để false cho “nhẹ tay”, đỡ ném lỗi khi cell chưa trùng tên
        }


        /// <summary>
        /// Tìm DetailReportBand theo Name (đúng với Name bạn đặt trong Designer).
        /// Ném lỗi nếu không thấy.
        /// </summary>
        private DetailReportBand FindDetailReportBandByName(string name)
        {
            foreach (Band b in this.Bands)
            {
                DetailReportBand? dr = b as DetailReportBand;
                if (dr != null && string.Equals(dr.Name, name, StringComparison.Ordinal))
                {
                    return dr;
                }
            }
            throw new InvalidOperationException("Không tìm thấy DetailReportBand: " + name);
        }

        /// <summary>
        /// Tìm ReportHeader (con) theo Name trong một DetailReportBand.
        /// </summary>
        private ReportHeaderBand FindChildReportHeader(DetailReportBand dr, string name)
        {
            foreach (Band b in dr.Bands)
            {
                ReportHeaderBand? rh = b as ReportHeaderBand;
                if (rh != null && string.Equals(rh.Name, name, StringComparison.Ordinal))
                {
                    return rh;
                }
            }
            // Nếu Designer không đặt Name, lấy ReportHeader đầu tiên
            foreach (Band b in dr.Bands)
            {
                ReportHeaderBand? rh = b as ReportHeaderBand;
                if (rh != null) return rh;
            }
            throw new InvalidOperationException("Không tìm thấy ReportHeaderBand con: " + name);
        }

        /// <summary>
        /// Lấy Detail (con) đầu tiên của một DetailReportBand.
        /// </summary>
        private DetailBand FindChildDetail(DetailReportBand dr)
        {
            foreach (Band b in dr.Bands)
            {
                DetailBand? d = b as DetailBand;
                if (d != null) return d;
            }
            return null;
        }

       

        /// <summary>
        /// Duyệt map và bind cell trong 1 scope (table/band).
        /// strict=true: thiếu cell → ném lỗi; false: bỏ qua và có thể log.
        /// </summary>
        private void BindCellsByMap(XRControl scope, Dictionary<string, string> map, bool strict)
        {
            // Duyệt từng cặp (cellName, expression) trong map.
            foreach (KeyValuePair<string, string> kv in map)
            {
                // Tìm các phần tử có tên (map.Key) nằm trong scope không, sử dụng đệ quy (true)
                XRControl cell = scope.FindControl(kv.Key, true) as XRControl;
                if (cell == null)
                {
                    // Lựa chọn ném ra ngoại lệ hoặc chỉ cần warning, chương trình vẫn chạy tiếp
                    if (strict)
                    {
                        throw new InvalidOperationException("Thiếu cell: " + kv.Key + " trong " + scope.Name);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARN: Không thấy cell " + kv.Key + " (bỏ qua).");
                        continue;
                    }
                }
                // Xóa binding đang tồn tại (nếu có) trong cell này trước khi binding dữ liệu mới
                cell.ExpressionBindings.Clear();
                // Mỗi khi có sự kiện `BeforePrint` thì sẽ lấy chuỗi expression DevExpress (ví dụ "[Reason]", Iif(...), Concat(...)…) từ kv.Value 
                // Lấy trường dữ liệu kv.Value cảu bản ghi hiện tại từ datasource đưa vào trường Text của cell
                cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", kv.Value));
                // cell.ExpressionBindings.Add(
                //new ExpressionBinding(
                //    "BeforePrint",                // Khi sắp in control cho mỗi bản ghi → tính biểu thức
                //    "BackColor",                  // Gán kết quả vào thuộc tính BackColor
                //    "Iif([CutState] == 'NG', 'Tomato', 'White')" // Biểu thức: nếu NG → đỏ cà chua, ngược lại trắng
                //));
            }
        }

        /// <summary>
        /// Bind dữ liệu từ Tag của một cell có cấu trúc tên: a|b|c|d|e với c là FiledName
        /// </summary>
        /// <param name="headerTable"></param>
        /// <param name="headerData"></param>
        /// <param name="delimiter"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void AutoBindHeaderTable_ByTag_UsingParameters(XRTable? headerTable, object headerData, char delimiter = '|')
        {
            // Validation dữ liệu đầu vào
            if (headerTable == null) throw new ArgumentNullException(nameof(headerTable));
            if (headerData == null) throw new ArgumentNullException(nameof(headerData));

            // Lấy mảng các PropertyInfo public của kiểu headerData (các trường trong Model)
            var props = headerData.GetType().GetProperties();

            // Tạo dictionary để tra cứu nhanh theo tên property, không phân biệt hoa/ thường
            // (StringComparer.OrdinalIgnoreCase đảm bảo "Name" == "name")
            var propMap = new Dictionary<string, System.Reflection.PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            // Dùng vòng lặp for để duyệt mảng props (nhanh hơn foreach trong một số trường hợp)
            for (int i = 0; i < props.Length; i++)
            {
                var pi = props[i];              // Lấy PropertyInfo hiện tại

                // Chỉ thêm những property có thể đọc được (tránh property write-only, hiếm gặp)
                if (pi.CanRead)
                    propMap[pi.Name] = pi;      // Gán vào map: key = tên property, value = PropertyInfo
                                                // Nếu có hai property cùng tên (không thường xảy ra), entry sau cùng sẽ ghi đè
            }

            // Duyệt từng hàng trong bảng
            foreach (XRTableRow row in headerTable.Rows)
            {
                // Duyệt từng cell trong 1 hàng
                foreach (XRTableCell cell in row.Cells)
                {
                    // Lấy meta từ Tag (Vì Name ko cho đặt | nên đặt tên từ Tag)
                    string? meta = cell.Tag == null ? null : Convert.ToString(cell.Tag);
                    // Nếu meta không có giá trị thì bỏ qua cell này, ko làm gì cả
                    if (string.IsNullOrWhiteSpace(meta)) continue;

                    // Tách Tag thành các chuỗi phân cách bởi ký tự: |
                    string[] parts = meta.Split(new char[] { delimiter }, StringSplitOptions.None);
                    if (parts.Length < 3) continue; // cần tối thiểu a|b|c

                    // Lấy chuỗi thứ 3 trong Tab: a|b|c|d|e
                    string? fieldName = parts[2] == null ? null : parts[2].Trim();
                    if (string.IsNullOrEmpty(fieldName)) continue;

                    // Tìm property tương ứng trên headerData
                    System.Reflection.PropertyInfo pi; // Khai báo biến pi kiểu PropertyInfo

                    // Tìm trong promap key = fieldname gán vào biến pi, nếu ko tồn tại fieldname (False) thì thông báo
                    if (!propMap.TryGetValue(fieldName, out pi))
                    {
                        // Không có property tương ứng → bỏ qua cell
                        System.Diagnostics.Debug.WriteLine($"WARN header: không có property '{fieldName}' trên {headerData.GetType().Name}");
                        continue;
                    }

                    // Lấy giá trị từ key được gán cho pi
                    object? value = pi.GetValue(headerData, null);

                    // Tạo/cập nhật parameter p_<fieldName>
                    string paramName = "p_" + fieldName;
                    this.CreateOrUpdateParameter(paramName, value);

                    // Bind Text = Parameters.p_<fieldName>
                    cell.ExpressionBindings.Clear();  // Xóa các bind nếu nó tồn tại, tránh chồng chéo bind 
                    cell.ExpressionBindings.Add(
                        new ExpressionBinding("BeforePrint", "Text", "Parameters." + paramName)
                    );
                }
            }
        }

        /// <summary>
        /// Tạo/ghi đè một Parameter tên 'name' với 'value' (dùng cho header).
        /// </summary>
        private void CreateOrUpdateParameter(string name, object value)
        {
            // [Field] trong ExpressionBinding("BeforePrint", "Text", "[Field]") luôn được lấy từ DataSource/DataMember của band chứa control đó (Detail, GroupHeader/GroupFooter, DetailReportBand,…)
            // Header lấy từ nguồn khác dataset Detail (ví dụ headerData là object khác, query khác), thì [Field] sẽ không thấy gì → ra trống.
            // Vi vậy sử dụng Parameter: tạo Parameters.p_... = headerData.X, rồi bind Text = Parameters.p_....

            // Lấy parameter theo tên từ bộ sưu tập this.Parameters của report. Parameter chỉ tồn tại trong report của DevExpress
            Parameter p = Parameters[name];
            // Nếu chưa có thì tiến hành tạo mới
            if (p == null)
            {
                p = new Parameter();
                p.Name = name;
                p.Visible = false; // không hiện ở UI
                Parameters.Add(p);
            }
            // Gán giá trị cho parameter này
            p.Value = value;
        }
    }
}
