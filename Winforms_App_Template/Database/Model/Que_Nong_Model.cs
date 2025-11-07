using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class Report_Header_Model
    {
        public string Name_Congdoan { get; set; } = "";
        public string ID_Congdoan { get; set; } = "";
        public string Code_Congdoan { get; set; } = "";
        public string Category_Code { get; set; } = "";
        public string Lotno_Congdoan { get; set; } = "";
        public string Batch_Number { get; set; } = "";
        public int? NG_Qty_Total { get; set; }
        public int? OK_Qty_Total { get; set; }
    }

    /// <summary>
    /// Model cho toàn bộ trường dữ liệu có thể tồn tại trong công đoạn que nong
    /// </summary>
    public sealed class Que_Nong_Rows
    {

        [System.ComponentModel.Browsable(false)] // <-- ẨN khỏi Field List
        public int idInput { get; init; }                          // ID Form nhập dữ liệu để lấy chi tiết lỗi

        [System.ComponentModel.DisplayName("Mã kiểm tra")]
        public string MaKT { get; init; } = "";                    // Mã lý do kiểm tra: I, II, III, IV
        public DateTime StartTime { get; init; }                   // Thời gian bắt đầu thao tác
        public string NguoiTT { get; init; } = "";                 // Người thao tác
        public string? TenMay_Ban { get; set; }                    // Tên máy bàn thao tác
        public string? val1 { get; set; }                          // Số lượng ống dài sử dụng
        public string? val2 { get; set; }                          // Số lượng ống dài cắt được
        public string? val3 { get; set; }                          // Mã quản lý thickness gauge
        public string? val4 { get; set; }                          // Đường kính ngoài ống dài
        public string? val5 { get; set; }                          // Đường kính ngoài ống dài yes no
        public string? val6 { get; set; }                          // Mã pingauge 098mm
        public string? val7 { get; set; }                          // Đường kính trong loại 4Fr, 4KFr xuyên (yes no)
        public string? val8 { get; set; }                          // Đường kính trong loại 4Fr, 4KFr không xuyên (yes no)
        public string? val9 { get; set; }                          // Trạng thái cắt 10 ống
        public string? val10 { get; set; }                         // Mã thước sử dụng
        public string? val11 { get; set; }                         // Thước sử dụng 1
        public string? val12 { get; set; }                         // Thước sử dụng 2
        public string? val13 { get; set; }                         // Thước sử dụng 3
        public string? val14 { get; set; }                         // Thước sử dụng yes no
        public string? val15 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val16 { get; set; }                         // Để trường cho tương lai
        public string? val17 { get; set; }                         // Để trường cho tương lai
        public string? val18 { get; set; }                         // Để trường cho tương lai
        public string? val19 { get; set; }                         // Để trường cho tương lai
        public string? val20 { get; set; }                         // Để trường cho tương lai
        public string? val21 { get; set; }                         // Để trường cho tương lai
        public string? val22 { get; set; }                         // Để trường cho tương lai
        public string? val23 { get; set; }                         // Để trường cho tương lai
        public string? val24 { get; set; }                         // Để trường cho tương lai
        public string? val25 { get; set; }                         // Để trường cho tương lai
        public string? val26 { get; set; }                         // Để trường cho tương lai
        public string? val27 { get; set; }                         // Để trường cho tương lai
        public string? val28 { get; set; }                         // Để trường cho tương lai
        public string? val29 { get; set; }                         // Để trường cho tương lai
        public string? val30 { get; set; }                         // Để trường cho tương lai
        public string? val31 { get; set; }                         // Để trường cho tương lai
        public string? val32 { get; set; }                         // Để trường cho tương lai
        public int SLSudung { get; init; }                         // Số lượng sản phẩm đã sử dụng
        public string? Remark { get; init; }                       // Số lượng sản phẩm đã sử dụng

        // --- phần chi tiết lỗi ---
        public int Cat_vat { get; set; }                          // 1: Lỗi cắt vát
        public int Bep { get; set; }                              // 3: Lỗi bẹp
        public int Chieu_dai_ngoai_tieu_chuan { get; set; }       // 4: Lỗi Chiều dài ngoài tiêu chuẩn
        public int Bavia { get; set; }                            // 5: Lỗi Bavia
        public int Bat_thuong_thiet_bi { get; set; }              // 5: Lỗi Bất thường thiết bị
        public int Khac { get; set; }                             // 7: Lỗi Khác
        public int Roi { get; set; }                              // 9: Lỗi rơi
        public int Bat_thuong_may { get; set; }                   // 10: Lỗi Bất thường máy
        public int Thung { get; set; }                            // 11: Thủng
        public int Sut { get; set; }                              // 12: Sứt
        public int Nong_sau { get; set; }                         // 13: Nông sâu (độ sâu cắm không phù hợp)
        public int Lom { get; set; }                              // 14: Lõm
        public int Di_vat_ban_khuon { get; set; }                 // 15: Dị vật bẩn khuôn
        public int Di_vat_duc { get; set; }                       // 16: Dị vật đúc
        public int Xuoc { get; set; }                             // 17: Xước
        public int Ngan { get; set; }                             // 18: Ngấn
        public int Mang_ca { get; set; }                          // 19: Mang cá
        public int Ran_ong { get; set; }                          // 20: Rạn ống
        public int Cong { get; set; }                             // 41: Lỗi Cong
        public int Di_vat_ban { get; set; }                       // 58: Dị vật bẩn
        public int Lo_thung { get; set; }                         // 128: Lỗ thung
        public int Nham_xu_long { get; set; }                     // 129: Nhám xù lông
        public int KTNQ_loi_lom { get; set; }                     // 130: KTNQ bằng tiếp xúc _ Lồi lõm
        public int KTNG_Khac { get; set; }                        // 131: KTNQ bằng tiếp xúc _ Khác
        public int NG_xuyen_qua_1 { get; set; }                   // 158: Số lượng NG Ktra xuyên qua 1
        public int NG_xuyen_qua_2 { get; set; }                   // 159: Số lượng NG Ktra xuyên qua 2
        public int Loi_lom { get; set; }                          // 160: Lỗi lồi lõm
    }

    /// <summary>
    /// Record mô tả "một công đoạn nhỏ trong 1 công đoạn lớn"
    /// </summary>
    public sealed record Step_Definition(
        int Id,                           // ID công đoạn, ví dụ: 68, 144
        string BandName,                  // Tên DetailReportBand trong layout (ví dụ: "Catthoong, ...")
        //string StandardsSubreportName,    // Tên XRSubreport chứa bảng tiêu chuẩn trong band (ví dụ: "SR_Standards_68")
        string HeaderParamPrefix,         // Prefix đẩy tham số header (ví dụ: "p_band_text")
        bool Isdkm = false                // Công đoạn này có điều kiện máy không
    );

    /// <summary>
    /// Kết quả chuẩn hoá dữ liệu cho MỘT công đoạn sau khi truy vấn và hợp nhất.
    /// </summary>
    public sealed class Data_Step_Model
    {
        public required int Id { get; init; }
        public required Report_Header_Model Header { get; init; }          // Dữ liệu cho tiêu đề trong báo cáo
        public required List<Que_Nong_Rows> Rows { get; init; }            // Dữ liệu chi tiết cho bảng trong
        public required List<Dieu_kien_may_Model> dkm { get; init; }       // Dữ liệu chi tiết cho bảng trong
        public required Dictionary<int, List<Standard_Model>> StandardsByInput { get; init; }   // Dữ liệu cho bảng tiêu chuẩn, theo idInput
    }

}
