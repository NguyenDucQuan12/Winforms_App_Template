using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class Report_Header_Model
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

    public sealed record Step_Definition(
        int Id,                           // ID công đoạn, ví dụ: 68, 144
        string BandName,                  // Tên DetailReportBand trong layout (ví dụ: "Catthoong, ...")
        string StandardsSubreportName,    // Tên XRSubreport chứa bảng tiêu chuẩn trong band (ví dụ: "SR_Standards_68")
        string HeaderParamPrefix          // Prefix đẩy tham số header (ví dụ: "cd68_")
    );

    /// <summary>
    /// Dữ liệu cho 1 công đoạn nhỏ trong 1 báo cáo
    /// </summary>
    public sealed class Data_Step_Model
    {
        public required int Id { get; init; }
        public required Report_Header_Model Header { get; init; }          // Dữ liệu cho tiêu đề trong báo cáo
        public required List<Que_Nong_Rows> Rows { get; init; }            // Dữ liệu chi tiết cho bảng trong
        public required Dictionary<int, List<Standard_Model>> StandardsByInput { get; init; }   // Dữ liệu cho bảng tiêu chuẩn, theo idInput
    }

}
