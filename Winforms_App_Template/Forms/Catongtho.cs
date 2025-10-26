using DevExpress.Drawing.Printing;
using DevExpress.Utils;
using DevExpress.XtraEditors;                   // TextEdit, SimpleButton
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.BandedGrid;    // BandedGridView, BandedGridColumn
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;                   // LayoutControl, LayoutControlGroup
using DevExpress.XtraLayout.Utils;             // LayoutMode.Table
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing; // ImageSource
using DevExpress.XtraPrintingLinks;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;                 // PaperKind, Margins
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Database.Table;

namespace Winforms_App_Template.Forms
{
    public partial class Catongtho : Form
    {
        private readonly NewInputs_Table _repo;         // Repository Dapper
        private CancellationTokenSource? _cts;          // Hủy tải dữ liệu

        private MayBan_Table _mayRepo;                          // repo danh mục máy
        private readonly Input_Error_Table input_error_repo;         // Repository cho bảng input_Error
        private readonly Standard_Table standard_repo;         // Repository cho bảng tiêu chuẩn

        public Catongtho(DbExecutor? db = null)
        {
            InitializeComponent();

            var executor = db ?? new DbExecutor();
            _repo = new NewInputs_Table(executor);
            _mayRepo = new MayBan_Table(executor);
            input_error_repo = new Input_Error_Table(executor);
            standard_repo = new Standard_Table(executor);

        }

        // Chuẩn hoá tên lỗi để map chắc chắn (bỏ khoảng trắng thừa, không phân biệt hoa/thường)
        private static string NormalizeKey(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return s.Trim().ToLowerInvariant();
        }

        // Build: { idInput => { "cắt vát" => qty, "bẹp" => qty, ... } }
        private static Dictionary<int, Dictionary<string, int>> BuildPivotMap(List<Input_Error_Model> errors)
        {
            var map = new Dictionary<int, Dictionary<string, int>>();

            foreach (var e in errors)
            {
                var key = NormalizeKey(e.TenLoi); // dùng TenLoi; fallback có thể dùng $"e{e.IdError}"
                if (!map.TryGetValue(e.idInput, out var inner))
                {
                    inner = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    map[e.idInput] = inner;
                }

                // cộng dồn theo tên lỗi
                inner[key] = inner.TryGetValue(key, out var cur) ? cur + e.Qty : e.Qty;
            }

            return map;
        }

        // Set 6 thuộc tính theo dictionary {tenLoi=>qty}
        private static void SetKnownErrorColumns(Catthoong_ReportRow dest, Dictionary<string, int>? kv)
        {
            dest.Bevel_Cut = 0;
            dest.Flat = 0;
            dest.Bavia = 0;
            dest.Fall = 0;
            dest.Beyond_The_Standard = 0;
            dest.Other = 0;

            if (kv == null || kv.Count == 0) return;

            // Map theo khóa chuẩn hoá (lowercase)
            dest.Bevel_Cut = kv.TryGetValue("cắt vát", out var v1) ? v1 : 0;
            dest.Flat = kv.TryGetValue("bẹp", out var v2) ? v2 : 0;
            dest.Bavia = kv.TryGetValue("bavia", out var v3) ? v3 : 0;
            dest.Fall = kv.TryGetValue("rơi", out var v4) ? v4 : 0;
            dest.Beyond_The_Standard = kv.TryGetValue("chiều dài ngoài tiêu chuẩn", out var v5) ? v5 : 0;
            dest.Other = kv.TryGetValue("khác", out var v6) ? v6 : 0;
        }

