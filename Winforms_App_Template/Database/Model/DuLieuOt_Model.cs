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
        public string CodeEmp { get; init; } = "";      // ID người chuyển
        public string ProfileName { get; init; } = "";      // ID người nhận
        public string BoPhan { get; init; } = "";     // Số tiền chuyển (decimal: phù hợp cho tiền tệ)
        public DateTime InTime { get; init; } // Số dư người chuyển sau giao dịch
        public DateTime OutTime { get; init; } // Số dư người nhận sau giao dịch
        public DateTime RegisterHours { get; init; } // Số dư người nhận sau giao dịch
        public string OvertimeTypeName { get; init; } = ""; // Số dư người nhận sau giao dịch
        public string Status { get; init; } = ""; // Số dư người nhận sau giao dịch
    }
}
