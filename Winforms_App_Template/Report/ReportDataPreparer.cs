// ===============================
// File: ReportDataPreparer.cs
// Namespace giữ nguyên theo cấu trúc dự án của bạn
// ===============================
using System;                                                    // Exception, ArgumentNullException, …
using System.Collections.Generic;                                // List<T>, Dictionary<TKey,TValue>
using System.Linq;                                               // LINQ: Select, GroupBy, ToDictionary, Distinct
using System.Threading;                                          // CancellationToken
using System.Threading.Tasks;                                    // Task, async/await
using Winforms_App_Template.Database;


// Các model dữ liệu bạn đang dùng trong dự án
using Winforms_App_Template.Database.Model;                      // Report_Header_Model, New_Input_Row, Input_Error_Model, Standard_Model, Que_Nong_Rows
using Winforms_App_Template.Database.Table;                      // NewInputs_Table, Input_Error_Table, Standard_Table

namespace Winforms_App_Template.Report
{
    /// <summary>
    /// - Chịu trách nhiệm TRUY VẤN & HỢP NHẤT dữ liệu cho 1..N công đoạn (step).
    /// - KHÔNG chạm UI, KHÔNG MessageBox (nếu lỗi → throw để tầng UI hứng sau await).
    /// </summary>
    public sealed class ReportDataPreparer
    {
        // =========================
        //  REPO TRUY VẤN DỮ LIỆU
        // =========================

        // Repo chính để lấy HEADER + DETAIL (New_Input_Row) cho một công đoạn
        private readonly NewInputs_Table get_detail_table_repo;      
        // Repo lấy lỗi chi tiết theo idInput
        private readonly Input_Error_Table input_error_repo;       
        // Repo lấy tiêu chuẩn theo idInput
        private readonly Standard_Table standard_repo;
        // Repo lấy điều kiện máy theo idInput
        private readonly DieuKienMay_Table dieu_kien_may_repo = null!;


        // =========================
        // 2) CTOR: nhận trực tiếp 3 repo
        // =========================

        /// <summary>
        /// Khởi tạo service với 3 repo sẵn có.
        /// </summary>
        public ReportDataPreparer(DbExecutor? db = null)
        {
            var executor = db ?? new DbExecutor();
            get_detail_table_repo = new NewInputs_Table(executor);
            input_error_repo = new Input_Error_Table(executor);
            standard_repo = new Standard_Table(executor);
            dieu_kien_may_repo = new DieuKienMay_Table(executor);
        }

        // ===========================================================
        //  CHUẨN HOÁ TÊN LỖI (đồng nhất khoá)
        // ===========================================================

        /// /// <summary>
        /// Chuẩn hoá khoá tên lỗi (trim + lower-invariant). Giúp giảm sai khác do chữ hoa/thường, khoảng trắng.
        /// </summary>
        private static string NormalizeErrorKey(string? key)
            => (key ?? string.Empty).Trim().ToLowerInvariant();


        private static string NormalizeKey(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return s.Trim().ToLowerInvariant();
        }

        // ===========================================================
        // PIVOT LỖI idInput → (tên lỗi → qty)
        // ===========================================================

        /// <summary>
        /// Từ list lỗi, tạo map: idInput → (tên lỗi chuẩn hoá → tổng Qty).
        /// </summary>
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


        // ===========================================================
        // 5) HÀM TIỆN ÍCH NỘI BỘ: MAP LỖI NGANG VÀO MẪU IN
        // ===========================================================

