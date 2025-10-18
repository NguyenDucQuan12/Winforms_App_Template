using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class MayBan_Model
    {
        // Đổi sang PascalCase để dễ map + dùng alias trong SQL
        public int IdMayBan { get; init; }          // idMay_Ban
        public int IdCongDoan { get; init; }        // idCongDoan
        public string TenMayBan { get; init; } = ""; // TenMay_Ban (tên hiển thị)
        public string MaMayBan { get; init; } = ""; // MaMay_Ban (mã nội bộ)
        public string MoTa { get; init; } = ""; // MoTa
        public bool Active { get; init; }       // Active
        public bool PLC { get; init; }       // PLC
    }
}
