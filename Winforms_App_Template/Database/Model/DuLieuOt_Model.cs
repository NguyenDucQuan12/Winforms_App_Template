using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class DuLieuOt_Model  // sealed: cấm kế thừa (bảo toàn bất biến)
    {
        // Các auto-properties “init-only”:
        // - get: có thể đọc ở mọi nơi
        // - init: chỉ gán khi khởi tạo object (object initializer) hoặc từ constructor của lớp này (hoặc lớp con)
        public string CodeEmp { get; init; } = "";           // Mã nhân viên
        public string ProfileName { get; init; } = "";       // Họ tên nhân viên
        public string BoPhan { get; init; } = "";            //  Bộ phận của nhân viên
        public DateTime WorkDateRoot { get; init; }          // Thời gian tăng ca
        public DateTime InTime { get; init; }                // Thời gian bắt đầu tăng ca
        public DateTime OutTime { get; init; }               // Thời gian kết thúc tăng ca
        public decimal RegisterHours { get; init; }          // Thời gian tăng ca
        public string OvertimeTypeName { get; init; } = "";  // Loại tăng ca
        public string Status { get; init; } = "";            // Trạng thái phê duyệt dữ liệu
    }

    public sealed class PageRequest
    {
        public int Page { get; init; } = 1;             // Trang bắt đầu từ 1
        public int Size { get; init; } = 50;            // Kích cỡ trang
        public string? SortBy { get; init; }            // Cột sắp xếp (whitelist để chống SQL injection)
        public bool Desc { get; init; }                 // true = DESC
        public string? CodeEmp { get; init; }           // Lọc theo mã NV
        public DateTime? FromDate { get; init; }        // Lọc từ ngày
        public DateTime? ToDate { get; init; }          // Lọc đến ngày (inclusive)
    }

    public sealed class PageResult<T>
    {
        public required IReadOnlyList<T> Items { get; init; }  // Dữ liệu trang
        public required int Page { get; init; }                // Trang hiện tại
        public required int Size { get; init; }                // Kích cỡ trang
        public required long Total { get; init; }              // Tổng số dòng (có thể tốn kém → cân nhắc cho phép bỏ qua)
        public int TotalPages => (int)Math.Ceiling((double)Total / Size);
        public bool HasNext => Page < TotalPages;
    }
}