        /// <summary>
        /// Gán các cột lỗi "ngang" (Bevel_Cut, Flat, …) vào <see cref="Que_Nong_Rows"/>.
        /// Nếu cần mở rộng, thêm case vào đây.
        /// </summary>
        private static void SetKnownErrorColumns(Que_Nong_Rows dest, Dictionary<string, int>? kv)
        {
            // Reset về 0 để tránh sót giá trị cũ khi tái sử dụng object, đưa về mặc định
            dest.Cat_vat = 0;
            dest.Bep = 0;
            dest.Bavia = 0;
            dest.Roi = 0;
            dest.Chieu_dai_ngoai_tieu_chuan = 0;
            dest.Khac = 0;
            dest.Di_vat_ban = 0;
            dest.Lo_thung = 0;
            dest.Di_vat_duc = 0;
            dest.Cong = 0;
            dest.Loi_lom = 0;
            dest.Nham_xu_long = 0;
            dest.Xuoc = 0;
            dest.KTNQ_loi_lom = 0;
            dest.KTNG_Khac = 0;
            dest.NG_xuyen_qua_1 = 0;
            dest.NG_xuyen_qua_2 = 0;



            // Không có lỗi → thôi
            if (kv == null || kv.Count == 0) return;

            // Lưu ý: tên lỗi phải KHỚP với tên trong DB sau khi chuẩn hoá (Bỏ viết hoa thường)
            dest.Cat_vat = kv.TryGetValue(NormalizeErrorKey("cắt vát"), out var v1) ? v1 : 0;
            dest.Bep = kv.TryGetValue(NormalizeErrorKey("bẹp"), out var v2) ? v2 : 0;
            dest.Bavia = kv.TryGetValue(NormalizeErrorKey("bavia"), out var v3) ? v3 : 0;
            dest.Roi = kv.TryGetValue(NormalizeErrorKey("rơi"), out var v4) ? v4 : 0;
            dest.Chieu_dai_ngoai_tieu_chuan = kv.TryGetValue(NormalizeErrorKey("chiều dài ngoài tiêu chuẩn"), out var v5) ? v5 : 0;
            dest.Khac = kv.TryGetValue(NormalizeErrorKey("khác"), out var v6) ? v6 : 0;
            dest.Di_vat_ban = kv.TryGetValue(NormalizeErrorKey("dị vật, bẩn"), out var v7) ? v7 : 0;
            dest.Lo_thung = kv.TryGetValue(NormalizeErrorKey("lỗ thủng"), out var v8) ? v8 : 0;
            dest.Di_vat_duc = kv.TryGetValue(NormalizeErrorKey("dị vật đúc"), out var v9) ? v9 : 0;
            dest.Cong = kv.TryGetValue(NormalizeErrorKey("cong"), out var v10) ? v10 : 0;
            dest.Loi_lom = kv.TryGetValue(NormalizeErrorKey("Lỗi lồi lõm"), out var v11) ? v11 : 0;
            dest.Nham_xu_long = kv.TryGetValue(NormalizeErrorKey("Nhám, xù lông"), out var v12) ? v12 : 0;
            dest.Xuoc = kv.TryGetValue(NormalizeErrorKey("xước"), out var v13) ? v13 : 0;
            dest.KTNQ_loi_lom = kv.TryGetValue(NormalizeErrorKey("KTNQ bằng tiếp xúc _ Lồi lõm"), out var v14) ? v14 : 0;
            dest.KTNG_Khac = kv.TryGetValue(NormalizeErrorKey("KTNQ bằng tiếp xúc _ Khác"), out var v15) ? v15 : 0;
            dest.NG_xuyen_qua_1 = kv.TryGetValue(NormalizeErrorKey("Số lượng NG Ktra xuyên qua 1"), out var v16) ? v16 : 0;
            dest.NG_xuyen_qua_2 = kv.TryGetValue(NormalizeErrorKey("Số lượng NG Ktra xuyên qua 2"), out var v17) ? v17 : 0;
        }


        // ===========================================================
        // LẤY DỮ LIỆU CHO 1 CÔNG ĐOẠN (Step_Definition)
        // ===========================================================

