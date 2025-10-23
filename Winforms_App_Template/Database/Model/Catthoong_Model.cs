using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class Catongtho_HeaderModel
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

    public sealed class Catthoong_Model
    {
        public string Reason { get; set; } = "";            // Lý do kiểm tra
        public string TimeAndWorker { get; set; } = "";     // "HH:mm yyyy-MM-dd\nNgười thao tác"
        public string Machine { get; set; } = "";           // Số máy
        public int QtyUsed { get; set; }       // Số lượng sử dụng
        public int QtyCut { get; set; }       // Số ống cắt
        public string ThickGauge { get; set; } = ""; // PR-IK-...
        public string OuterDiameter { get; set; } = ""; // "⌀ 2.80"
        public string Pin098 { get; set; } = ""; // "OK"/"NG"
        public string InnerJudge { get; set; } = ""; // "通過 / 不通過"...
        public string CutState { get; set; } = ""; // "OK"/"NG"
        public string CutLengths { get; set; } = ""; // "100 / 100 / 100"
        public string Acceptance { get; set; } = ""; // "OK"/"NG"
        public int? NG_CatVat { get; set; }
        public int? NG_Bep { get; set; }
        public int? NG_Bavia { get; set; }
        public int? NG_Roi { get; set; }
        public int? NG_LengthOut { get; set; }
        public int? NG_Khac { get; set; }
    }

    public sealed class Catthoong_Row
    {
        public int idInput { get; init; }                          // ID Form nhập dữ liệu để lấy chi tiết lỗi
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
        public string? Remark { get; init; }                         // Số lượng sản phẩm đã sử dụng
    }

    public sealed class NewInput_Row
    {
        public List<Catthoong_Row> Rows { get; set; } = new();
    }
}
