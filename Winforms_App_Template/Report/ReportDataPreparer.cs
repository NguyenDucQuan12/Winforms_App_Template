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
        //  Map tĩnh từ propertyName -> pretty error name (nguyên dạng human)
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
            ["Loi_lom"] = "Lỗi lồi lõm",
            ["Di_vat_ban_khuon"] = "Dị vật bẩn khuôn",
            ["Di_vat_duc"] = "dị vật đúc",
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
            // Thêm các mapping khác tương ứng với các property trong Que_Nong_Rows...
            // Nếu thiếu, code sẽ fallback dùng tên property đã normalize theo công thức nhất định bên dưới.
        };

        // -----------------------------
        // Cache: key = normalized(prettyName) -> PropertyInfo
        //    Dùng Lazy để khởi tạo một lần khi cần, thread-safe.
        // -----------------------------
        private static readonly Lazy<Dictionary<string, PropertyInfo>> NormalizedKeyToPropertyMapLazy =
            new Lazy<Dictionary<string, PropertyInfo>>(BuildNormalizedMap, isThreadSafe: true);

        // Truy cập map đã được tạo
        private static Dictionary<string, PropertyInfo> NormalizedKeyToPropertyMap => NormalizedKeyToPropertyMapLazy.Value;

        // -----------------------------
        // gán tự động dựa trên map cache
        // -----------------------------
        /// <summary>
        /// Gán các cột lỗi tự động cho dest từ dictionary kv (tên lỗi chuẩn hoá -> qty).
        /// Nếu key không tồn tại trong kv thì gán 0.
        /// </summary>
        /// <param name="dest">Đối tượng Que_Nong_Rows cần gán</param>
        /// <param name="kv">Pivot dictionary: key = tên lỗi (chưa chuẩn hoá), value = số lượng</param>
        public static void SetKnownErrorColumns_Cached(Que_Nong_Rows dest, Dictionary<string, int>? kv)
        {
            // Nếu danh sách lỗi null hoặc rỗng:
            //  → gán 0 cho TẤT CẢ CỘT LỖI trong model (theo map cache),
            //    KHÔNG ĐỤNG tới các int khác (vì map chỉ chứa các cột lỗi)
            if (kv == null || kv.Count == 0)
            {
                // Duyệt tất cả PropertyInfo trong cache lỗi và gán 0
                foreach (var prop in NormalizedKeyToPropertyMap.Values)
                {
                    // prop: chính là các property kiểu int tương ứng với mã lỗi
                    prop.SetValue(dest, 0);
                }
                return;
            }

            // Chuẩn hoá toàn bộ keys của kv sang dạng dùng để so sánh (normalize)
            var normalizedKv = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in kv)
            {
                // NormalizeErrorKey:
                // - lowercase
                // - bỏ dấu tiếng Việt
                // - thay '_' '-' '/' ',' thành space
                // - gộp nhiều space thành 1
                var normKey = NormalizeErrorKey(item.Key);

                // Nếu cùng key sau normalize xuất hiện nhiều lần, cộng dồn số lượng lỗi
                if (normalizedKv.ContainsKey(normKey))
                    normalizedKv[normKey] += item.Value;
                else
                    normalizedKv[normKey] = item.Value;
            }

            // Duyệt các cột lỗi đã được cache trong NormalizedKeyToPropertyMap
            foreach (var kvp in NormalizedKeyToPropertyMap)
            {
                // normKey: key đã chuẩn hoá tương ứng với 1 cột lỗi trong model
                var normKey = kvp.Key;

                // propInfo: property tương ứng (VD: Cat_vat, Bep, Thieu_nhua, ...)
                var propInfo = kvp.Value;

                // Lấy giá trị lỗi từ normalizedKv nếu có, nếu không thì 0
                var valueToSet = normalizedKv.TryGetValue(normKey, out var val) ? val : 0;

                // Gán giá trị vào property tương ứng trên đối tượng dest
                propInfo.SetValue(dest, valueToSet);
            }
        }

        // -----------------------------
        //  BuildNormalizedMap: tạo dictionary normalizedKey -> PropertyInfo
        //    CHỈ cho các cột lỗi nằm trong PropertyToPrettyName
        // -----------------------------
        private static Dictionary<string, PropertyInfo> BuildNormalizedMap()
        {
            // Lấy type của model Que_Nong_Rows (model in báo cáo)
            var type = typeof(Que_Nong_Rows);

            // Tạo dictionary kết quả: key = tên lỗi đã normalize, value = PropertyInfo tương ứng
            var map = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            // Duyệt TỪ ĐIỂN PropertyToPrettyName:
            //   - Key: tên property trong model (VD: "Cat_vat")
            //   - Value: tên lỗi dạng human (VD: "cắt vát")
            foreach (var kv in PropertyToPrettyName)
            {
                // Lấy tên property trong model (VD: "Cat_vat")
                var propertyName = kv.Key;

                // Lấy "pretty error name" (VD: "cắt vát")
                var prettyName = kv.Value;

                // Lấy PropertyInfo tương ứng với tên property trong model
                // BindingFlags.Public | BindingFlags.Instance: chỉ lấy property public instance
                // BindingFlags.IgnoreCase: không phân biệt hoa thường khi tìm property
                var prop = type.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                );

                // Nếu không tìm thấy property trong model → bỏ qua (tránh crash)
                if (prop == null)
                {
                    // Ở đây có thể ghi log lại nếu muốn:
                    // Console.WriteLine($"[Warn] Không tìm thấy property '{propertyName}' trong Que_Nong_Rows.");
                    continue;
                }

                // Chỉ nhận những property kiểu int (đúng là cột lỗi)
                if (prop.PropertyType != typeof(int))
                {
                    // Nếu trong PropertyToPrettyName lỡ khai báo nhầm property không phải int thì bỏ qua
                    continue;
                }

                // Normalize "pretty error name" để làm key tra cứu
                // Ví dụ: "cắt vát" -> "cat vat"
                var normalizedKey = NormalizeErrorKey(prettyName);

                // Gán vào map:
                //   - key: tên lỗi đã normalize
                //   - value: PropertyInfo để set value sau này
                // Nếu trùng key (hiếm khi) thì key sau sẽ ghi đè key trước
                map[normalizedKey] = prop;
            }

            // Trả về map đã xây dựng
            return map;
        }

        // -----------------------------
        //Hàm normalize: chuyển input thành dạng chuẩn để so sánh
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
        // RemoveDiacritics: loại bỏ dấu tiếng Việt (Unicode normalization)
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

        /// <summary>
        /// Chuẩn hoá các giá trị string "true"/"false"/null trong 1 object bất kỳ:
        ///   - "true"  (không phân biệt hoa/thường, có thể có khoảng trắng) → "OK"
        ///   - "false" (không phân biệt hoa/thường, có thể có khoảng trắng) → "NG"
        ///   - null / rỗng / toàn khoảng trắng                     → "N/A"
        ///   - Giá trị khác (số, text bình thường)                 → giữ nguyên
        ///
        /// Tham số onlyValPrefix:
        ///   - true  → chỉ xử lý các property tên bắt đầu bằng "val" (val1..val32, v.v.)
        ///   - false → xử lý TẤT CẢ property kiểu string trong object
        /// </summary>
        private static void NormalizeTrueFalseStringValues(object target, bool onlyValPrefix = true, params string[] excludedPropertyNames)
        {
            // Nếu object truyền vào null thì không làm gì, tránh lỗi NullReferenceException
            if (target == null) return;

            // Lấy kiểu runtime của object, ví dụ:
            // - Que_Nong_Rows
            // - Report_Header_Model
            // - Dieu_kien_may_Model
            // - v.v...
            var type = target.GetType();

            // Lấy danh sách TẤT CẢ property public instance trên type này
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Chuẩn hoá danh sách tên property cần bỏ qua vào HashSet
            // để so sánh nhanh & không phân biệt hoa/thường
            var excluded =
                new HashSet<string>(excludedPropertyNames ?? Array.Empty<string>(),
                    StringComparer.OrdinalIgnoreCase);

            // Duyệt từng property trong danh sách
            foreach (var prop in props)
            {
                // Nếu tên property nằm trong danh sách bị loại trừ → bỏ qua
                if (excluded.Contains(prop.Name))
                    continue;

                // Nếu onlyValPrefix = true:
                //   → chỉ xử lý những property có tên bắt đầu bằng "val" (val1, val2, valXYZ,...)
                if (onlyValPrefix)
                {
                    // Nếu tên property KHÔNG bắt đầu bằng "val" (không phân biệt hoa/thường) → bỏ qua
                    if (!prop.Name.StartsWith("val", StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                // Chỉ xử lý các property kiểu string
                if (prop.PropertyType != typeof(string))
                    continue; // nếu không phải string thì bỏ qua

                // Nếu property không có setter (read-only) thì cũng bỏ qua
                if (!prop.CanWrite)
                    continue;

                // Lấy giá trị hiện tại của property trên object
                // sử dụng as string để nếu null thì raw sẽ là null
                var raw = prop.GetValue(target) as string;

                // Nếu string null, rỗng, hoặc toàn khoảng trắng
                if (string.IsNullOrWhiteSpace(raw))
                {
                    // → gán lại value là "N/A"
                    prop.SetValue(target, "N/A");

                    // Xử lý xong property này, chuyển sang property tiếp theo
                    continue;
                }

                // Loại bỏ khoảng trắng đầu/cuối để so sánh chính xác
                var trimmed = raw.Trim();

                // Nếu chuỗi là "true" (không phân biệt hoa/thường)
                if (trimmed.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    // → đổi thành "OK"
                    prop.SetValue(target, "OK");
                }
                // Nếu chuỗi là "false" (không phân biệt hoa/thường)
                else if (trimmed.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    // → đổi thành "NG"
                    prop.SetValue(target, "NG");
                }
                // Nếu không phải "true"/"false"
                //   → giữ nguyên giá trị ban đầu (raw), nên không cần làm gì thêm
            }
        }

        // ----------------------------------------------------------
        // Hàm helper: chuẩn hoá "true"/"false"/null cho TỪNG PHẦN TỬ
        // trong 1 collection (List<Dieu_kien_may_Model>, List<Que_Nong_Rows>, ...)
        // ----------------------------------------------------------
        private static void NormalizeTrueFalseStringValues<T>(IEnumerable<T> items, bool onlyValPrefix = true, params string[] excludedPropertyNames)
        {
            // Nếu collection null thì không làm gì
            if (items == null) return;

            // Duyệt từng phần tử và áp dụng hàm core
            foreach (var item in items)
            {
                NormalizeTrueFalseStringValues(item!, onlyValPrefix, excludedPropertyNames); // item! để compiler khỏi warning nullable
            }
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
            var header = headerTask.Result ?? new Report_Header_Model();                               // Header có thể null (nếu không có dữ liệu)
            var rows = rowsTask.Result ?? new List<New_Input_Row>();      // Detail rows (nếu null → coi rỗng)

            if (header != null)
            {
                NormalizeTrueFalseStringValues(header, false);
            }

            // Không có Header → xem như "không tìm thấy dữ liệu" cho step.Id (throw để UI xử lý)
            //if (header is null)
            //    throw new InvalidOperationException(
            //        $"Không tìm thấy Dữ liệu cho công đoạn Id={step.Id} (Item={itemNumber}, Lot={lotNo}, SoMe={soMe}).");

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

            // ------------------------------------------------------
            // chuẩn hoá các giá trị "true"/"false" trong val1..val32
            //   - "true"  → "OK"
            //   - "false" → "NG"
            //   - null/rỗng → "N/A"
            // ------------------------------------------------------
            NormalizeTrueFalseStringValues(dieuKienMayDetails, false, "Ly_do_kiem_tra");
            NormalizeTrueFalseStringValues(standards, false);

            // Tổng số lỗi cho công đoạn này (tính theo Qty)
            var totalErrorQtyForThisStep = errorDetails.Sum(e => e.Qty);

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
                    Ly_do_kiem_tra = m.Ly_do_kiem_tra,
                    TenMay_Ban = m.TenMay_Ban,
                    SLSudung = m.SLSudung,
                    StartTime = m.StartTime,
                    NguoiTT = m.NguoiTT,
                    TenNguoiThaoTac = m.TenNguoiThaoTac,
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
                    val33 = m.val33,
                    val34 = m.val34,
                    val35 = m.val35,
                    val36 = m.val36,
                    val37 = m.val37,
                    val38 = m.val38,
                    val39 = m.val39,
                    val40 = m.val40,
                    val41 = m.val41,
                    val42 = m.val42,
                    val43 = m.val43,

                    Remark = m.Remark
                };

                // Gán các cột lỗi ngang vào r (0 nếu không có)
                SetKnownErrorColumns_Cached(r, errsDict);

                // ------------------------------------------------------
                // chuẩn hoá các giá trị "true"/"false" trong val1..val32
                //   - "true"  → "OK"
                //   - "false" → "NG"
                //   - null/rỗng → "N/A"
                // ------------------------------------------------------
                NormalizeTrueFalseStringValues(r, false, "Ly_do_kiem_tra");

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
                StandardsByInput = stdByInput, // Map idInput → List<Standard_Model>
                DkmByInput = dieuKienMayDetails
                    .GroupBy(d => d.idInput)
                    .ToDictionary(g => g.Key, g => g.ToList()),
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

        /// <summary>
        /// Trả về true nếu giá trị string "như là true" (true/OK/1/YES...).
        /// </summary>
        private static bool IsTrueLike(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var v = value.Trim();

            return v.Equals("true", StringComparison.OrdinalIgnoreCase)
                || v.Equals("ok", StringComparison.OrdinalIgnoreCase)
                || v.Equals("1", StringComparison.OrdinalIgnoreCase)
                || v.Equals("y", StringComparison.OrdinalIgnoreCase)
                || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra trong blocks có công đoạn 77 (Tổng kết) và val7 ("Mẻ cuối lô") = true hay không.
        /// </summary>
        public static bool IsLastLotBatch(
            IReadOnlyDictionary<int, Data_Step_Model> blocks)
        {
            // Không có công đoạn tổng kết -> chắc chắn không phải mẻ cuối
            if (!blocks.TryGetValue(77, out var tongKetBlock))
                return false;

            // Lấy dòng đầu tiên (thường band Tong_ket chỉ có 1 dòng)
            var row = tongKetBlock.Rows.FirstOrDefault();
            if (row == null)
                return false;

            // Lấy giá trị val7 từ model in báo cáo (Que_Nong_Rows)
            var flag = row.val7;

            // Chuyển sang bool theo kiểu "true-like"
            return IsTrueLike(flag);
        }
    }
}
