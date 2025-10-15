using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Entities
{
    public class ThongTinNhanVien
    {
        public string CodeEmp { get; set; } = "";   // PK - Mã nhân viên / Primary Key

        public string ProfileName { get; set; } = "";// Họ tên / Full name
        public string Gender { get; set; } = "";     // Giới tính (Nam/Nữ/Khác...) / Gender
        public DateTime? DateHire { get; set; }      // Ngày vào làm / Hire date (nullable)
        public string BoPhan { get; set; } = "";     // Bộ phận / Department
        public DateTime? DateQuit { get; set; }      // Ngày nghỉ việc (nếu có) / Quit date (nullable)
        public string StatusSyn { get; set; } = "";  // Trạng thái đồng bộ / Sync status
    }
}
