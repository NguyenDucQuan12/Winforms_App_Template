using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class MasterForm_Control_Model
    {
        public int Id { get; set; }                // id (PK)
        public string ControlCode { get; set; } = default!;  // ControlCode: a|b|DBColumn|length|xxx|FriendlyName|xxx
        public int IdMasterForm { get; set; }      // idMasterForm nhóm
        public string DBColumn { get; set; } = default!;     // DBColumn (val1, val2, ...)
        public string FriendlyName { get; set; } = default!; // FriendlyName (label hiển thị)
    }
}
