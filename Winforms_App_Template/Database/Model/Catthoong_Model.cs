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
        public string Reason { get; set; } = ""; // Lý do kiểm tra
        public string TimeAndWorker { get; set; } = ""; // "HH:mm yyyy-MM-dd\nNgười thao tác"
        public string Machine { get; set; } = ""; // Số máy
        public int QtyUsed { get; set; }       // Số lượng sử dụng
        public int QtyCut { get; set; }       // Số ống cắt
        public string ThickGauge { get; set; } = ""; // PR-IK-...
        public string OuterDiameter { get; set; } = ""; // "⌀ 2.80"
        public string Pin098 { get; set; } = ""; // "OK"/"NG"
        public string InnerJudge { get; set; } = ""; // "通過 / 不通過"...
        public string CutState { get; set; } = ""; // "OK"/"NG"
        public string CutLengths { get; set; } = ""; // "100 / 100 / 100"
        public string Acceptance { get; set; } = ""; // "OK"/"NG"

        // (tuỳ chọn) Các trường NG phía dưới
        public int? NG_CatVat { get; set; }
        public int? NG_Bep { get; set; }
        public int? NG_Bavia { get; set; }
        public int? NG_Roi { get; set; }
        public int? NG_LengthOut { get; set; }
        public int? NG_Khac { get; set; }
    }
}
