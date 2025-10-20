using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Collections.Generic;

namespace Winforms_App_Template.Forms
{
    public partial class Testreport : DevExpress.XtraReports.UI.XtraReport
    {
        public Testreport()
        {
            InitializeComponent();
            this.InitializeBindings_ByDictionary();
        }

        /// <summary>
        /// Cách 2: cấu hình map cell->expression ở 1 chỗ, sau đó duyệt map để bind.
        /// </summary>
        private void InitializeBindings_ByDictionary()
        {
            XRTable table = this.GetRequiredTableByName("Catongtho_Table");

            Dictionary<string, string> map = new Dictionary<string, string>();

            // Map tên cell -> field trong Catthoong_Model
            map.Add("Reason_Check", "[Reason]");
            map.Add("Time_Check", "[TimeAndWorker]");
            map.Add("Person_Check", "''");                    // bạn chưa có field riêng -> để rỗng, hoặc thêm property Worker
            map.Add("Machine_number", "[Machine]");
            map.Add("Raw_Materials", "''");                    // chưa có trong model mẫu
            map.Add("Number_Of_Cut_Pipes", "[QtyCut]");
            map.Add("Thickness_Gauge_Code", "[ThickGauge]");
            map.Add("Outer_Diameter", "[OuterDiameter]");
            map.Add("Outer_Diameter_Check", "Iif(Not IsNullOrEmpty([OuterDiameter]), 'OK', 'NG')"); // ví dụ
            map.Add("Pingauge_Code", "[Pin098]");
            map.Add("Cut_Check", "[CutState]");            // OK/NG 10 ống
            map.Add("Cutting_Length", "[CutLengths]");          // bạn đang gộp 3 số vào 1 chuỗi
            map.Add("Cutting_Length_Number_1", "''");                    // model mẫu chưa có 3 số riêng
            map.Add("Cutting_Length_Number_2", "''");
            map.Add("Cutting_Length_Number_3", "''");
            map.Add("Cutting_Length_Check", "[CutState]");            // tạm dùng lại, hoặc tạo field riêng
            map.Add("Number_Of_Use", "[QtyUsed]");
            map.Add("Bevel_Cut", "[NG_CatVat]");
            map.Add("Flat", "[NG_Bep]");
            map.Add("Bavia", "[NG_Bavia]");
            map.Add("Fall", "[NG_Roi]");
            map.Add("Beyond_The_Standard", "[NG_LengthOut]");
            map.Add("Other", "[NG_Khac]");
            map.Add("Check_Inventory", "[Acceptance]");          // Kết quả xác nhận tồn lưu

            foreach (KeyValuePair<string, string> kv in map)
            {
                XRControl cell = table.FindControl(kv.Key, true) as XRControl;
                if (cell == null)
                {
                    throw new InvalidOperationException("Thiếu cell: " + kv.Key);
                }
                this.SetTextBinding(cell, kv.Value);
            }
        }

        /// <summary>
        /// Tìm đúng bảng theo tên, chỉ trong Detail (đỡ quét toàn bộ report).
        /// Ném lỗi rõ ràng nếu thiếu để dễ debug.
        /// </summary>
        private XRTable GetRequiredTableByName(string tableName)
        {
            DetailBand detailBand = this.Bands.GetBandByType(typeof(DetailBand)) as DetailBand;
            if (detailBand == null)
            {
                throw new InvalidOperationException("Report chưa có DetailBand, không thể tìm bảng.");
            }

            XRControl found = detailBand.FindControl(tableName, true);
            XRTable table = found as XRTable;
            if (table == null)
            {
                throw new InvalidOperationException("Không tìm thấy XRTable tên: " + tableName);
            }
            return table;
        }

        /// <summary>
        /// Gán 1 ExpressionBinding cho thuộc tính Text ở thời điểm BeforePrint.
        /// Xoá binding cũ để tránh chồng chéo.
        /// </summary>
        private void SetTextBinding(XRControl control, string expression)
        {
            control.ExpressionBindings.Clear();
            control.ExpressionBindings.Add(
                new ExpressionBinding("BeforePrint", "Text", expression)
            );
        }
    }
}
