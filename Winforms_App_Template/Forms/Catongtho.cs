using DevExpress.CodeParser;
using DevExpress.DataAccess.ObjectBinding;
using DevExpress.LookAndFeel;                        // UserLookAndFeel cho form Designer
using DevExpress.UIAutomation;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;                    // XtraReport, ReportDesignTool
using DevExpress.XtraReports.UserDesigner;          // XRDesignMdiController, XRDesignPanel, ReportState
using System.Data;
using System.IO;
using System.Text;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Database.Table;
using Winforms_App_Template.Loading;
using Winforms_App_Template.Report;
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
        private static void SetKnownErrorColumns(Que_Nong_Rows dest, Dictionary<string, int>? kv)
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

            // Lấy dữ liệu đầu vào
            string ID_Cong_Doan_String = ID_Cong_Doan_Text.Text;
            string ItemNumber = Item_Number_Text.Text;
            string LotNo = Lot_No_Text.Text;
            string So_Me_String = So_Me_Text.Text;

            // Biến cần truyền cho DB
            int ID_Cong_Doan;
            int So_Me;

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

            try
            {
                var rpt = await LoadingHelper.RunFunctionWithLoadingAsync<string, string, int, XtraReport>(
                    owner: this,
                    workMethod: Prepare_report,
                    arg1: ItemNumber,
                    arg2: LotNo,
                    arg3: So_Me,
                    caption: "Đang tải dữ liệu báo cáo ...");

                if (rpt != null)
                    new ReportPrintTool(rpt).ShowRibbonPreviewDialog();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(this, "Đã hủy xuất báo cáo.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Lỗi khi xuất báo cáo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Chuẩn bị dữ liệu để xuất PDF cho thao tác gia công que nong
        /// </summary>
        /// <param name="ItemNumber"></param>
        /// <param name="LotNo"></param>
        /// <param name="So_Me"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<XtraReport?> Prepare_report(string ItemNumber, string LotNo, int So_Me, CancellationToken ct)
        {

            // Khai báo công đoạn sử dụng trong report hiện tại
            // Đặt BandName/SubreportName đúng với layout bạn đã thiết kế & lưu trong DB
            var steps = new[]
            {
                new Step_Definition(68 , "Catongtho_Report" , "xrSubreport1" , "cd68_"),
                new Step_Definition(144, "Kiem_tra_ong_sau_cat_tho", "Kiem_tra_ong_sau_cat_tho_Standards_Subreport", "cd144_"),
                // thêm công đoạn khác (221, 305, …) tại đây:
                // new StepDefinition(221, "DR_CongDoan_221", "SR_Standards_221", "cd221_"),
            };

            // 1) Lấy dữ liệu tất cả công đoạn (song song có giới hạn)
            var blocks = await FetchAllStepsAsync(
                steps.Select(s => s.Id),
                ItemNumber, LotNo, So_Me,
                maxConcurrency: 3, // tuỳ DB, 3–4 là an toàn
                ct).ConfigureAwait(false);

            ct.ThrowIfCancellationRequested();

            // 2) Tạo report và nạp layout từ DB như cũ
            var rpt = new Testreport();
            rpt.DisplayName = "Quenong_Report";

            var updatedBy = Environment.UserName;
            var reportKey = ReportLayoutStore.GetKey(rpt);
            var store = new ReportLayoutStore(reportKey, updatedBy);
            await store.TryLoadAsync(rpt).ConfigureAwait(false);

            // Nạp layout DB cho subreports con
            foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))
            {
                if (sub.ReportSource is XtraReport child)
                    await new ReportLayoutStore(ReportLayoutStore.GetKey(child), updatedBy)
                        .TryLoadAsync(child).ConfigureAwait(false);
            }

            // 3) Chuẩn hoá expression nếu cần (như code bạn đang làm)
            NormalizeFieldPrefixes(rpt, "[Main].");
            foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))
                if (sub.ReportSource is XtraReport child)
                    NormalizeFieldPrefixes(child, "[Standards].");

            // 4) Kiểm tra field sai (tuỳ chọn)
            {
                var sb = new StringBuilder();
                var invalidMain = ReportLayoutHelpers.CollectInvalidFields(rpt, typeof(Que_Nong_Rows));
                if (invalidMain.Count > 0)
                {
                    sb.AppendLine("Các field KHÔNG tồn tại trong Que_Nong_Rows:");
                    foreach (var f in invalidMain) sb.Append(" - ").AppendLine(f);
                }
                var invalidSub = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))
                    if (sub.ReportSource is XtraReport child)
                        foreach (var f in ReportLayoutHelpers.CollectInvalidFields(child, typeof(Standard_Model)))
                            invalidSub.Add(f);
                if (invalidSub.Count > 0)
                {
                    sb.AppendLine("Các field KHÔNG tồn tại trong Standard_Model:");
                    foreach (var f in invalidSub) sb.Append(" - ").AppendLine(f);
                }
                if (sb.Length > 0)
                {
                    // CHÚ Ý: đừng Show() trong nền. Ở đây Prepare_report đã chạy trong nền,
                    // nên hãy chuyển về return “thông điệp cảnh báo” cho UI hiển thị sau await nếu bạn muốn.
                    // Để mẫu gọn, mình bỏ qua popup ngay trong hàm này.
                }
            }

            // 5) Bind từng công đoạn vào đúng Band/Subreport + đẩy Header với prefix
            foreach (var step in steps)
            {
                if (!blocks.TryGetValue(step.Id, out var block))
                    throw new InvalidOperationException($"Thiếu dữ liệu cho công đoạn {step.Id}.");

                // Bind data vào band + subreport
                BindStepToBand(rpt, block, step.BandName, step.StandardsSubreportName);

                // Đẩy header vào Parameters với prefix riêng cho công đoạn
                PushHeaderValuesWithPrefix(rpt, block.Header, step.HeaderParamPrefix);
            }

            ct.ThrowIfCancellationRequested();
            return rpt;
        }

        /// <summary>
        /// Truy vấn dữ liệu phần tiêu đề và dữ liệu bảng cho 1 công đoạn con
        /// </summary>
        /// <param name="idCongDoan"></param>
        /// <param name="itemNumber"></param>
        /// <param name="lotNo"></param>
        /// <param name="soMe"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<Data_Step_Model> FetchStepBlockAsync(int idCongDoan, string itemNumber, string lotNo, int soMe, CancellationToken ct)
        {
            // Chạy song song phần Header và Rows (độc lập)
            var headerTask = _repo.Get_Report_Header(IdCongDoan: idCongDoan, ItemNumber: itemNumber, LotNo: lotNo, So_Me: soMe, ct: ct);

            var rowsTask = _repo.Get_Detail_Table(IdCongDoan: idCongDoan, ItemNumber: itemNumber, LotNo: lotNo, So_Me: soMe, ct: ct);

            await Task.WhenAll(headerTask, rowsTask).ConfigureAwait(false);

            var header = headerTask.Result;
            var rows = rowsTask.Result ?? new List<New_Input_Row>();

            if (header is null)
                throw new InvalidOperationException(
                    $"Không tìm thấy HEADER (IdCongDoan={idCongDoan}, Item={itemNumber}, Lot={lotNo}, SoMe={soMe}).");

            ct.ThrowIfCancellationRequested();

            // Lấy idInput riêng của công đoạn này
            var idInputs = rows.Select(r => r.idInput).Distinct().ToArray();

            // Lấy lỗi & tiêu chuẩn: có thể chạy song song
            var errorsTask = input_error_repo.Get_Detail_Error(idInputs: idInputs, ct: ct);
            var stdTask = standard_repo.Get_Detail_Standard(idInputs: idInputs);

            await Task.WhenAll(errorsTask, stdTask).ConfigureAwait(false);

            var errorDetails = errorsTask.Result ?? new List<Input_Error_Model>();
            var standards = stdTask.Result ?? new List<Standard_Model>();

            // Pivot lỗi: idInput -> (tenLoi->qty)
            var pivot = BuildPivotMap(errorDetails);

            // Hợp nhất sang Row dành cho report
            var resultRows = new List<Que_Nong_Rows>(rows.Count);
            foreach (var m in rows)
            {
                pivot.TryGetValue(m.idInput, out var errsDict);

                var r = new Que_Nong_Rows
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
                    val16 = m.val16,
                    val17 = m.val17,
                    val18 = m.val18,
                    val19 = m.val19,
                    val20 = m.val20,
                    val21 = m.val21,
                    val22 = m.val22,
                    val23 = m.val23,
                    val24 = m.val24,
                    val25 = m.val25,
                    val26 = m.val26,
                    val27 = m.val27,
                    val28 = m.val28,
                    val29 = m.val29,
                    val30 = m.val30,
                    val31 = m.val31,
                    val32 = m.val32,
                    Remark = m.Remark
                };
                SetKnownErrorColumns(r, errsDict);
                resultRows.Add(r);
            }

            // Nhóm tiêu chuẩn theo idInput
            var stdByInput = standards
                .GroupBy(s => s.idInput)
                .ToDictionary(g => g.Key, g => g.ToList());

            ct.ThrowIfCancellationRequested();

            return new Data_Step_Model
            {
                Id = idCongDoan,
                Header = header,
                Rows = resultRows,
                StandardsByInput = stdByInput
            };
        }

        /// <summary>
        /// Xử lý nhiều công đoạn cùng lúc
        /// </summary>
        /// <param name="stepIds"></param>
        /// <param name="itemNumber"></param>
        /// <param name="lotNo"></param>
        /// <param name="soMe"></param>
        /// <param name="maxConcurrency"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<Dictionary<int, Data_Step_Model>> FetchAllStepsAsync(
                IEnumerable<int> stepIds, string itemNumber, string lotNo, int soMe,
                int maxConcurrency, CancellationToken ct)
        {
            var dict = new Dictionary<int, Data_Step_Model>();
            using var gate = new SemaphoreSlim(maxConcurrency);

            var tasks = stepIds.Select(async id =>
            {
                await gate.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    var block = await FetchStepBlockAsync(id, itemNumber, lotNo, soMe, ct).ConfigureAwait(false);
                    lock (dict) dict[id] = block;
                }
                finally { gate.Release(); }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return dict;
        }

        // ==========================
        // 5) Đẩy header theo prefix (tránh đè nhau giữa các công đoạn)
        //    Designer sẽ dùng Parameters.p_{prefix}{Property}
        //    ví dụ p_cd68_ItemNumber, p_cd144_ItemNumber
        // ==========================
        private static void PushHeaderValuesWithPrefix(XtraReport rpt, object header, string prefix)
        {
            if (header is null) return;

            foreach (var prop in header.GetType().GetProperties())
            {
                var p = rpt.Parameters[$"p_{prefix}{prop.Name}"];
                if (p != null)
                {
                    var val = prop.GetValue(header);
                    p.Value = val;
                }
            }
        }

        // ==========================
        // 6) Bind 1 công đoạn vào 1 band + subreport theo quy ước tên
        //    - bandName: DetailReportBand chứa lưới chính của công đoạn
        //    - subreportName: XRSubreport hiển thị Standards
        //    - idFieldName: tên property khoá (idInput)
        // ==========================
        private static void BindStepToBand(
            XtraReport rpt, Data_Step_Model block, string bandName, string subreportName, string idFieldName = "idInput")
        {
            // Tìm band của công đoạn
            var band = rpt.Bands
                .OfType<DevExpress.XtraReports.UI.DetailReportBand>()
                .FirstOrDefault(b => string.Equals(b.Name, bandName, StringComparison.Ordinal));

            if (band == null)
                throw new InvalidOperationException($"Không thấy band '{bandName}' trong layout.");

            // Bind rows cho band
            band.DataSource = block.Rows;
            band.DataMember = null; // dùng trực tiếp List<T>, không cần DataMember

            // Tìm subreport tiêu chuẩn trong band này
            var sub = band.FindControl(subreportName, true) as DevExpress.XtraReports.UI.XRSubreport;
            if (sub == null)
                return; // có layout không có sub standards -> bỏ qua

            // Mặc định: report con dùng Standard_Model
            if (sub.ReportSource is XtraReport child)
            {
                // Không bind cố định; feed theo từng dòng của band
                child.DataSource = Array.Empty<Standard_Model>();

                sub.BeforePrint += (_, __) =>
                {
                    // Lấy row hiện tại của band để tra idInput
                    var current = band.GetCurrentRow() as Que_Nong_Rows;
                    if (current != null &&
                        block.StandardsByInput.TryGetValue(current.idInput, out var list))
                    {
                        child.DataSource = list;
                        child.DataMember = null;
                    }
                    else
                    {
                        child.DataSource = Array.Empty<Standard_Model>();
                        child.DataMember = null;
                    }
                };
            }
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

        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                var tool = await LoadingHelper.RunFunctionWithLoadingAsync(
                    owner: this,
                    workMethod: LoadReportLayoutAsync,                   // <-- method group
                    arg: "Quenong_Report",                                 // reportKey
                    caption: "Đang tải thiết kế ..."
                     // gifOverride: Properties.Resources.loading_gif_khac // nếu muốn
                 );
                if (tool == null)
                {
                    // Lỗi khi tải layout
                    MessageBox.Show(this, "Không thể tải trang thiết kế báo cáo.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Mở Designer dạng MODAL: block cho đến khi người dùng đóng
                tool.ShowRibbonDesignerDialog(UserLookAndFeel.Default);       // Khác với ShowRibbonDesigner(): modal giúp “đóng → rồi save”
            }
            catch (OperationCanceledException)
            {
                // Người dùng bấm Hủy → không làm gì thêm (tuỳ ý)
                MessageBox.Show(this,"Hủy", "Người dùng hủy",
                    MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                // ❗️BÁO LỖI Ở UI THREAD (hợp lệ, KHÔNG cross-thread)
                MessageBox.Show(this, ex.Message, "Lỗi mở Designer/Nạp layout",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
            

        /// <summary>
        /// Tải trang thiết kế form cho từng công đoạn theo reportKey
        /// </summary>
        /// <param name="reportKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ReportDesignTool?> LoadReportLayoutAsync(string reportkey, CancellationToken ct)
        {
            try
            {
                // Khởi tạo report cần thiết kế
                XtraReport rpt = new Testreport();
                rpt.DisplayName = reportkey;                       // Đặt display name cho báo cáo này để lưu trữ vào DB

                // Khai báo class chịu trách nhiệm quản lý việc tải form, lưu form vào DB
                var updatedBy = Environment.UserName;
                var reportName = ReportLayoutStore.GetKey(rpt);    // Lấy key của report (dùng DisplayName nếu có, không thì lấy tên class)
                var store = new ReportLayoutStore(
                    reportName: reportName,
                    updatedBy: updatedBy             // Audit: ai là người “Save”
                );

                // Trước khi mở Designer – thử nạp layout mới nhất từ DB thông qua từ khóa DisplayName hoặc tên class của report được truyền vào
                await store.TryLoadAsync(rpt, ct: ct);                  // Nếu có trong DB → LoadLayoutFromXml; nếu không → giữ layout mặc định

                // Nạp layout từ DB cho từng Subreport con trước khi mở Designer
                foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))
                {
                    var child = sub.ReportSource as XtraReport;
                    if (child == null) continue;

                    // Nếu chưa set, đặt DisplayName mặc định = tên class
                    if (string.IsNullOrWhiteSpace(child.DisplayName))
                        child.DisplayName = child.GetType().Name;

                    await new ReportLayoutStore(ReportLayoutStore.GetKey(child), updatedBy)
                        .TryLoadAsync(child,  ct: ct);
                }

                // Chuẩn bị whitelist → DataTable schema cho từng band để gắn vào datasource, dùng cho design
                var bandSchemas = new Dictionary<string, DataTable>
                {
                    // Band "Catongtho_Report": Sử dụng bảng Catthoong và đặt tên hiển thị là Cat_tho_ong
                    ["Catongtho_Report"] = FieldWhitelistRegistry.Catthoong.ToDesignSchema("Cat_tho_ong"),

                    // Band "Kiem_tra_ong_sau_cat_tho":
                     ["Kiem_tra_ong_sau_cat_tho"] = FieldWhitelistRegistry.Kiemtrasaucattho.ToDesignSchema("Kiemtrasaucattho"),
                };

                // Gắn schema cho từng band theo tên
                DesignSchema.AttachBandSchemas(rpt, bandSchemas);

                // Tạo ReportDesignTool để mở End-User Designer
                var tool = new ReportDesignTool(rpt);           // tool chứa form Designer (Ribbon) và controller MDI
                var form = tool.DesignRibbonForm;               // IDesignForm (XRDesignRibbonForm implements IDesignForm) chính là cửa sổ Designer (bản Ribbon).
                var controller = form.DesignMdiController;      // XRDesignMdiController: trung tâm điều phối các "DesignPanel" (tab) trong Designer

                // 6) SUBREPORT SCHEMA: chỉ khi người dùng mở tab subreport → mới gắn schema phù hợp cho subreport đó
                DesignSchema.WireSubreportSchemaOnDemandByBand(
                    controller: controller,
                    mainReport: rpt,
                    subSchemaFactory: sub =>
                    {
                        // Xác định sub này thuộc band nào
                        var ownerBand = DesignSchema.FindOwningDetailReportBand(rpt, sub);

                        // Nếu sub nằm trong band "Catongtho_Report" → dùng schema SUB mặc định
                        if (ownerBand != null && string.Equals(ownerBand.Name, "Catongtho_Report", StringComparison.Ordinal))
                            return FieldWhitelistRegistry.Standard_Catthoong.ToDesignSchema("StdRows");

                        // Nếu bạn có band khác với schema khác, xử lý tại đây:
                         if (ownerBand?.Name == "Kiem_tra_ong_sau_cat_tho") return FieldWhitelistRegistry.Kiemtrasaucattho_Standard.ToDesignSchema("StdRows2");

                        // Mặc định: vẫn trả schema Sub
                        return FieldWhitelistRegistry.Standard_Catthoong.ToDesignSchema("StdRows");
                    });

                // đổi nhãn trong whitelist trước khi mở:
                // FieldWhitelistRegistry.Main.SetLabel("NguoiTT", "Người thao tác (VN)");
                // FieldWhitelistRegistry.Main.Add("val1", typeof(string), "Ống dài sử dụng");
                // FieldWhitelistRegistry.Main.Remove("Remark");

                // Ghi đè sự kiện DesignPanelLoaded để gắn SaveCommandHandler cho từng panel khi nó được tạo
                // Mỗi panel sẽ tương ứng với 1 tab thiết kế (report chính hoặc subreport) trong Designer
                // Ghi đè sự kiện mỗi khi người dùng ấn Save hoặc Ctrl+S trong Designer
                controller.DesignPanelLoaded += (sender, e) =>
                {
                    // Lấy panel đang hiển thị (Theo tài liệu chính thức từ DevXpress)
                    var panel = (XRDesignPanel)sender;
                    if (panel == null) return;

                    // Đặt nhãn hiển thị
                    var currentKey = ReportLayoutStore.GetKey(panel.Report);
                    panel.FileName = $"{currentKey} (DB + Local)";

                    // Tránh gắn trùng nhiều lần (đánh dấu bằng Tag)
                    if (panel.Tag as string == "SaveHandlerWired") return;

                    // Tạo store riêng cho panel này (vì có thể là subreport)
                    var storeForThisPanel = new ReportLayoutStore(currentKey, updatedBy);
                    panel.AddCommandHandler(new SaveCommandHandler(panel, storeForThisPanel, currentKey));

                    panel.Tag = "SaveHandlerWired"; // đánh dấu đã wire
                };

                // Khai báo danh sách parameter cần cho band "Catongtho_Report"
                var headerParams = new[]
                {
                    new ParameterSpec("Name_Congdoan",  typeof(string),  "Tên công đoạn"),
                    new ParameterSpec("ID_Congdoan",    typeof(string),  "ID công đoạn"),
                    new ParameterSpec("Code_Congdoan",  typeof(string),  "Mã công đoạn"),
                    new ParameterSpec("Category_Code",  typeof(string),  "Mã sản phẩm"),
                    new ParameterSpec("Lotno_Congdoan", typeof(string),  "Số lô"),
                    new ParameterSpec("Batch_Number",   typeof(string),  "Số mẻ"),
                };

                //    Tạo các parameter dạng p_{Band}_{Param} ở cấp REPORT:
                BandParameterHelper.EnsureParametersForBand(
                    rpt,
                    bandName: "Catongtho_Report",
                    specs: headerParams,
                    visible: false);

                return tool;

            }
            catch (Exception ex)
            {
                // BẮT MỌI LỖI ĐỂ KHÔNG VĂNG APP
                MessageBox.Show(this, ex.Message, "Lỗi mở Designer/Nạp layout",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }   
        }
    }
}
