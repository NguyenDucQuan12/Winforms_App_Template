using DevExpress.XtraReports.UI;
using Polly.Caching;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Utils;

namespace Winforms_App_Template.Forms.SubReport
{
    public partial class StandardsSubreport : DevExpress.XtraReports.UI.XtraReport
    {
        public StandardsSubreport()
        {
            InitializeComponent();

            // Kiểm tra xem đã tồn tại parameter chưa
            //if (Parameters["pIdInput"] == null)
            //{
            //    // Tạo 1 parameter mới để sử dụng truy vấn dữ liệu từ Datasource truyền vào từ report cha
            //    Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
            //    {
            //        Name = "pIdInput",        // Tên phải trùng với tên bind ở report cha
            //        Type = typeof(int),       // Kiểu int (khóa idInput)
            //        Visible = false,          // Ẩn khỏi UI, chỉ dùng nội bộ để lọc
            //        Value = 0                 // Giá trị mặc định (không in nếu không có)
            //    });
            //}

            //// Gán detail band và bảng để bind dữ liệu
            //var detail = Bands[BandKind.Detail] as DetailBand
            //     ?? throw new InvalidOperationException("No Detail band");

            //// Tìm bảng có tên là "Check_Table" trong detail
            //var tblB = FindControl("Check_Table", true) as XRTable
            //           ?? throw new InvalidOperationException("Missing Check_Table");

            //// Bind expression từ Tag (chưa cần DataSource)
            //XtraTableAutoBinder.AutoBindSingleTableByTag(tblB);

            //// Câu lệnh xử lý khi subreport cần dữ liệu, có nghĩa là mỗi khi in 1 instance subreport
            //this.DataSourceDemanded += (s, e) =>
            //{
            //    // Subreport hiện đang có DataSource là Dictionary<int, List<Standard_Model>> được truyền từ report cha
            //    var ds = this.DataSource;

            //    // Tiến hành chuẩn hóa lại thông tin Datasource cho subreport
            //    // Mặc định ban đầu là rỗng
            //    IEnumerable<Standard_Model> list = Enumerable.Empty<Standard_Model>();

            //    // Lấy idInput hiện tại do report cha truyền vào
            //    int id = 0;
            //    var pid = this.Parameters["pIdInput"]?.Value;
            //    if (pid != null) int.TryParse(pid.ToString(), out id);

            //    // Kiểm tra kiểu DataSource hiện tại để trích xuất dữ liệu con tương ứng
            //    if (ds is IDictionary<int, List<Standard_Model>> map)
            //    {
            //        // Tra map để lấy list cho idInput hiện tại, tương ứng với lần nhập có idInput đó
            //        if (id != 0 && map.TryGetValue(id, out var bucket) && bucket != null) // Kiểm tra id và tiến hành lấy bucket nếu có
            //            list = bucket;
            //    }
            //    else if (ds is IEnumerable<Standard_Model> ready) // phòng trường hợp Datasource truyền thẳng list
            //    {
            //        list = ready;
            //    }

            //    /// ĐỔI DataSource của *instance đang in* thành list con vừa lấy được
            //    this.DataSource = list;  // Từ đây subreport hoạt động trên list con (đúng với từng idInput)
            //    this.DataMember = null;  // IEnumerable không cần DataMember

            //    // nếu muốn xác thực field tồn tại theo schema thực tế:
            //    if (tblB != null)
            //    {
            //        XtraTableAutoBinder.AutoBindSingleTableByTag(
            //            table: tblB,
            //            checkFieldExists: true,
            //            dataSourceForCheck: this.DataSource
            //        );
            //    }
            //};
        }
    }
}
