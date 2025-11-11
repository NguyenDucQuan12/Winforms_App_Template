using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class MasterForm_Model
    {
        public int Id { get; set; }                // id (PK)
        public string idCongDoan { get; set; } = default!;  // ControlCode
        public int Ver { get; set; }      // idMasterForm nhóm
        public int idLoaiCS { get; set; }  // DBColumn (val1, val2, ...)
        public DateTime Fromdate { get; set; } = default!; // FriendlyName (label hiển thị)
        public DateTime ToDate { get; set; } = default!; // FriendlyName (label hiển thị)
        public string BackgroundPicture { get; set; } = default!; // FriendlyName (label hiển thị)
        public DateTime Ngaydangky { get; set; } = default!; // FriendlyName (label hiển thị)
        public string ChecksheetName { get; set; } = default!; // FriendlyName (label hiển thị)
        public int idErrorMaster { get; set; } // FriendlyName (label hiển thị)
        public bool isDieuKienMay { get; set; } = default!; // FriendlyName (label hiển thị)
        public bool isTTBS1 { get; set; } = default!; // FriendlyName (label hiển thị)
        public bool isTTBS2 { get; set; } = default!; // FriendlyName (label hiển thị)
    }
}
