using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class NewInput_Row
    {
        public int IdCongDoan { get; init; }              // MÃ công đoạn
        public int IdMay_ban { get; init; }               // ID của máy bàn sử dụng
        public string? TenMay { get; set; }        // thêm
        public string? TenCongDoan { get; set; }   // thêm
        public int IdLydoKT { get; init; }                // ID lý do kiểm tra
        public int SLSudung { get; init; }                // Số lượng sản phẩm đã sử dụng
        public int OKQty { get; init; }                   // Số lượng sản phẩm phù hợp
        public int NGQty { get; init; }                   // Số lượng sản phẩm không phù hợp
        public string ItemNumber { get; init; } = "";     // Mã sản phẩm
        public string LotNo { get; init; } = "";          // Số lô
        public string So_Me { get; init; } = "";          // Số mẻ
        public DateTime StartTime { get; init; }          // Thời gian bắt đầu
        public string NguoiTT { get; init; } = "";        // Người thao tác
        public string Remark { get; init; } = "";         // Ghi chú
        public string val1 { get; init; } = "";          // Số lượng ống dài sử dụng (val1)
        public string val2 { get; init; } = "";              // Số ống cắt được (val2)
        public string val3 { get; init; }= "";       // Mã quản lý thickness (val3)
        public string val4 { get; init; }= "";       // đường kính ngoài ống dài (val4)
        public string val5 { get; init; }= ""; // đường kính ngoài ống dài yes no (val5)
        public string val6 { get; init; } = "";     // MÃ pinguage (val6)
        public string val7 { get; init; }= "";    // đường kính trong xuyên (val7)
        public string val8 { get; init; }= ""; // đường kính trong không xuyên (val8)
        public string val9 { get; init; }= "";           // Tạng thái cắt (val9)
        public string val10 { get; init; } = "";          // Thước sử dụng (val10)
        public string val11 { get; init; }= "";           // Thước sử dụng 1 (val11)
        public string val12 { get; init; }= "";          // Thước sử dụng 2 (val12)
        public string val13 { get; init; }= "";         // Thước sử dụng 3 (val13)
        public string val14 { get; init; }= "";           // Thước sử dụng yesno (val14)
        public string val15 { get; init; }= "";           // Xác nhận tồn kho (val15)
    }

    public sealed class NewInput_Model
    {
        public List<NewInput_Row> Rows { get; set; } = new();
    }
}
