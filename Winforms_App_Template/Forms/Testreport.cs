using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using DevExpress.XtraReports.Parameters;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Utils;

namespace Winforms_App_Template.Forms
{
    public partial class Testreport : XtraReport
    {
        public Testreport()
        {
            InitializeComponent();

            // KHÔNG gán DataSource ở cấp Report → tránh lặp tất cả controls trong Detail của Report.
            this.DataSource = null;
            this.DataMember = null;
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
            IList<Catthoong_Model> rows,
            Catongtho_HeaderModel headerData,
            bool notePrintOnlyOnce = false)
        {
            // Kiểm tra có dữ liệu đầu vào không
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows), "rows null: không có dữ liệu để lặp.");
            }

            // Lấy đúng DetailReportBand (dr) theo Name = "Catongtho_Report"
            DetailReportBand dr = this.FindDetailReportBandByName("Catongtho_Report");

            // Gán DataSource cho DR (KHÔNG gán cho report, chỉ mỗi detail mà thôi)
            dr.DataSource = rows;
            dr.DataMember = null; // List<T> không cần DataMember

            // Lấy ReportHeader của Detail Report theo Name = "Catongtho_Header" (Header chỉ in 1 lần)
            ReportHeaderBand drHeader = this.FindChildReportHeader(dr, "Catongtho_Header");
            // Lấy header table cần bind dữ liệu
            XRTable? header_table = drHeader.FindControl("Header_Table", true) as XRTable;
            // Bind dữ liệu từ tag với cấu trúc a|b|c|d|e, c sẽ là FiledName trong DB
            this.AutoBindHeaderTable_ByTag_UsingParameters(header_table, headerData);
            //Binding_Xtratbale_Tag.AutoBindCellsByDelimitedMeta(header_table, true);

            // Lấy bảng cần chứa dữ liệu từ DB

            // Tìm các cột cần chứa dữ liệu trong header
            XRControl h_Name = drHeader.FindControl("Name_Congdoan", true);
            XRControl h_ID = drHeader.FindControl("ID_Congdoan", true);
            XRControl h_Code = drHeader.FindControl("Code_Congdoan", true);
            XRControl h_Lot = drHeader.FindControl("Lotno_Congdoan", true);
            XRControl h_Batch = drHeader.FindControl("Batch_Number", true);
            XRControl h_NG = drHeader.FindControl("NG_Qty_Total", true);
            XRControl h_OK = drHeader.FindControl("OK_Qty_Total", true);

            // Lấy Detail con đầu tiên trong Deatil Report cha
            DetailBand drDetail = this.FindChildDetail(dr);
            if (drDetail == null)
            {
                throw new InvalidOperationException("Catongtho_Report thiếu Detail (band con).");
            }

            // Truy vấn 2 bảng và 1 richtext trong detail con
            XRTable tblA = drDetail.FindControl("Catthoong_Table", true) as XRTable;
            XRTable tblB = drDetail.FindControl("Check_Table", true) as XRTable;
            XRRichText noteRtf = drDetail.FindControl("Note_Richtext", true) as XRRichText;

            if (tblA == null) throw new InvalidOperationException("Thiếu XRTable: Catthoong_Table trong Catongtho_Report.Detail.");
            if (tblB == null) throw new InvalidOperationException("Thiếu XRTable: Check_Table trong Catongtho_Report.Detail.");
            if (noteRtf == null) { /* không bắt buộc, nhưng cảnh báo nhẹ */ }

            //// ==== Bind dữ liệu cho header ====
            //// Nếu có dữ liệu header riêng --> gán trực tiếp Text qua ExpressionBinding với nguồn là Parameters
            //if (headerData != null)
            //{
            //    // Tạo các Parameter nội bộ để header bind vào (cách sạch, không đụng rows).
            //    // Vì ReportHeader của DR cũng có DataContext, ta muốn tách độc lập → dùng Parameters là đơn giản nhất.
            //    this.BindHeaderByParameters(drHeader, headerData, h_Name, h_ID, h_Code, h_Lot, h_Batch, h_NG, h_OK);
            //}
            //else
            //{
            //    // Không có headerData → lấy từ record đầu của rows (bind aggregate hoặc First)
            //    // Ở DevExpress, có thể dùng Min/Max cho text ổn định, nếu field giống nhau mọi record.
            //    this.BindHeaderFromFirstRecord(drHeader, h_Name, h_ID, h_Code, h_Lot, h_Batch, h_NG, h_OK);
            //}

            // ==== Bind 2 bảng lặp trong DETAIL ====

            // Catthoong_Table: dùng map cell -> biểu thức theo Catthoong_Model
            this.BindCatthoongTableCells(tblA);

            // Check_Table
            //this.BindCheckTableCells(tblB);

            // 7.3 Note_Richtext:
            if (noteRtf != null)
            {
                // Trường hợp 1: muốn GHI CHÚ theo từng record:
                //  - Nếu dữ liệu là plain text: bind "Text"
                //  - Nếu dữ liệu là chuỗi RTF:   bind "Rtf" (hoặc SerializableRtfString)
                noteRtf.ExpressionBindings.Clear();
                noteRtf.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Note]"));

                // Trường hợp 2: chỉ in ghi chú ở record đầu:
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

        // ----------------- BINDING IMPLEMENTATIONS -----------------

        /// <summary>
        /// Bind HEADER từ một đối tượng headerData bằng Parameters → ReportHeader in 1 lần, độc lập với rows.
        /// </summary>
        private void BindHeaderByParameters(
            ReportHeaderBand header,
            Catongtho_HeaderModel headerData,
            XRControl name, XRControl id, XRControl code, XRControl lot, XRControl batch, XRControl ng, XRControl ok)
        {
            // Tạo parameters :
            // Vì header in-một - lần, dùng Parameter giúp:
            //    - Không phụ thuộc vào dữ liệu chi tiết (có thể rows = 0 vẫn hiển thị).
            //    - Không cần field tương ứng trong model chi tiết.
            //    - Giá trị ổn định, không đổi khi band lặp record.
            this.CreateOrUpdateParameter("p_Name_Congdoan", headerData.Name_Congdoan);
            this.CreateOrUpdateParameter("p_ID_Congdoan", headerData.Code_Congdoan);
            this.CreateOrUpdateParameter("p_Code_Congdoan", headerData.Category_Code);
            this.CreateOrUpdateParameter("p_Lotno_Congdoan", headerData.Lotno_Congdoan);
            this.CreateOrUpdateParameter("p_Batch_Number", headerData.Batch_Number);
            this.CreateOrUpdateParameter("p_NG_Qty_Total", headerData.NG_Qty_Total);
            this.CreateOrUpdateParameter("p_OK_Qty_Total", headerData.OK_Qty_Total);

            // Gán ExpressionBindings cho từng control trong header.
            //    Ở DevExpress, khi viết expression tham chiếu parameter, ta dùng: Parameters.TenParameter
            this.BindTextToParameter(name, "Parameters.p_Name_Congdoan");
            this.BindTextToParameter(id, "Parameters.p_ID_Congdoan");
            this.BindTextToParameter(code, "Parameters.p_Code_Congdoan");
            this.BindTextToParameter(lot, "Parameters.p_Lotno_Congdoan");
            this.BindTextToParameter(batch, "Parameters.p_Batch_Number");
            this.BindTextToParameter(ng, "Parameters.p_NG_Qty_Total");
            this.BindTextToParameter(ok, "Parameters.p_OK_Qty_Total");
        }

        /// <summary>
        /// Bind HEADER “lấy từ record đầu” (nếu không truyền headerData).
        /// Dùng aggregate Min(...) để ổn định giá trị (giả định các record cùng giá trị).
        /// </summary>
        private void BindHeaderFromFirstRecord(
            ReportHeaderBand header,
            XRControl name, XRControl id, XRControl code, XRControl lot, XRControl batch, XRControl ng, XRControl ok)
        {
            // Dùng Min([Field]) vì:
            //     - DevExpress hỗ trợ Min/Max/Avg/Sum/Count chuẩn.
            //     - Nếu tất cả record có cùng giá trị header, Min == Max == giá trị đó.
            //     - Tránh phụ thuộc "thứ tự" (không có "First()" chuẩn trong mọi phiên bản).
            this.SetTextBindingSafe(name, "Min([Name_Congdoan])");
            this.SetTextBindingSafe(id, "Min([ID_Congdoan])");
            this.SetTextBindingSafe(code, "Min([Code_Congdoan])");
            this.SetTextBindingSafe(lot, "Min([Lotno_Congdoan])");
            this.SetTextBindingSafe(batch, "Min([Batch_Number])");
            this.SetTextBindingSafe(ng, "Min([NG_Qty_Total])");
            this.SetTextBindingSafe(ok, "Min([OK_Qty_Total])");
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

        // ----------------- HELPERs: tìm band, bind an toàn, v.v. -----------------

        /// <summary>
        /// Tìm DetailReportBand theo Name (đúng với Name bạn đặt trong Designer).
        /// Ném lỗi nếu không thấy.
        /// </summary>
        private DetailReportBand FindDetailReportBandByName(string name)
        {
            foreach (Band b in this.Bands)
            {
                DetailReportBand dr = b as DetailReportBand;
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
                ReportHeaderBand rh = b as ReportHeaderBand;
                if (rh != null && string.Equals(rh.Name, name, StringComparison.Ordinal))
                {
                    return rh;
                }
            }
            // Nếu Designer không đặt Name, lấy ReportHeader đầu tiên
            foreach (Band b in dr.Bands)
            {
                ReportHeaderBand rh = b as ReportHeaderBand;
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
                DetailBand d = b as DetailBand;
                if (d != null) return d;
            }
            return null;
        }

        /// <summary>
        /// Bind Text = [expr] an toàn: bỏ qua nếu control null.
        /// </summary>
        private void SetTextBindingSafe(XRControl control, string expr)
        {
            if (control == null) return;
            control.ExpressionBindings.Clear();
            control.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", expr));
        }

        /// <summary>
        /// Bind Text = Parameters.xxx cho một control (cell header).
        /// </summary>
        private void BindTextToParameter(XRControl control, string paramPath)
        {
            // Kiểm tra có truyền vào control không
            if (control == null) return;
            // Xóa các bind đang tồn tại
            control.ExpressionBindings.Clear();
            // Binding dữ liệu trước khi in (BeforePrint) vào trường Text của control
            control.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", paramPath));
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
                    object value = pi.GetValue(headerData, null);

                    // Tạo/cập nhật parameter p_<fieldName>
                    string paramName = "p_" + fieldName;
                    this.CreateOrUpdateParameter(paramName, value);

                    // Bind Text = Parameters.p_<fieldName>
                    cell.ExpressionBindings.Clear();
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
            // Tạo parameter vì Datasource chỉ áp dụng trên toàn detail, các band khác không thể 
            // Nếu đã có, cập nhật; nếu chưa, thêm mới
            Parameter p = this.Parameters[name];
            if (p == null)
            {
                p = new Parameter();
                p.Name = name;
                p.Visible = false; // không hiện ở UI
                this.Parameters.Add(p);
            }
            p.Value = value;
        }

        /// <summary>
        /// Tính chiều cao đủ chứa controls bên trong một band, cộng padding phụ.
        /// </summary>
        private float ComputeBandHeight(Band band, float extraPadding)
        {
            float maxBottom = 0f;
            foreach (XRControl c in band.Controls)
            {
                float bottom = c.LocationF.Y + c.HeightF;
                if (bottom > maxBottom) maxBottom = bottom;
            }
            return maxBottom + extraPadding;
        }
    }
}
