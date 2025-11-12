// ===============================
// File: ReportDataPreparer.cs
// Namespace giữ nguyên theo cấu trúc dự án của bạn
// ===============================
using System;                                                    // Exception, ArgumentNullException, …
using System.Collections.Generic;                                // List<T>, Dictionary<TKey,TValue>
using System.Globalization;
using System.Linq;                                               // LINQ: Select, GroupBy, ToDictionary, Distinct
using System.Reflection;
using System.Text;
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


        // -----------------------------
        // 1) Map tĩnh từ propertyName -> pretty error name (nguyên dạng human)
        //    Bạn có thể mở rộng/bổ sung nếu có property mới hoặc tên khóa DB khác.
        // -----------------------------
        private static readonly Dictionary<string, string> PropertyToPrettyName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // "Tên property trong model" => "Tên lỗi chuẩn (dùng để normalize và so sánh với pivot)"
            ["Cat_vat"] = "cắt vát",
            ["Bep"] = "bẹp",
            ["Chieu_dai_ngoai_tieu_chuan"] = "chiều dài ngoài tiêu chuẩn",
            ["Bavia"] = "bavia",
            ["Bat_thuong_thiet_bi"] = "bất thường thiết bị",
            ["Khac"] = "khác",
            ["Roi"] = "rơi",
            ["Bat_thuong_may"] = "bất thường máy",
            ["Thung"] = "Thủng",
            ["Sut"] = "Sứt",
            ["Nong_sau"] = "Nông sâu (độ sâu cắm không phù hợp)",
            ["Lom"] = "Lõm",
            ["Di_vat_ban_khuon"] = "Dị vật bẩn khuôn",
            ["Di_vat_duc"] = "dị vật đúc",
            ["Xuoc"] = "xước",
            ["Ngan"] = "ngấn",
            ["Mang_ca"] = "mang cá",
            ["Ran_ong"] = "Rạn ống",
            ["Vang_chay_dau_mut"] = "Vàng cháy đầu mút",
            ["Bep_gap_ong"] = "Bẹp, gập ống",
            ["Dap_dau_mut"] = "Dập đầu mút",
            ["Nut_vo"] = "Nứt, vỡ",
            ["Thieu_nhua"] = "Thiếu nhựa",
            ["Gia_cong_chua_hoan_thien"] = "Gia công chưa hoàn thiện",
            ["Cong_bien_dang"] = "Cong, biến dạng",
            ["Thieu_linh_kien"] = "Thiếu linh kiện",
            ["Cong"] = "cong",
            ["Loi"] = "lồi",
            ["Di_vat_ban"] = "Dị vật, bẩn",
            ["Xuoc"] = "xước",
            ["Lom_thieu_nhua"] = "Lõm, thiếu nhựa",
            ["Lo_thung"] = "Lỗ thủng",
            ["Nham_xu_long"] = "nhám, xù lông",
            ["KTNQ_loi_lom"] = "KTNQ bằng tiếp xúc _ Lồi lõm",
            ["KTNG_Khac"] = "KTNQ bằng tiếp xúc _ Khác",
            ["NG_xuyen_qua_1"] = "Số lượng NG Ktra xuyên qua 1",
            ["NG_xuyen_qua_2"] = "số lượng NG Ktra xuyên qua 2",
            // Thêm các mapping khác tương ứng với các property int trong Que_Nong_Rows...
            // Nếu thiếu, code sẽ fallback dùng tên property đã normalize theo công thức nhất định bên dưới.
        };

        // -----------------------------
        // 2) Cache: key = normalized(prettyName) -> PropertyInfo
        //    Dùng Lazy để khởi tạo một lần khi cần, thread-safe.
        // -----------------------------
        private static readonly Lazy<Dictionary<string, PropertyInfo>> NormalizedKeyToPropertyMapLazy =
            new Lazy<Dictionary<string, PropertyInfo>>(BuildNormalizedMap, isThreadSafe: true);

        // Truy cập map đã được tạo
        private static Dictionary<string, PropertyInfo> NormalizedKeyToPropertyMap => NormalizedKeyToPropertyMapLazy.Value;

        // -----------------------------
        // 3) Hàm chính: gán tự động dựa trên map cache
        // -----------------------------
        /// <summary>
        /// Gán các cột lỗi tự động cho dest từ dictionary kv (tên lỗi chuẩn hoá -> qty).
        /// Nếu key không tồn tại trong kv thì gán 0.
        /// </summary>
        /// <param name="dest">Đối tượng Que_Nong_Rows cần gán</param>
        /// <param name="kv">Pivot dictionary: key = tên lỗi (chưa chuẩn hoá), value = số lượng</param>
        public static void SetKnownErrorColumns_Cached(Que_Nong_Rows dest, Dictionary<string, int>? kv)
        {
            // 1) Nếu null hoặc rỗng, gán 0 cho tất cả property int theo map để đảm bảo reset
            if (kv == null || kv.Count == 0)
            {
                // Duyệt tất cả property trong cache và gán 0
                foreach (var prop in NormalizedKeyToPropertyMap.Values)
                {
                    prop.SetValue(dest, 0);
                }
                return;
            }

            // 2) Chuẩn hoá toàn bộ keys của kv sang dạng dùng để so sánh,
            //    để giảm số lần chuẩn hoá nhiều lần ta tạo dictionary mới
            var normalizedKv = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in kv)
            {
                // NormalizeErrorKey sẽ:
                // - lowercase, remove diacritics (dấu tiếng Việt), replace punctuation/underscores -> spaces, trim
                var normKey = NormalizeErrorKey(item.Key);
                // Nếu cùng key sau normalize xuất hiện nhiều lần, cộng dồn (nếu cần).
                if (normalizedKv.ContainsKey(normKey))
                    normalizedKv[normKey] += item.Value;
                else
                    normalizedKv[normKey] = item.Value;
            }

            // 3) Duyệt map cache (tất cả property lỗi), lấy giá trị tương ứng từ normalizedKv nếu có, ngược lại = 0
            foreach (var kvp in NormalizedKeyToPropertyMap)
            {
                var normKey = kvp.Key;           // key đã chuẩn hoá tương ứng với property
                var propInfo = kvp.Value;       // property info cần gán

                // Lấy giá trị từ normalizedKv (nếu có) hoặc 0
                var valueToSet = normalizedKv.TryGetValue(normKey, out var val) ? val : 0;

                // Gán giá trị vào property (PropertyInfo.SetValue)
                propInfo.SetValue(dest, valueToSet);
            }
        }

        // -----------------------------
        // 4) BuildNormalizedMap: tạo dictionary normalizedKey -> PropertyInfo
        //    gọi 1 lần khi Lazy khởi tạo
        // -----------------------------
        private static Dictionary<string, PropertyInfo> BuildNormalizedMap()
        {
            // Lấy type của model
            var type = typeof(Que_Nong_Rows);

            // Lấy tất cả property public instance
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Tạo kết quả
            var map = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in props)
            {
                // Chúng ta chỉ quan tâm tới các property kiểu int (các cột lỗi)
                if (prop.PropertyType != typeof(int)) continue;

                // 1) Nếu có mapping "pretty name" khai báo rõ ràng thì dùng nó
                if (PropertyToPrettyName.TryGetValue(prop.Name, out var pretty))
                {
                    var normalized = NormalizeErrorKey(pretty);
                    // Nếu có trùng key (hiếm), có thể ghi đè hoặc bỏ qua. Ở đây ghi đè.
                    map[normalized] = prop;
                    continue;
                }

                // 2) Nếu không có mapping thủ công, fallback dùng tên property (ví dụ "Cat_vat")
                //    chuyển đổi propertyName -> readable -> normalize
                var fallbackPretty = prop.Name.Replace('_', ' '); // Cat_vat -> "Cat vat"
                var normalizedFallback = NormalizeErrorKey(fallbackPretty);
                map[normalizedFallback] = prop;
            }

            return map;
        }

        // -----------------------------
        // 5) Hàm normalize: chuyển input thành dạng chuẩn để so sánh
        //    - lower case
        //    - remove diacritics (dấu tiếng Việt)
        //    - replace underscores/dashes/commas -> blank
        //    - trim multiple spaces
        // -----------------------------
        private static string NormalizeErrorKey(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // 1) trim + lowercase (invariant)
            var s = input.Trim().ToLowerInvariant();

            // 2) Replace underscores/hyphens/commas/slashes with space
            s = s.Replace("_", " ").Replace("-", " ").Replace("/", " ").Replace(",", " ");

            // 3) Remove diacritics (dấu) bằng cách dùng normalization + filter
            //    Ví dụ: "chiều" -> "chieu", "đ" -> "d"
            s = RemoveDiacritics(s);

            // 4) Several replacements to normalize common punctuation
            s = s.Replace("  ", " ").Replace("   ", " ").Trim();

            return s;
        }

        // -----------------------------
        // 6) RemoveDiacritics: loại bỏ dấu tiếng Việt (Unicode normalization)
        // -----------------------------
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Chuẩn hoá về FormD để tách base char và dấu
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(capacity: normalized.Length);

            foreach (var ch in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                // Giữ lại ký tự là base letter (không phải dấu)
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            // Chuyển về FormC
            var cleaned = sb.ToString().Normalize(NormalizationForm.FormC);

            // Thay riêng ký tự đ/Đ (nếu tồn tại) -- một số hệ thống normalize không chuyển đ -> d
            cleaned = cleaned.Replace('đ', 'd').Replace('Đ', 'D');

            return cleaned;
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
                SetKnownErrorColumns_Cached(r, errsDict);

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
