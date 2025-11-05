using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class New_Input_Row
    {

        [System.ComponentModel.Browsable(false)] // <-- ẨN khỏi Field List
        public int idInput { get; init; }                          // ID Form nhập dữ liệu để lấy chi tiết lỗi

        [System.ComponentModel.DisplayName("Mã kiểm tra")]
        public string MaKT { get; init; } = "";                    // Mã lý do kiểm tra: I, II, III, IV
        public DateTime StartTime { get; init; }                   // Thời gian bắt đầu thao tác
        public string NguoiTT { get; init; } = "";                 // Người thao tác
        public string? TenMay_Ban { get; set; }                    // Tên máy bàn thao tác
        public int SLSudung { get; init; }                         // Số lượng sản phẩm đã sử dụng
        public string? Remark { get; init; }                       // Ghi chú
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
        public string? val16 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val17 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val18 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val19 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val20 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val21 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val22 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val23 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val24 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val25 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val26 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val27 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val28 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val29 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val30 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val31 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
        public string? val32 { get; set; }                         // Kết quả xác nhận tồn lưu yes no
    }
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
        public int Bevel_Cut { get; set; }                         // 1: Lỗi cắt vát
        public int Flat { get; set; }                              // 3: Lỗi bẹp
        public int Bavia { get; set; }                             // 5: Lỗi Bavia
        public int Fall { get; set; }                              // 9: Lỗi rơi
        public int Beyond_The_Standard { get; set; }               // 4: Lỗi Chiều dài ngoài tiêu chuẩn
        public int Other { get; set; }                             // 6: Lỗi khác
        public int Divat { get; set; }                             // 58: Dị vật, bẩn
        public int Lo { get; set; }                             // 128: Lỗ thủng
        public int Cong { get; set; }                             // 41: Cong
        public int LoiLom { get; set; }                             // 29: Lồi lõm
        public int Divatduc { get; set; }                             // 16: Dị vật đúc
        public int Xuoc { get; set; }                             // 17: Xước
        public int Nham { get; set; }                             // 14: Lỗi khác
    }
}
