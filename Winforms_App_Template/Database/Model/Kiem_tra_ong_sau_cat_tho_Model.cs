using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class Kiem_tra_ong_sau_cat_tho_Model
    {
        public DateTime StartTime { get; init; }              // Thời gian bắt đầu thao tác
        public string NguoiTT { get; init; } = "";            // Người thao tác
        public string? TenMay_Ban { get; set; }               // Tên máy bàn thao tác
        public int SLSudung { get; init; }                    // Số lượng sản phẩm đã sử dụng
        public string? Remark { get; init; }                  // Số lượng sản phẩm đã sử dụng
        public string val1 { get; set; } = "";                // Mã pingauge 1
        public string val2 { get; set; } = "";                // Mã pingauge 2
        public string val3 { get; set; } = "";                // Độ dài pingauge 1
        public string val4 { get; set; } = "";                // Độ dài pingauge 2
        public string val5 { get; set; } = "";                // Số lượng kiểm tra 1
        public string val6 { get; set; } = "";                // Số lượng kiểm tra 2
        public string val7 { get; set; } = "";                // Số lượng OK 1
        public string val8 { get; set; } = "";                // Số lượng OK 2
        public string val9 { get; set; } = "";                // Số lượng NG 1
        public string val10 { get; set; } = "";               // Số lượng NG 2
        public string val11 { get; set; } = "";               // Mã quản lý mẫu giới hạn
        public string val12 { get; set; } = "";               // Tổng số hàng chuyển công đoạn sau
        public string val13 { get; set; } = "";               // Kết quả xác nhận tồn lưu

        // --- phần chi tiết lỗi ---
        public int Bevel_Cut { get; set; }                    // Lỗi cắt vát
        public int Flat { get; set; }                         // Lỗi bẹp
        public int Bavia { get; set; }                        // Lỗi Bavia
        public int Fall { get; set; }                         // Lỗi rơi
        public int Beyond_The_Standard { get; set; }          // Lỗi Chiều dài ngoài tiêu chuẩn
        public int Other { get; set; }                        // Lỗi khác
    }
}
