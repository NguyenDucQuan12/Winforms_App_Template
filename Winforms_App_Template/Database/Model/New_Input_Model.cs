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
        public string MaKT { get; init; } = "N/A";                    // Mã lý do kiểm tra: I, II, III, IV
        public string Ly_do_kiem_tra { get; init; } = "";          // Lý do kiểm tra nếu mã kiểm tra là V: khác
        public DateTime StartTime { get; init; }                      // Thời gian bắt đầu thao tác
        public string NguoiTT { get; init; } = "N/A";                 // Người thao tác
        public string TenNguoiThaoTac { get; init; } = "N/A";         // Tên người thao tác
        public string? TenMay_Ban { get; set; } = "N/A";              // Tên máy bàn thao tác
        public int SLSudung { get; init; }                            // Số lượng sản phẩm đã sử dụng
        public int OKQty { get; init; }                               // Số lượng hàng phù hợp
        public int NGQty { get; init; }                               // Số lượng hàng không phù hợp
        public string? Remark { get; init; } = "N/A";                 // Ghi chú
        public string? val1 { get; set; } = "N/A";                    // Số lượng ống dài sử dụng
        public string? val2 { get; set; } = "N/A";                    // Số lượng ống dài cắt được
        public string? val3 { get; set; } = "N/A";                    // Mã quản lý thickness gauge
        public string? val4 { get; set; } = "N/A";                    // Đường kính ngoài ống dài
        public string? val5 { get; set; } = "N/A";                    // Đường kính ngoài ống dài yes no
        public string? val6 { get; set; } = "N/A";                    // Mã pingauge 098mm
        public string? val7 { get; set; } = "N/A";                    // Đường kính trong loại 4Fr, 4KFr xuyên (yes no)
        public string? val8 { get; set; } = "N/A";                    // Đường kính trong loại 4Fr, 4KFr không xuyên (yes no)
        public string? val9 { get; set; } = "N/A";                    // Trạng thái cắt 10 ống
        public string? val10 { get; set; } = "N/A";                   // Mã thước sử dụng
        public string? val11 { get; set; } = "N/A";                   // Thước sử dụng 1
        public string? val12 { get; set; } = "N/A";                   // Thước sử dụng 2
        public string? val13 { get; set; } = "N/A";                   // Thước sử dụng 3
        public string? val14 { get; set; } = "N/A";                   // Thước sử dụng yes no
        public string? val15 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val16 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val17 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val18 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val19 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val20 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val21 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val22 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val23 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val24 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val25 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val26 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val27 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val28 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val29 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val30 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val31 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val32 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val33 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val34 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val35 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val36 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val37 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val38 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val39 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val40 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val41 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val42 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
        public string? val43 { get; set; } = "N/A";                   // Kết quả xác nhận tồn lưu yes no
    }
}