        /// <summary>
        /// Truy vấn & hợp nhất dữ liệu cho **một** công đoạn nhỏ (Step_Definition).
        /// hàm này CHỈ TRẢ VỀ DỮ LIỆU (Data_Step_Model).
        /// </summary>
        /// <param name="step">Đặc tả công đoạn nhỏ (Id, tên band/subreport, prefix header) — dùng Id ở đây</param>
        /// <param name="itemNumber">Tham số chung</param>
        /// <param name="lotNo">Tham số chung</param>
        /// <param name="soMe">Tham số chung</param>
        /// <param name="ct">Token hủy</param>
        public async Task<Data_Step_Model> FetchStepBlockAsync(
            Step_Definition step,
            string itemNumber,
            string lotNo,
            int soMe,
            CancellationToken ct)
        {
            // Nếu công đoạn thuộc idCongDoan thuộc (68, 144) thì để nguyên ItemNumber; ngược lại, bỏ 1 ký tự đầu tiên
            if (step.Id != 68 && step.Id != 144 && itemNumber.Length > 1)
            {
                if (char.IsLetter(itemNumber[0])) itemNumber = itemNumber.Substring(1);
            }
            // Chạy truy vấn song song: Header + Detail rows (hai truy vấn độc lập)
            var headerTask = get_detail_table_repo.Get_Report_Header(
                IdCongDoan: step.Id, ItemNumber: itemNumber, LotNo: lotNo, So_Me: soMe, ct: ct);

            var rowsTask = get_detail_table_repo.Get_Detail_Table(
                IdCongDoan: step.Id, ItemNumber: itemNumber, LotNo: lotNo, So_Me: soMe, ct: ct);

            await Task.WhenAll(headerTask, rowsTask).ConfigureAwait(false);

            // Lấy kết quả từng phần
            var header = headerTask.Result;                               // Header có thể null (nếu không có dữ liệu)
            var rows = rowsTask.Result ?? new List<New_Input_Row>();      // Detail rows (nếu null → coi rỗng)

            // Không có Header → xem như "không tìm thấy dữ liệu" cho step.Id (throw để UI xử lý)
            if (header is null)
                throw new InvalidOperationException(
                    $"Không tìm thấy HEADER cho công đoạn Id={step.Id} (Item={itemNumber}, Lot={lotNo}, SoMe={soMe}).");

            // Kiểm tra người dùng có bấm Cancel chưa
            ct.ThrowIfCancellationRequested();

            // Lấy idInput duy nhất từ detail rows để query lỗi & tiêu chuẩn
            var idInputs = rows.Select(r => r.idInput).Distinct().ToArray();

            // Chạy song song: lỗi & tiêu chuẩn và điều kiện máy
            var errorsTask = input_error_repo.Get_Detail_Error(idInputs: idInputs, ct: ct);
            var stdsTask = standard_repo.Get_Detail_Standard(idInputs: idInputs);
            var dkmTask = dieu_kien_may_repo.Get_Detail_Dieu_Kien_May(idInputs: idInputs, ct: ct);

            await Task.WhenAll(errorsTask, stdsTask).ConfigureAwait(false);

            // Kiểm tra nếu nhận đầu vào có điều kiện máy là true thì tiến hành truy vấn dữ liệu điều kiện máy
            if (step.Isdkm)
            {
                await dkmTask.ConfigureAwait(false);
            } else
            {
                // Nếu không có điều kiện máy thì gán dkmTask là một Task trả về danh sách rỗng
                dkmTask = Task.FromResult(new List<Dieu_kien_may_Model>());
            }    

            var errorDetails = errorsTask.Result ?? new List<Input_Error_Model>();
            var standards = stdsTask.Result ?? new List<Standard_Model>();
            var dieuKienMayDetails = dkmTask.Result ?? new List<Dieu_kien_may_Model>();

            // Pivot lỗi: idInput → (tên lỗi chuẩn hoá → tổng qty)
            var pivot = BuildPivotMap(errorDetails);

            // Map New_Input_Row → Que_Nong_Rows và gán lỗi ngang từ pivot
            var resultRows = new List<Que_Nong_Rows>(rows.Count);         // pre-alloc capacity = rows.Count
            foreach (var m in rows)
            {
                // Lấy dictionary lỗi theo idInput hiện tại (có thể null nếu idInput không có lỗi)
                pivot.TryGetValue(m.idInput, out var errsDict);

                // Khởi tạo 1 dòng Que_Nong_Rows (model in báo cáo)
                var r = new Que_Nong_Rows
                {
                    idInput = m.idInput,
                    MaKT = m.MaKT,
                    TenMay_Ban = m.TenMay_Ban,
                    SLSudung = m.SLSudung,
                    StartTime = m.StartTime,
                    NguoiTT = m.NguoiTT,
                    OKQty = m.OKQty,
                    NGQty = m.NGQty,

                    // Sao chép 32 cột đo/giá trị sang model in
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

                // Gán các cột lỗi ngang vào r (0 nếu không có)
                SetKnownErrorColumns(r, errsDict);

                // Đưa vào list kết quả
                resultRows.Add(r);
            }

            // Gom tiêu chuẩn theo idInput để feed nhanh vào subreport
            var stdByInput = standards
                .GroupBy(s => s.idInput)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Kiểm tra huỷ lần cuối trước khi trả kết quả
            ct.ThrowIfCancellationRequested();

            // 11) Trả block dữ liệu đã chuẩn hoá cho công đoạn hiện tại
            return new Data_Step_Model
            {
                Id = step.Id,   // Lưu Id để tầng UI biết block này thuộc công đoạn nào
                Header = header,    // Header cho step.Id
                Rows = resultRows,// Dòng chi tiết đã hợp nhất lỗi ngang
                dkm = dieuKienMayDetails, // Dữ liệu cho điều kiện máy
                StandardsByInput = stdByInput // Map idInput → List<Standard_Model>
            };
        }


        // ===========================================================
        //  LẤY DỮ LIỆU CHO NHIỀU CÔNG ĐOẠN (song song có hạn)
        // ===========================================================

        /// <summary>
        /// Truy vấn & hợp nhất dữ liệu cho **nhiều** Step_Definition cùng lúc.
        /// - Chạy song song có kiểm soát bằng SemaphoreSlim để tránh quá tải DB.
        /// - Trả về: Id công đoạn → Data_Step_Model.
        /// </summary>
        /// <param name="steps">Danh sách step (ít nhất chứa Id); BandName/SubreportName/HeaderParamPrefix dùng ở tầng bind</param>
        /// <param name="itemNumber">Tham số chung</param>
        /// <param name="lotNo">Tham số chung</param>
        /// <param name="soMe">Tham số chung</param>
        /// <param name="maxConcurrency">Số tác vụ tối đa chạy đồng thời (khuyến nghị 3–4 cho 7–8 công đoạn)</param>
        /// <param name="ct">Token hủy</param>
        public async Task<Dictionary<int, Data_Step_Model>> FetchAllStepsAsync(
            IEnumerable<Step_Definition> steps,
            string itemNumber,
            string lotNo,
            int soMe,
            int maxConcurrency,
            CancellationToken ct)
        {
            // Từ danh sách step (có thể chứa trùng Id do merge nhầm) → lọc distinct theo Id cho chắc
            var stepList = steps
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .ToList();

            // Map kết quả: Id → Data_Step_Model (sẽ được điền dần)
            var dict = new Dictionary<int, Data_Step_Model>();

            // "Van" điều tiết song song để không dồn DB (ví dụ set = 3)
            using var gate = new SemaphoreSlim(maxConcurrency);

            // Tạo một task cho mỗi step
            var tasks = stepList.Select(async step =>
            {
                // Chờ đến lượt nếu đang quá số "slot" cho phép
                await gate.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    // Lấy block dữ liệu cho step hiện tại
                    var block = await FetchStepBlockAsync(step, itemNumber, lotNo, soMe, ct)
                        .ConfigureAwait(false);

                    // Ghi vào dictionary kết quả (Dictionary không thread-safe → cần lock)
                    lock (dict)
                        dict[step.Id] = block;
                }
                finally
                {
                    // Mở khoá cho step tiếp theo
                    gate.Release();
                }
            });

            // Chờ tất cả task hoàn tất
            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Trả về map Id → Data_Step_Model
            return dict;
        }
    }
}
