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

            var detail = Bands[BandKind.Detail] as DetailBand
                 ?? throw new InvalidOperationException("No Detail band");
            var tblB = FindControl("Check_Table", true) as XRTable
                       ?? throw new InvalidOperationException("Missing Check_Table");

            // Bind expression từ Tag (chưa cần DataSource)
            XtraTableAutoBinder.AutoBindSingleTableByTag(tblB);

            this.DataSourceDemanded += (s, e) =>
            {
                // Subreport hiện đang có DataSource là Dictionary<int, List<Standard_Model>>
                var ds = this.DataSource;

                // Mặc định: rỗng
                IEnumerable<Standard_Model> list = Enumerable.Empty<Standard_Model>();

                // Lấy idInput hiện tại do parent truyền vào
                int id = 0;
                var pid = this.Parameters["pIdInput"]?.Value;
                if (pid != null) int.TryParse(pid.ToString(), out id);

                if (ds is IDictionary<int, List<Standard_Model>> map)
                {
                    // Tra map để lấy list cho idInput hiện tại
                    if (id != 0 && map.TryGetValue(id, out var bucket) && bucket != null)
                        list = bucket;
                }
                else if (ds is IEnumerable<Standard_Model> ready) // phòng trường hợp bạn truyền thẳng list
                {
                    list = ready;
                }

                // ĐỔI DataSource của *instance đang in* thành list con
                this.DataSource = list;
                this.DataMember = null;

                // (Tuỳ chọn) nếu muốn xác thực field tồn tại theo schema thực tế:
                if (tblB != null)
                {
                    XtraTableAutoBinder.AutoBindSingleTableByTag(
                        table: tblB,
                        checkFieldExists: true,
                        dataSourceForCheck: this.DataSource
                    );
                }

                // (Tuỳ chọn) Không in gì nếu DataSource rỗng (tránh header trống)
                this.PrintOnEmptyDataSource = false; // hoặc this.ReportPrintOptions.PrintOnEmptyDataSource = false;
            };
        }
    }
}