        // Ví dụ: gọi từ một Form (ví dụ khi nhấn nút)
        private async Task btnShowReport_Click(object? sender, EventArgs? e)
        {
            // Lấy data cho header
            Catongtho_HeaderModel header = await _repo.Get_Header_catthoong(IdCongDoan: 68, ItemNumber: "CRS25C60N10W", LotNo: "250505G01", So_Me: 1);
            // Dữ liệu mẫu cho Header in 1 lần
            //Catongtho_HeaderModel header = new Catongtho_HeaderModel
            //{
            //    Name_Congdoan = "Cắt thô ống/ abc",
            //    ID_Congdoan = "68",
            //    Code_Congdoan = "zzzzzzz",
            //    Category_Code = "123456G01",
            //    Lotno_Congdoan = "12316541",
            //    Batch_Number = "1",
            //    NG_Qty_Total = 1,
            //    OK_Qty_Total = 900
            //};

            // Lấy data cho bảng trong detail
            List<Catthoong_Row> rows = await _repo.Get_Cat_Ong_Tho(IdCongDoan: 68, ItemNumber: "CRS25C60N10W", LotNo: "250505G01", So_Me: 1);

            // Lấy danh sách id_input từ list thu được
            var List_IdInput = rows.Select(x => x.idInput).Distinct().ToArray();

            // Lấy chi tiết lỗi
            List<Input_Error_Model> detail_error = await input_error_repo.Get_Detail_Error(idInputs: List_IdInput);

            var pivot = BuildPivotMap(detail_error); // idInput -> (tenLoi->qty)

            // Xử lý data trước khi bind Gom lỗi theo IdInput để lookup nhanh
            //var errByInput = detail_error
            //    .GroupBy(e => e.idInput)
            //    .ToDictionary(g => g.Key, g => g.ToList());

            // Gộp dữ liệu vào 1 list
            List<Catthoong_ReportRow> result = new List<Catthoong_ReportRow>(rows.Count);

            // Thêm từng dòng chi tiết lỗi cũng 1 idinput vào cùng nhau
            foreach (var m in rows)
            {
                // Tìm dictionary lỗi của idInput hiện tại
                pivot.TryGetValue(m.idInput, out var errsDict);
                // Tóm tắt lỗi


                // Tạo report-row

                var r = new Catthoong_ReportRow
                {
                    // copy main
                    idInput = m.idInput,
                    MaKT = m.MaKT,
                    TenMay_Ban = m.TenMay_Ban,
                    SLSudung = m.SLSudung,
                    StartTime = m.StartTime,
                    NguoiTT = m.NguoiTT,

                    val1 = m.val1,
                    val2 = m.val2,
                    val3 = m.val3,
                    val4 = m.val4,
                    val5 = m.val5,
                    val6 = m.val6,
                    val7 = m.val7,
                    val8 = m.val8,
                    val9 = m.val9,
                    val10 = m.val10,
                    val11 = m.val11,
                    val12 = m.val12,
                    val13 = m.val13,
                    val14 = m.val14,
                    val15 = m.val15,

                    Remark = m.Remark
                };
                // Gán 6 cột lỗi ngang theo map (KHÔNG dùng qtySum cho tất cả)
                SetKnownErrorColumns(r, errsDict);

                result.Add(r);
            }

            // Lấy dữ liệu cho bảng tiêu chuẩn
            List<Standard_Model> List_Standard_Report = await standard_repo.Get_Detail_Standard(idInputs: List_IdInput);
            // Nhóm tiêu chuẩn theo idInput để tra nhanh
            var stdByInput = List_Standard_Report
                .GroupBy(s => s.idInput)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var m in result)
            {
                if (stdByInput.TryGetValue(m.idInput, out var list))
                    m.Standards = list;                 // ~ 3 dòng cho mỗi lần nhập
                else
                    m.Standards = new List<Standard_Model>();
            }

            var rpt = new Testreport();
            rpt.ConfigureLayoutForCatongtho(result, List_Standard_Report, header, /*notePrintOnlyOnce*/ false);

            new ReportPrintTool(rpt).ShowRibbonPreviewDialog();
        }

