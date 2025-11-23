using DevExpress.LookAndFeel;                        // UserLookAndFeel cho form Designer
using DevExpress.XtraReports.UI;                    // XtraReport, ReportDesignTool
using DevExpress.XtraReports.UserDesigner;          // XRDesignMdiController, XRDesignPanel, ReportState
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private CancellationTokenSource? _cts;                   // Hủy tải dữ liệu

        private MayBan_Table _mayRepo;                           // repo danh mục máy
        private NewInputs_Table get_detail_table_repo;           // repo cho bảng NewInput
        private readonly Input_Error_Table input_error_repo;     // Repository cho bảng input_Error
        private readonly Standard_Table standard_repo;           // Repository cho bảng tiêu chuẩn

        public Catongtho(DbExecutor? db = null)
        {
            InitializeComponent();

            var executor = db ?? new DbExecutor();
            get_detail_table_repo = new NewInputs_Table(executor);
            _mayRepo = new MayBan_Table(executor);
            input_error_repo = new Input_Error_Table(executor);
            standard_repo = new Standard_Table(executor);

        }

        /// <summary>
        /// Xuất dữ liệu báo cáo sang định dạng A3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Export_Document_Button_Click(object sender, EventArgs e)
        {
            //// Tạo ƯhiteList bằng hàm và các trường trong DB
            //using var cts = new CancellationTokenSource();
            //var whitelistBuilder = new Auto_Build_FieldWhiteList(); // Create an instance
            //var whitelist = await whitelistBuilder.GetWhitelistsForFormsAsync([71], ct: cts.Token);

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

            // Truy vấn dữ liệu cho form và tạo báo cáo
            try
            {
                var rpt = await LoadingHelper.RunFunctionWithLoadingAsync<string, string, int, XtraReport>(
                    owner: this,
                    workMethod: Prepare_report,
                    arg1: ItemNumber,
                    arg2: LotNo,
                    arg3: So_Me,
                    caption: "Đang tải dữ liệu báo cáo ...");

                // Nếu tồn tại báo cáo thì hiển thị nó ra
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
            // Đặt BandName/SubreportName/Tham số parameter đúng với layout đã thiết kế & lưu trong DB
            var steps = new[]
            {
                new Step_Definition(68 , "Catongtho_Report" , "Catongtho_Report_"),
                new Step_Definition(144, "Kiem_tra_ong_sau_cat_tho", "Kiem_tra_ong_sau_cat_tho_"),
                new Step_Definition(70, "Cam_chot", "Cam_chot_dkm_", true),
                new Step_Definition(71, "Dap_chuoi_cat_dinh_muc", "Dap_chuoi_cat_dinh_muc_dkm_", true),
                // thêm công đoạn khác (221, 305, …) tại đây:
                new Step_Definition(175, "Tu_dong_lap_rap_que_nong", "Tu_dong_lap_rap_que_nong_", true),
                new Step_Definition(72, "Gia_cong_dau_mut_v1_5", "Gia_cong_dau_mut_v1_5_", true),
                new Step_Definition(73, "Rua_dau_mut_que_nong", "Rua_dau_mut_que_nong_"),
                new Step_Definition(74, "Kiem_tra_ngoai_quan", "Kiem_tra_ngoai_quan_"),
                new Step_Definition(75, "Xu_ly_silicon", "Xu_ly_silicon_"),
                new Step_Definition(76, "Kiem_tra_lan_cuoi", "Kiem_tra_lan_cuoi_"),
                new Step_Definition(77, "Tong_ket", "Tong_ket_"),
            };

            // ===== KHỞI TẠO SERVICE DỮ LIỆU =====
            var dataSvc = new ReportDataPreparer();

            // Lấy dữ liệu tất cả công đoạn (song song)
            var blocks = await dataSvc.FetchAllStepsAsync(
                steps: steps,
                itemNumber: ItemNumber,
                lotNo: LotNo,
                soMe: So_Me,
                maxConcurrency: 3,           // 3–4 là hợp lý với 7–8 công đoạn
                ct: ct);  //.ConfigureAwait(false);

            ct.ThrowIfCancellationRequested();

            // Tạo report và nạp layout từ DB
            var rpt = new Testreport();
            rpt.DisplayName = "Quenong_Report";

            var updatedBy = Environment.UserName;
            var reportKey = ReportLayoutStore.GetKey(rpt);
            var store = new ReportLayoutStore(reportKey, updatedBy);

            // Tải form mới nhất từ DB nếu có
            await store.TryLoadAsync(rpt).ConfigureAwait(false);

            // Nạp layout DB cho subreports con
            foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))
            {
                if (sub.ReportSource is XtraReport child)
                    await new ReportLayoutStore(ReportLayoutStore.GetKey(child), updatedBy)
                        .TryLoadAsync(child).ConfigureAwait(false);
            }

            // Chuẩn hoá expression 
            //NormalizeFieldPrefixes(rpt, "[Main].");
            //foreach (var sub in ReportLayoutHelpers.EnumerateSubreports(rpt))
            //    if (sub.ReportSource is XtraReport child)
            //        NormalizeFieldPrefixes(child, "[Standards].");

            // Kiểm tra field sai (Đang không hoạt động)
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

            // Bind từng công đoạn vào đúng Band/Subreport + đẩy Header với prefix
            foreach (var step in steps)
            {
                if (!blocks.TryGetValue(step.Id, out var block))
                    throw new InvalidOperationException($"Thiếu dữ liệu cho công đoạn {step.Id}.");

                // Bind data vào band + subreport
                BindStepToBand(rpt, block, step.BandName, step.Isdkm);

                // Đẩy header vào Parameters với prefix riêng cho công đoạn
                PushHeaderValuesWithPrefix(rpt, block.Header, step.HeaderParamPrefix);
            }

            ct.ThrowIfCancellationRequested();

            // Kiểm tra xem mẻ hiện tại có phải "mẻ cuối lô" hay không thông qua công đoạn 77 - Tong_ket
            var isLastBatch = ReportDataPreparer. IsLastLotBatch(blocks);

            // Tìm subreport tổng kết trong layout
            var subFinal = rpt.FindControl("Summary_report", ignoreCase: true) as XRSubreport;

            // Nếu không tìm thấy control -> bỏ qua
            if (subFinal != null)
            {
                if (!isLastBatch)
                {
                    // Không phải mẻ cuối lô:
                    //  -> ẨN hẳn subreport, không hiển thị gì
                    subFinal.Visible = false;
                }
                else
                {
                    // Là mẻ cuối lô:
                    //  -> Tính DataTable tổng kết và gắn vào ReportSource của subreport

                    // itemNumber / lotNo chính là arg của Prepare_report
                    var lotSummaryList = await get_detail_table_repo.Get_Lot_Summary(ItemNumber, LotNo, ct);


                    if (subFinal.ReportSource is XtraReport summaryReport)
                    {
                        // Gán datasource cho report con Summary_report
                        summaryReport.DataSource = lotSummaryList;
                        summaryReport.DataMember = null;
                    }
                }
            }

            return rpt;
        }
        
        // ==========================
        //  Đẩy header theo prefix (tránh đè nhau giữa các công đoạn)
        //  Designer sẽ dùng Parameters.p_{prefix}{Property}
        //  ví dụ p_cd68_ItemNumber, p_cd144_ItemNumber
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
        // Bind 1 công đoạn vào 1 band + (optional) band điều kiện máy (_dkm)
        //    - bandName      : DetailReportBand chứa lưới chính của công đoạn
        //    - bandName_dkm  : DetailReportBand chứa điều kiện máy (nếu is_dkm = true)
        //    - is_dkm        : cho biết công đoạn này có thêm band điều kiện máy hay không
        //    - idFieldName   : tên property khoá (idInput) trên row (Que_Nong_Rows / Dieu_kien_may_Model)
        // ==========================
        private static void BindStepToBand(
            XtraReport rpt,
            Data_Step_Model block,
            string bandName,
            bool is_dkm,
            string idFieldName = "idInput")
        {
            if (rpt == null)
                throw new ArgumentNullException(nameof(rpt));

            if (block == null)
                throw new ArgumentNullException(nameof(block));

            // 1. Tìm band chi tiết chính của công đoạn theo tên
            var mainBand = ReportLayoutHelpers.FindDetailReportBandOrThrow(rpt, bandName);

            // 2. Nếu không có dữ liệu hàng nào cho công đoạn này
            if (block.Rows == null || block.Rows.Count == 0)
            {
                // Ẩn band chính
                mainBand.Visible = false;

                // Nếu có band điều kiện máy, cũng ẩn luôn
                if (is_dkm)
                {
                    var dkmBandName = bandName + "_dkm";
                    var dkmBand = ReportLayoutHelpers.TryFindDetailReportBand(rpt, dkmBandName);
                    if (dkmBand != null)
                        dkmBand.Visible = false;
                }

                // Không bind gì thêm
                return;
            }

            // 3. Có dữ liệu -> hiển thị band chính + gắn DataSource
            mainBand.Visible = true;
            mainBand.DataSource = block.Rows; // List<Que_Nong_Rows>
            mainBand.DataMember = null;       // dùng trực tiếp List<T>

            // 4. Trong band chính: gắn dữ liệu cho tất cả subreport "standard"
            //    - Lấy dữ liệu từ block.StandardsByInput (Dictionary<int, List<Standard_Model>>)
            if (block.StandardsByInput != null)
            {
                foreach (var sub in ReportLayoutHelpers.EnumerateSubreportsInBand(mainBand))
                {
                    binding_data_source_for_standard_report(
                        parentBand: mainBand,
                        xRSubreport: sub,
                        subreport_name: "standard",                // từ khoá cần tìm trong Name
                        dataByInput: block.StandardsByInput,       // Dictionary<int, List<Standard_Model>>
                        idFieldName: idFieldName                   // vd: "idInput"
                                                                   // không cần dummy & normalize → null
                    );
                }
            }

            // 5. Nếu công đoạn có điều kiện máy (is_dkm = true)
            //    - Xử lý subreport tên có "dkm" trong band chính
            //    - Dùng chung helper để tránh lặp code
            if (is_dkm && block.DkmByInput != null)
            {
                foreach (var sub in ReportLayoutHelpers.EnumerateSubreportsInBand(mainBand))
                {
                    // Ở đây ta coi "dkm" là keyword trong Name của subreport điều kiện máy
                    binding_data_source_for_standard_report(
                        parentBand: mainBand,
                        xRSubreport: sub,
                        subreport_name: "dkm",                     // từ khoá "dkm"
                        dataByInput: block.DkmByInput,             // Dictionary<int, List<Dieu_kien_may_Model>>
                        idFieldName: idFieldName,
                        // dummyFactory: tạo 1 dòng điều kiện máy giả khi không có data
                        dummyFactory: (int id) =>
                        {
                            var dummy = new Dieu_kien_may_Model
                            {
                                idInput = id
                                // Các field string còn lại sẽ được normalize ở dưới
                            };

                            // Ép tất cả string null/rỗng => "N/A"
                            // onlyValPrefix = false → xử lý TẤT CẢ property string, không chỉ val1..val54
                            ReportDataPreparer.NormalizeTrueFalseStringValues(
                                dummy,
                                onlyValPrefix: false,
                                "Ly_do_kiem_tra");

                            return dummy;
                        },
                        // normalizeAction: không cần thêm gì nữa vì đã normalize trong dummyFactory
                        normalizeAction: null
                    );
                }
            }

            // 6. Nếu công đoạn có thêm band điều kiện máy (bandName + "_dkm")
            //    - band này hiển thị riêng một lưới điều kiện máy
            //    - Ở đây ví dụ: DataSource = block.dkm (List<Dieu_kien_may_Model>)
            if (is_dkm)
            {
                var dkmBandName = bandName + "_dkm";
                var dkmBand = ReportLayoutHelpers.TryFindDetailReportBand(rpt, dkmBandName);

                if (dkmBand != null)
                {
                    // Nếu không có dữ liệu điều kiện máy -> ẩn band_dkm
                    if (block.dkm == null || block.dkm.Count == 0)
                    {
                        dkmBand.Visible = false;
                    }
                    else
                    {
                        // Có dữ liệu điều kiện máy -> bind vào band_dkm
                        dkmBand.Visible = true;
                        dkmBand.DataSource = block.dkm;   // List<Dieu_kien_may_Model>
                        dkmBand.DataMember = null;

                        // Trong band điều kiện máy nếu cũng có subreport "standard"
                        // thì ta cũng dùng chung helper để bind theo StandardsByInput
                        if (block.StandardsByInput != null)
                        {
                            foreach (var sub in ReportLayoutHelpers.EnumerateSubreportsInBand(dkmBand))
                            {
                                binding_data_source_for_standard_report(
                                    parentBand: dkmBand,
                                    xRSubreport: sub,
                                    subreport_name: "standard",
                                    dataByInput: block.StandardsByInput,  // vẫn tra theo idInput
                                    idFieldName: idFieldName
                                // không cần dummy & normalize → null
                                );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lấy giá trị khoá (vd: idInput) từ 1 row bất kỳ bằng reflection.
        /// Dùng được cho cả Que_Nong_Rows, Dieu_kien_may_Model... miễn là có property đó.
        /// </summary>
        private static int GetIdFieldValue(object row, string idFieldName)
        {
            if (row == null || string.IsNullOrWhiteSpace(idFieldName))
                return 0;

            var type = row.GetType();

            // Tìm property theo tên, không phân biệt hoa thường
            var prop = type.GetProperty(
                idFieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (prop == null)
                return 0;

            var value = prop.GetValue(row);
            if (value == null)
                return 0;

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Bind datasource cho 1 XRSubreport theo:
        ///  - Từ khoá trong tên subreport (subreport_name)
        ///  - Dictionary<int, List<TChild>> dataByInput (key = idInput)
        ///  - Trường khoá idFieldName trên row của parentBand (vd: "idInput")
        ///  - Tuỳ chọn dummyFactory: tạo 1 bản ghi giả nếu không có data
        ///  - Tuỳ chọn normalizeAction: chuẩn hoá dữ liệu trước khi gán
        /// 
        /// Dùng được cho:
        ///  - Standard: TChild = Standard_Model, dataByInput = StandardsByInput, không cần dummy
        ///  - DKM    : TChild = Dieu_kien_may_Model, dataByInput = DkmByInput, có dummy
        /// </summary>
        private static void binding_data_source_for_standard_report<TChild>(
            DetailReportBand parentBand,
            XRSubreport xRSubreport,
            string subreport_name,
            IDictionary<int, List<TChild>> dataByInput,
            string idFieldName,
            Func<int, TChild> dummyFactory = null,
            Action<TChild> normalizeAction = null)
            where TChild : class
        {
            if (parentBand == null || xRSubreport == null)
                return;

            // Subreport phải có ReportSource kiểu XtraReport thì ta mới gán DataSource cho nó được
            if (xRSubreport.ReportSource is not XtraReport childReport)
                return;

            // Nếu Name của subreport không chứa từ khoá subreport_name -> bỏ qua
            var subNameLower = (xRSubreport.Name ?? string.Empty).ToLowerInvariant();
            var keywordLower = (subreport_name ?? string.Empty).ToLowerInvariant();

            if (!subNameLower.Contains(keywordLower))
                return;

            // Ban đầu: set datasource rỗng + ẩn subreport
            childReport.DataSource = Array.Empty<TChild>();
            childReport.DataMember = null;
            xRSubreport.Visible = false;

            // Đăng ký BeforePrint để mỗi lần in 1 dòng của parentBand sẽ lấy đúng list data
            xRSubreport.BeforePrint += (_, __) =>
            {
                // Lấy dòng hiện tại mà parentBand đang in
                var currentRow = parentBand.GetCurrentRow();
                if (currentRow == null)
                {
                    xRSubreport.Visible = false;
                    childReport.DataSource = Array.Empty<TChild>();
                    childReport.DataMember = null;
                    return;
                }

                // Lấy idInput (hoặc field khoá khác) bằng reflection
                int idValue = GetIdFieldValue(currentRow, idFieldName);

                if (idValue == 0)
                {
                    xRSubreport.Visible = false;
                    childReport.DataSource = Array.Empty<TChild>();
                    childReport.DataMember = null;
                    return;
                }

                List<TChild> list = null;

                // Tra dictionary nếu có
                if (dataByInput != null)
                {
                    dataByInput.TryGetValue(idValue, out list);
                }

                // Nếu có list và có ít nhất 1 phần tử -> dùng luôn list này
                if (list != null && list.Count > 0)
                {
                    // Nếu có normalizeAction thì xử lý từng phần tử trước khi bind
                    if (normalizeAction != null)
                    {
                        foreach (var item in list)
                            normalizeAction(item);
                    }

                    xRSubreport.Visible = true;
                    childReport.DataSource = list;
                    childReport.DataMember = null;
                    return;
                }

                // Không có data -> nếu có dummyFactory thì tạo bản ghi giả
                if (dummyFactory != null)
                {
                    var dummy = dummyFactory(idValue);

                    if (dummy != null)
                    {
                        // Cho phép normalize dummy nếu cần
                        normalizeAction?.Invoke(dummy);

                        xRSubreport.Visible = true;
                        childReport.DataSource = new List<TChild> { dummy };
                        childReport.DataMember = null;
                        return;
                    }
                }

                // Không có data và cũng không tạo dummy -> ẩn subreport
                xRSubreport.Visible = false;
                childReport.DataSource = Array.Empty<TChild>();
                childReport.DataMember = null;
            };
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
        /// Hiển thị chế độ designer để thiết kế báo cáo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                // Lưu ý Key phải là tên trùng với tên DetailReportBand trong Designer để dễ quản lý
                var bandSchemas = new Dictionary<string, DataTable>
                {
                    // Band "Catongtho_Report": Sử dụng bảng Catthoong và đặt tên hiển thị là Cắt thô ống
                    ["Catongtho_Report"] = FieldWhitelistRegistry.Catthoong.ToDesignSchema("Cắt thô ống"),
                    // Band "Kiem_tra_ong_sau_cat_tho":
                     ["Kiem_tra_ong_sau_cat_tho"] = FieldWhitelistRegistry.Kiemtrasaucattho.ToDesignSchema("Kiểm tra sau cắt thô"),
                    // Band "Cam_chot": Sử dụng bảng Camchot và đặt tên hiển thị là Cắm chốt
                    ["Cam_chot"] = FieldWhitelistRegistry.Camchot.ToDesignSchema("Cắm chốt"),
                    ["Cam_chot_dkm"] = FieldWhitelistRegistry.Camchot_DKM.ToDesignSchema("Điều kiện máy cắm chốt"),
                    // Band "Dap_chuoi_cat_dinh_muc": Sử dụng bảng Dap_chuoi_cat_dinh_muc và đặt tên hiển thị là Dập chuôi cắt định mức
                    ["Dap_chuoi_cat_dinh_muc"] = FieldWhitelistRegistry.Dap_chuoi_cat_dinh_muc.ToDesignSchema("Dập chuôi cắt định mức"),
                    ["Dap_chuoi_cat_dinh_muc_dkm"] = FieldWhitelistRegistry.Dap_chuoi_cat_dinh_muc_DKM.ToDesignSchema("Điều kiện máy dập chuôi cắt định mức"),
                    ["Tu_dong_lap_rap_que_nong"] = FieldWhitelistRegistry.Tu_dong_lap_rap_que_nong.ToDesignSchema("Tự động lắp ráp que nong"),
                    ["Gia_cong_dau_mut_v1_5"] = FieldWhitelistRegistry.Gia_cong_dau_mut_v1_5.ToDesignSchema("Gia công đầu mút V1~V5"),
                    ["Rua_dau_mut_que_nong"] = FieldWhitelistRegistry.Rua_dau_mut_que_nong.ToDesignSchema("Rửa đầu mút que nong"),
                    ["Kiem_tra_ngoai_quan"] = FieldWhitelistRegistry.Kiem_tra_ngoai_quan.ToDesignSchema("Kiểm tra ngoại quan"),
                    ["Xu_ly_silicon"] = FieldWhitelistRegistry.Xu_ly_silicon.ToDesignSchema("Xử lý silicon"),
                    ["Kiem_tra_lan_cuoi"] = FieldWhitelistRegistry.Kiem_tra_lan_cuoi.ToDesignSchema("Kiểm tra lần cuối"),
                    ["Tong_ket"] = FieldWhitelistRegistry.Tong_ket.ToDesignSchema("Tổng kết"),               
                };

                // Gắn schema cho từng band theo tên
                DesignSchema.AttachBandSchemas(rpt, bandSchemas);

                // Tạo ReportDesignTool để mở End-User Designer
                var tool = new ReportDesignTool(rpt);           // tool chứa form Designer (Ribbon) và controller MDI
                var form = tool.DesignRibbonForm;               // IDesignForm (XRDesignRibbonForm implements IDesignForm) chính là cửa sổ Designer (bản Ribbon).
                var controller = form.DesignMdiController;      // XRDesignMdiController: trung tâm điều phối các "DesignPanel" (tab) trong Designer

                // SUBREPORT SCHEMA: chỉ khi người dùng mở tab subreport → mới gắn schema phù hợp cho subreport đó
                // Lưu ý đặt Key cho từng Schema phải trùng với tên với tên của XRsubreport trong Designer để dễ quản lý
                var subSchemas = new Dictionary<string, DataTable>
                {
                    // Key phải trùng với XRSubreport.Name trong designer
                    ["Cat_tho_ong_standard"] = FieldWhitelistRegistry.Standard_Catthoong.ToDesignSchema("Tiêu chuẩn cắt thô ống"),
                    ["Kiem_tra_ong_sau_cat_tho_standard"] = FieldWhitelistRegistry.Kiemtrasaucattho_Standard.ToDesignSchema("Tiêu chuẩn kiểm tra sau cắt thô"),
                    // Khai báo thêm các schema khác cho subreport tại đây
                    ["Cam_chot_standard"] = FieldWhitelistRegistry.Camchot_Standard.ToDesignSchema("Tiêu chuẩn cắm chốt"),
                    ["Dap_chuoi_cat_dinh_muc_standard"] = FieldWhitelistRegistry.Dap_chuoi_cat_dinh_muc_Standard.ToDesignSchema("Tiêu chuẩn dập chuôi cắt định mức"),
                    ["Tu_dong_lap_rap_que_nong_dkm"] = FieldWhitelistRegistry.Tu_dong_lap_rap_que_nong_DKM.ToDesignSchema("Điều kiện máy tự động lắp ráp que nong"),
                    ["Tu_dong_lap_rap_que_nong_standard"] = FieldWhitelistRegistry.Tu_dong_lap_rap_que_nong_Standard.ToDesignSchema("Tiêu chuẩn tự động lắp ráp que nong"),
                    ["Gia_cong_dau_mut_v1_5_standard"] = FieldWhitelistRegistry.Gia_cong_dau_mut_v1_5_Standard.ToDesignSchema("Tiêu chuẩn gia công đầu mút"),
                    ["Gia_cong_dau_mut_v1_5_dkm"] = FieldWhitelistRegistry.Gia_cong_dau_mut_v1_5_DKM.ToDesignSchema("Điều kiện máy gia công đầu mút"),
                    ["Gia_cong_dau_mut_v1_5_dkm_standard"] = FieldWhitelistRegistry.Gia_cong_dau_mut_v1_5_dkm_Standard.ToDesignSchema("Tiêu chuẩn điều kiện máy gia công đầu mút"),
                    ["Kiem_tra_ngoai_quan"] = FieldWhitelistRegistry.Kiem_tra_ngoai_quan_Standard.ToDesignSchema("Điều kiện máy kiểm tra ngoại quan"),
                    ["Summary_report"] = FieldWhitelistRegistry.Summary_Report.ToDesignSchema("Tổng kết theo lô"),
                };

                // Khai báo 1 schema mặc định (dùng nếu không match tên ở trên)
                var defaultSubSchema =
                    FieldWhitelistRegistry.Standard_Catthoong.ToDesignSchema("Tiêu chuẩn mặc định");

                // Gắn auto cho toàn bộ subreport trong Designer
                DesignSchema.WireSubreportSchemaOnDemandBySubName(
                    controller: controller,
                    mainReport: rpt,
                    schemasByName: subSchemas,
                    defaultSchema: defaultSubSchema
                );

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
                    //new ParameterSpec("Name_Congdoan",  typeof(string),  "Tên công đoạn"),
                    //new ParameterSpec("ID_Congdoan",    typeof(string),  "ID công đoạn"),
                    //new ParameterSpec("Code_Congdoan",  typeof(string),  "Mã công đoạn"),
                    new ParameterSpec("Category_Code",  typeof(string),  "Mã sản phẩm"),
                    new ParameterSpec("Lotno_Congdoan", typeof(string),  "Số lô"),
                    new ParameterSpec("Batch_Number",   typeof(string),  "Số mẻ"),
                    new ParameterSpec("NG_Qty_Total",   typeof(string),  "Tổng số hàng không phù hợp"),
                    new ParameterSpec("OK_Qty_Total",   typeof(string),  "Tổng số lượng hàng chuyển công đoạn sau"),
                };

                // Tạo các parameter dạng p_{Band}_{Param} ở cấp REPORT
                // Tạo parameter cho 1 band cụ thể
                //DesignSchema.EnsureParametersForBand(
                //    rpt,
                //    bandName: "Catongtho_Report",
                //    specs: headerParams,
                //    visible: false);

                // Tạo tự động parameter và đặt tên theo từng band để tránh nhầm lẫn
                DesignSchema.Attach_allparameter_report(rpt, headerParams);

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
