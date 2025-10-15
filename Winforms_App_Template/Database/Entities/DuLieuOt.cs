using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Entities
{
    /// <summary>
    /// POCO cho bảng [dbo].[DuLieuOT]. Khai báo schema cho một bảng/khung dữ liệu EF sẽ làm việc.
    /// Lưu ý: bảng này thường KHÔNG có khóa chính rõ ràng => ta map "keyless" để đọc/phân trang.
    /// Nếu bạn muốn cập nhật/track đầy đủ bằng EF, nên bổ sung cột PK (ví dụ: bigint [Id] IDENTITY).
    /// </summary>
    public class DuLieuOt
    {
        public string CodeEmp { get; set; } = "";        // Mã nhân viên
        public DateTime WorkDateRoot { get; set; }       // Ngày làm việc (date/datetime)

        public string ProfileName { get; set; } = "";    // Họ tên
        public string BoPhan { get; set; } = "";         // Bộ phận
        public TimeSpan? InTime { get; set; }            // Giờ vào (time) -> TimeSpan trong .NET
        public TimeSpan? OutTime { get; set; }           // Giờ ra (time)
        public decimal? RegisterHours { get; set; }      // Số giờ đăng ký (decimal)
        public string OvertimeTypeName { get; set; } = "";// Tên loại OT
        public string Status { get; set; } = "";         // Trạng thái (text/int tùy DB)
    }
}
