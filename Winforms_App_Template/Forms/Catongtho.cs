using DevExpress.DataAccess.ObjectBinding;
using DevExpress.LookAndFeel;                        // UserLookAndFeel cho form Designer
using DevExpress.XtraReports.UI;                    // XtraReport, ReportDesignTool
using DevExpress.XtraReports.UserDesigner;          // XRDesignMdiController, XRDesignPanel, ReportState
using System.IO;
using System.Text;
using System.Threading.Tasks;                       // Task/async
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Database.Table;
using Winforms_App_Template.Utils;

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

        private async void Export_Document_Button_Click(object sender, EventArgs e)
        {
            // Hiển thị loading
            ToggleUiLoading(true);

            // Lấy dữ liệu đầu vào
            string ID_Cong_Doan_String = ID_Cong_Doan_Text.Text;
            string ItemNumber = Item_Number_Text.Text;
            string LotNo = Lot_No_Text.Text;
            string So_Me_String = So_Me_Text.Text;

            // Biến cần truyền cho DB
            int ID_Cong_Doan;
            int So_Me;

            // Token hủy tải dữ liệu
            using var cts = new CancellationTokenSource();
            _cts = cts;

            // Validation dữ liệu
            if (string.IsNullOrWhiteSpace(ItemNumber) || string.IsNullOrWhiteSpace(LotNo))
            {
                MessageBox.Show("Item Number hoặc Số lô không hợp lệ!");
                return; // dừng sớm
            }

            if (!int.TryParse(ID_Cong_Doan_String, out ID_Cong_Doan) || ID_Cong_Doan_String == string.Empty || !int.TryParse(So_Me_String, out So_Me) || So_Me_String == string.Empty)
            {
                MessageBox.Show("ID công đoạn hoặc số mẻ không hợp lệ!");
                return;
            }

            // Lấy data cho header của công đoạn
            Catongtho_HeaderModel? header = null;
            try
            {
                header = await _repo.Get_Header_catthoong(IdCongDoan: ID_Cong_Doan, ItemNumber: ItemNumber, LotNo: LotNo, So_Me: So_Me, ct: cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Hủy quá trình tải dữ liệu
                MessageBox.Show("Quá trình tải dữ liệu đã bị hủy.");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validation dữ liệu nhận từ DB
            if (header == null)
            {
                MessageBox.Show($"Không tìm thấy dữ liệu cho công đoạn {ID_Cong_Doan}, Itemnumber: {ItemNumber}, Số lô: {LotNo} và số mẻ: {So_Me}");
                // Ẩn loading và mở khóa các nút
                ToggleUiLoading(false);
                _cts?.Dispose();
                _cts = null;
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
            List<Catthoong_Row>? rows = null;
            try
            {
                rows = await _repo.Get_Cat_Ong_Tho(IdCongDoan: ID_Cong_Doan, ItemNumber: ItemNumber, LotNo: LotNo, So_Me: So_Me, ct: cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Hủy quá trình tải dữ liệu
                MessageBox.Show("Quá trình tải dữ liệu đã bị hủy.");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lấy danh sách id_input từ list thu được
            var List_IdInput = rows.Select(x => x.idInput).Distinct().ToArray();

            // Lấy chi tiết lỗi
            List<Input_Error_Model>? detail_error = null;
            try
            {
                detail_error = await input_error_repo.Get_Detail_Error(idInputs: List_IdInput, ct: cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Hủy quá trình tải dữ liệu
                MessageBox.Show("Quá trình tải dữ liệu đã bị hủy.");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
                // Gán 6 cột lỗi ngang theo map
                SetKnownErrorColumns(r, errsDict);

                result.Add(r);
            }

            // Lấy dữ liệu cho bảng tiêu chuẩn
            List<Standard_Model>? List_Standard_Report = null;
            try
            {
                List_Standard_Report = await standard_repo.Get_Detail_Standard(idInputs: List_IdInput);
            }
            catch (OperationCanceledException)
            {
                // Hủy quá trình tải dữ liệu
                MessageBox.Show("Quá trình tải dữ liệu đã bị hủy.");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Nhóm tiêu chuẩn theo idInput để tra nhanh
            var stdByInput = List_Standard_Report
                .GroupBy(s => s.idInput)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Gán tiêu chuẩn vào từng dòng của datasource, nếu sử dụng thì cần khai báo Datamember vì nó là List <T> trong 1 List<T> khác
            foreach (var m in result)
            {
                if (stdByInput.TryGetValue(m.idInput, out var list))
                    m.Standards = list;                 // ~ 3 dòng cho mỗi lần nhập
                else
                    m.Standards = new List<Standard_Model>();
            }
            // Ẩn loading và mở khóa các nút
            ToggleUiLoading(false);
            _cts?.Dispose();
            _cts = null;

            // Tạo mẫu báo cáo mới
            var rpt = new Testreport();
            rpt.DisplayName = "Catongtho_Main";          // key ổn định (đừng thay đổi)

            // Truy vấn Layout mới nhất từ DB
            var updatedBy = Environment.UserName;
            var reportKey = ReportLayoutStore.GetKey(rpt);
            var store = new ReportLayoutStore(reportKey, updatedBy);

            // Load layout mới nhất
            await store.TryLoadAsync(rpt);

            // ==== CHUẨN HOÁ EXPRESSION + KIỂM TRA FIELD ( Khi người dùng thêm expression trong design) ====

            // Loại bỏ prefix [Main]. (nếu người thiết kế vô tình để DataMember="Main" lúc design)
            NormalizeFieldPrefixes(rpt, "[Main].");                // report chính
            foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))          // subreport trong report chính
            {
                if (sub.ReportSource is XtraReport child)
                    NormalizeFieldPrefixes(child, "[Standards]."); // phòng khi sub có DataMember="Standards" lúc design
            }

            // Kiểm tra các field name xuất hiện trong ExpressionBindings có khớp model runtime không
            var sb = new StringBuilder(); // Chứa các fieldname ko phù hợp

            // Kiểm tra đối với report chính
            var invalidMain = ReportLayoutHelpers.CollectInvalidFields(rpt, typeof(Catthoong_ReportRow));          // field dùng ở main

            if (invalidMain.Count > 0)
            {
                sb.AppendLine("Các field KHÔNG tồn tại trong Catthoong_ReportRow:");
                foreach (var f in invalidMain)
                    sb.Append(" - ").AppendLine(f);
            }
            // Kiểm tra các Subreport có thể có trong report chính
            var invalidSub = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // Duyệt tất cả các subreport và kiểm tra
            foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))
            {
                if (sub.ReportSource is XtraReport child)
                    foreach (var f in ReportLayoutHelpers.CollectInvalidFields(child, typeof(Standard_Model)))
                        invalidSub.Add(f);
            }

            if (invalidSub.Count > 0)
            {
                sb.AppendLine("Các field KHÔNG tồn tại trong Standard_Model:");
                foreach (var f in invalidSub)
                    sb.Append(" - ").AppendLine(f);
            }

            // Nếu có field “lạ” → cảnh báo (Cảnh báo xong vẫn mở preview để sửa)
            if (sb.Length > 0)
            {
                MessageBox.Show(this, sb.ToString(),
                    "Cảnh báo field không khớp model",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            // ==== END CHUẨN HOÁ + KIỂM TRA FIELD ====

            // ==== ĐỔ THAM SỐ HEADER VÀO REPORT (NẾU TRÙNG TÊN bINDING TRONG DESIGN VÀ MODEL) ====

            // Tự động gán Parameter.Value nếu tên Parameter trùng với property của headerData (HeaderData dùng parameter)
            ReportLayoutHelpers.ApplyParametersFromObject(rpt, header);

            // ==== BIND RUNTIME DATASOURCE + SUBREPORT MAPPING ====

            ReportLayoutHelpers.BindForRuntime(rpt, result, stdByInput, idFieldName: "idInput"); // gán List<Catthoong_ReportRow> + map subreport theo idInput
            ReportLayoutHelpers.PushHeaderValues(rpt, header);
            // Cấu hình Dữ liệu cho báo cáo
            //rpt.ConfigureLayoutForCatongtho(result, List_Standard_Report, header, /*notePrintOnlyOnce*/ false);
            // Hiển thị giao diện báo cáo
            new ReportPrintTool(rpt).ShowRibbonPreviewDialog();
        }

        /// <summary>
        /// Loại bỏ prefix trong Expression kiểu "[Main].Field" → "Field".
        /// Duyệt toàn bộ controls và mọi ExpressionBindings.
        /// </summary>
        private static void NormalizeFieldPrefixes(XtraReport rpt, string prefixToRemove)
        {
            // Kiểm tra đầu vào
            if (rpt == null)
                throw new ArgumentNullException(nameof(rpt), "Report không được null.");

            if (string.IsNullOrWhiteSpace(prefixToRemove))
                return;

            // Loại bỏ dấu "." ở cuối prefix nếu có (để tránh Replace thừa)
            prefixToRemove = prefixToRemove.Trim();
            if (prefixToRemove.EndsWith("."))
                prefixToRemove = prefixToRemove[..^1];

            // Duyệt tất cả các band trong 1 report
            foreach (Band band in rpt.Bands)
            {
                // Nếu band đấy không có control nào thì bảo qua, xử lý các band tiếp theo
                if (band?.Controls == null || band.Controls.Count == 0)
                    continue;

                // Tiếp tục duyệt từng control trong 1 band
                foreach (XRControl control in ReportLayoutHelpers.EnumerateControls(band.Controls))
                {
                    // Nếu ko có control hoặc control đấy đang không binding thì không cần loai tiền tố
                    if (control == null || control.ExpressionBindings == null)
                        continue;

                    // Duyệt tất cả các binding của cotrol này (1 control có thể có nhiều binding)
                    foreach (var binding in control.ExpressionBindings)
                    {
                        if (string.IsNullOrWhiteSpace(binding.Expression))
                            continue;

                        // So sánh không phân biệt hoa thường & có thể tránh Replace lỗi
                        if (binding.Expression.Contains(prefixToRemove + ".", StringComparison.Ordinal))
                        {
                            binding.Expression = binding.Expression.Replace(
                                prefixToRemove + ".",
                                "",
                                StringComparison.Ordinal
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Hủy truy vấn dữ liệu nếu đang chạy và hủy xuất báo cáo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Export_Document_Click(object sender, EventArgs e)
        {
            // Hủy truy vấn dữ liệu nếu đang chạy
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            // Hủy xuất báo cáo
            MessageBox.Show("Đã hủy xuất báo cáo.");

        }

        /// <summary>
        /// Ẩn hiện biểu tượng loading và khóa nút bấm tương ứng
        /// </summary>
        /// <param name="loading"></param>
        private void ToggleUiLoading(bool loading)
        {
            Export_Document_Button.Enabled = !loading;
            Cancel_Export_Document.Enabled = loading;
            UseWaitCursor = loading;
        }

        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                var rpt = new Testreport();                     // Instance report ban đầu (layout mặc định/embedded khi không thể lấy bản mới nhất từ máy chủ)
                rpt.DisplayName = "Catongtho_Main";             // key ổn định cho report chính

                // Khai báo class chịu trách nhiệm quản lý việc tải form mới, lưu form vào DB
                var updatedBy = Environment.UserName;

                var reportName = ReportLayoutStore.GetKey(rpt);   // Lấy key ổn định cho report (dùng DisplayName nếu có, không thì lấy tên class)
                var store = new ReportLayoutStore(
                    reportName: reportName,
                    updatedBy: updatedBy             // Audit: ai là người “Save”
                );

                // Trước khi mở Designer – thử nạp layout mới nhất từ DB 
                await store.TryLoadAsync(rpt);                  // Nếu có trong DB → LoadLayoutFromXml; nếu không → giữ layout mặc định

                // Nạp layout DB cho từng Subreport con trước khi mở Designer
                foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))
                {
                    var child = sub.ReportSource as XtraReport;
                    if (child == null) continue;

                    // Nếu chưa set, đặt DisplayName mặc định = tên class
                    if (string.IsNullOrWhiteSpace(child.DisplayName))
                        child.DisplayName = child.GetType().Name;

                    await new ReportLayoutStore( ReportLayoutStore.GetKey(child), updatedBy)
                        .TryLoadAsync(child);
                }

                // Tạo ReportDesignTool để mở End-User Designer
                var tool = new ReportDesignTool(rpt);           // tool chứa form Designer (Ribbon) và controller MDI

                // Lấy IDesignForm & MDI Controller (mọi event/active panel nằm trên controller)
                var form = tool.DesignRibbonForm;               // IDesignForm (XRDesignRibbonForm implements IDesignForm) chính là cửa sổ Designer (bản Ribbon).
                var controller = form.DesignMdiController;      // XRDesignMdiController: trung tâm điều phối các "DesignPanel" (tab) trong Designer

                //XRDesignPanel? openedPanel = null;                     // Giữ tham chiếu panel đã mở (để auto-save sau)

                // Khi một DesignPanel MỚI được tạo (Designer mở report)
                // KHI PANEL ĐƯỢC TẠO XONG → GẮN HANDLER LÊN CHÍNH PANEL ẤY
                controller.DesignPanelLoaded += (sender, e) =>
                {
                    // LẤY PANEL ĐÚNG CÁCH
                    var panel = (XRDesignPanel)sender;
                    if (panel == null) return;

                    // Đặt nhãn hiển thị
                    var currentKey = ReportLayoutStore.GetKey(panel.Report);
                    panel.FileName = $"{currentKey} (DB + Local)";

                    // Tránh gắn trùng nhiều lần (đánh dấu bằng Tag)
                    if (panel.Tag as string == "SaveHandlerWired") return;

                    var storeForThisPanel = new ReportLayoutStore(currentKey, updatedBy);
                    panel.AddCommandHandler(new SaveCommandHandler(panel, storeForThisPanel, currentKey));

                    panel.Tag = "SaveHandlerWired"; // đánh dấu đã wire
                };

                // Gán các FieldName vào Design để có thể chọn các thuộc tính Expression
                //ReportDesignSchemaHelper.AttachDesignSchema(rpt);  // <— Gọi 1 dòng trước khi mở Designer
                ReportDesignSchemaHelper.AttachDesignSchema(rpt, attachHeaderParameters: true, headerModelType: typeof(Catongtho_HeaderModel));
                // Mở Designer dạng MODAL: block cho đến khi người dùng đóng
                tool.ShowRibbonDesignerDialog(UserLookAndFeel.Default);       // Khác với ShowRibbonDesigner(): modal giúp “đóng → rồi save”

                // Sau khi Designer đóng: nếu còn thay đổi mà user QUÊN bấm Save → auto-save (Chọn ko save cũng là yes luôn)
                var activePanel = controller.ActiveDesignPanel; // Lấy panel đang/đã active qua MDI controller (không cần cast form) 
                if (activePanel != null && activePanel.ReportState == ReportState.Changed) // KHÔNG dùng Modified; đúng là Changed
                {
                    string? Program_Name = Application.ProductName;
                    if (Program_Name == string.Empty || Program_Name == null) Program_Name = "Default";

                    // a) Local
                    var localPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        Program_Name, "Reports", $"{reportName}.repx");
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
                    rpt.SaveLayoutToXml(localPath);

                    // b) DB
                    await store.SaveAsync(activePanel.Report);  // Lưu vào DB để lần sau mọi máy cùng nhận bản mới
                }
            }
            catch (Exception ex)
            {
                // BẮT MỌI LỖI ĐỂ KHÔNG VĂNG APP
                MessageBox.Show(this, ex.Message, "Lỗi mở Designer/Nạp layout",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            //ReportDesignTool designTool = new ReportDesignTool(new Testreport());

            //// Mở cửa sổ thiết kế form nhưng chương trình chính vẫn chạy bình thường
            ////designTool.ShowRibbonDesigner();

            //// Mở cửa sổ thiết kế form (dạng hộp thoại). Chặn luồng chương trình chính cho đến khi người dùng đóng cửa sổ design
            //designTool.ShowRibbonDesignerDialog(UserLookAndFeel.Default);
        }
    }
}