        private async void Export_Document_Button_Click(object sender, EventArgs e)
        {

            // Lấy dữ liệu đầu vào
            string ID_Cong_Doan_String = ID_Cong_Doan_Text.Text;
            string ItemNumber = Item_Number_Text.Text;
            string LotNo = Lot_No_Text.Text;
            string So_Me_String = So_Me_Text.Text;

            // Biến cần truyền cho DB
            int ID_Cong_Doan;
            int So_Me;

            // Validation dữ liệu
            if (LotNo == string.Empty || ItemNumber == string.Empty)
            {
                MessageBox.Show("Item Number hoặc số lô không hợp lệ!");
                return;
            }

            if(!int.TryParse(ID_Cong_Doan_String, out ID_Cong_Doan) || ID_Cong_Doan_String == string.Empty || !int.TryParse(So_Me_String, out So_Me) || So_Me_String == string.Empty)
            {
                MessageBox.Show("ID công đoạn hoặc số mẻ không hợp lệ!");
                return ;
            }

            // Lấy data cho header của công đoạn
            Catongtho_HeaderModel header = await _repo.Get_Header_catthoong(IdCongDoan: ID_Cong_Doan, ItemNumber: ItemNumber, LotNo: LotNo, So_Me: So_Me);
            
            // Validation dữ liệu nhận từ DB
            if (header == null)
            {
                MessageBox.Show($"Không tìm thấy dữ liệu cho công đoạn {ID_Cong_Doan}, Itemnumber: {ItemNumber}, Số lô: {LotNo} và số mẻ: {So_Me}");
                return;
            }

            // Dữ liệu mẫu cho Header in 1 lần
            //Catongtho_HeaderModel header = new Catongtho_HeaderModel
            //{
            //    Name_Congdoan = "Cắt thô ống/ abc",
            //    ID_Congdoan = "68",
            //    Code_Congdoan = "zzzzzzz",
            //    Category_Code = "123456G01",
            //    Lotno_Congdoan = "12316541",
            //    Batch_Number = "1",
            //    NG_Qty_Total = 1,
            //    OK_Qty_Total = 900
            //};

            // Lấy data cho bảng trong detail
            List<Catthoong_Row> rows = await _repo.Get_Cat_Ong_Tho(IdCongDoan: ID_Cong_Doan, ItemNumber: ItemNumber, LotNo: LotNo, So_Me: So_Me);

            // Lấy danh sách id_input từ list thu được
            var List_IdInput = rows.Select(x => x.idInput).Distinct().ToArray();

            // Lấy chi tiết lỗi
            List<Input_Error_Model> detail_error = await input_error_repo.Get_Detail_Error(idInputs: List_IdInput);

            var pivot = BuildPivotMap(detail_error); // idInput -> (tenLoi->qty)

            // Xử lý data trước khi bind Gom lỗi theo IdInput để lookup nhanh
            //var errByInput = detail_error
            //    .GroupBy(e => e.idInput)
            //    .ToDictionary(g => g.Key, g => g.ToList());

            // Gộp dữ liệu vào 1 list
            List<Catthoong_ReportRow> result = new List<Catthoong_ReportRow>(rows.Count);

            // Thêm từng dòng chi tiết lỗi cũng 1 idinput vào cùng nhau
            foreach (var m in rows)
            {
                // Tìm dictionary lỗi của idInput hiện tại
                pivot.TryGetValue(m.idInput, out var errsDict);
                // Tóm tắt lỗi


                // Tạo report-row

                var r = new Catthoong_ReportRow
                {
                    // copy main
                    idInput = m.idInput,
                    MaKT = m.MaKT,
                    TenMay_Ban = m.TenMay_Ban,
                    SLSudung = m.SLSudung,
                    StartTime = m.StartTime,
                    NguoiTT = m.NguoiTT,

                    val1 = m.val1,
                    val2 = m.val2,
                    val3 = m.val3,
                    val4 = m.val4,
                    val5 = m.val5,
                    val6 = m.val6,
                    val7 = m.val7,
                    val8 = m.val8,
                    val9 = m.val9,
                    val10 = m.val10,
                    val11 = m.val11,
                    val12 = m.val12,
                    val13 = m.val13,
                    val14 = m.val14,
                    val15 = m.val15,

                    Remark = m.Remark
                };
                // Gán 6 cột lỗi ngang theo map (KHÔNG dùng qtySum cho tất cả)
                SetKnownErrorColumns(r, errsDict);

                result.Add(r);
            }

            // Lấy dữ liệu cho bảng tiêu chuẩn
            List<Standard_Model> List_Standard_Report = await standard_repo.Get_Detail_Standard(idInputs: List_IdInput);
            // Nhóm tiêu chuẩn theo idInput để tra nhanh
            var stdByInput = List_Standard_Report
                .GroupBy(s => s.idInput)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var m in result)
            {
                if (stdByInput.TryGetValue(m.idInput, out var list))
                    m.Standards = list;                 // ~ 3 dòng cho mỗi lần nhập
                else
                    m.Standards = new List<Standard_Model>();
            }

            var rpt = new Testreport();
            rpt.ConfigureLayoutForCatongtho(result, List_Standard_Report, header, /*notePrintOnlyOnce*/ false);

            new ReportPrintTool(rpt).ShowRibbonPreviewDialog();
        }
    }
}
