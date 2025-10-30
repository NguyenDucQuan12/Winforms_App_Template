using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    // DÒNG REPORT-READY (đã gộp main + error) ==
    public sealed class Catthoong_ReportRow
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
        public int SLSudung { get; init; }                         // Số lượng sản phẩm đã sử dụng
        public string? Remark { get; init; }                       // Số lượng sản phẩm đã sử dụng

        // --- phần chi tiết lỗi ---
        public int Bevel_Cut { get; set; }                         // Lỗi cắt vát
        public int Flat { get; set; }                              // Lỗi bẹp
        public int Bavia { get; set; }                             // Lỗi Bavia
        public int Fall { get; set; }                              // Lỗi rơi
        public int Beyond_The_Standard { get; set; }               // Lỗi Chiều dài ngoài tiêu chuẩn
        public int Other { get; set; }                             // Lỗi khác

        // Phần bảng tiêu chuẩn
        public List<Standard_Model> Standards { get; set; } = new();
    }

    static readonly DesignSchema.ColumnSpec[] MAIN_COLUMNS =
{
    new() { Name = "MaKT",        Type = typeof(string) },
    new() { Name = "StartTime",   Type = typeof(DateTime) },
    new() { Name = "NguoiTT",     Type = typeof(string) },
    new() { Name = "TenMay_Ban",  Type = typeof(string) },
    new() { Name = "SLSudung",    Type = typeof(int)     },
    new() { Name = "Remark",      Type = typeof(string) },

    // Mở rộng dần:
    // new() { Name = "val1", Type = typeof(string) },
    // new() { Name = "Bevel_Cut", Type = typeof(int) },
    // ...
};

        // WHITELIST cho subreport (Standard_Model)
        static readonly DesignSchema.ColumnSpec[] SUB_COLUMNS =
        {
    new() { Name = "TenTieuChuan", Type = typeof(string) },
    new() { Name = "MaTieuChuan",  Type = typeof(string) },
    new() { Name = "TCMin",        Type = typeof(string) },
    new() { Name = "TCMax",        Type = typeof(string) },

    // mở rộng khi cần: Loai_size, Loai_kytu, ...
};
    }
