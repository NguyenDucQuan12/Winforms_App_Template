using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class Input_Error_Model
    {
        public int idInput { get; init; }                          // ID Form nhập dữ liệu để lấy chi tiết lỗi
        public int idError { get; init; }                          // Id lỗi 
        public string? TenLoi { get; set; }                        // Tên lỗi
        public int Qty { get; init; }                              // Số lượng lỗi của id lỗi này
        public int NguoiThaoTac { get; init; }                     // Id người thao tác 
        public string Remark { get; init; } = "";                  // Ghi chú
        public DateTime ThoiGian { get; init; }                    // Thời gian bắt đầu thao tác
    }
}
