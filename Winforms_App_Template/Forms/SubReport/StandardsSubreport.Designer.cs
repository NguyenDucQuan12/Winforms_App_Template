namespace Winforms_App_Template.Forms.SubReport
{
    partial class StandardsSubreport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel1 = new DevExpress.XtraReports.UI.XRLabel();
            this.Check_Table = new DevExpress.XtraReports.UI.XRTable();
            this.xrTableRow17 = new DevExpress.XtraReports.UI.XRTableRow();
            this.xrTableCell126 = new DevExpress.XtraReports.UI.XRTableCell();
            this.Outer_Diameter_Item = new DevExpress.XtraReports.UI.XRTableCell();
            this.Outer_Diameter_Size = new DevExpress.XtraReports.UI.XRTableCell();
            this.Outer_Diameter_Pingauge_Through = new DevExpress.XtraReports.UI.XRTableCell();
            this.Outer_Diameter_Pingauge_Not_Through = new DevExpress.XtraReports.UI.XRTableCell();
            this.Outer_Diameter_Criterion = new DevExpress.XtraReports.UI.XRTableCell();
            this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
            this.xrTable1 = new DevExpress.XtraReports.UI.XRTable();
            this.xrTableRow1 = new DevExpress.XtraReports.UI.XRTableRow();
            this.xrTableCell1 = new DevExpress.XtraReports.UI.XRTableCell();
            this.xrTableCell2 = new DevExpress.XtraReports.UI.XRTableCell();
            this.xrTableCell3 = new DevExpress.XtraReports.UI.XRTableCell();
            this.xrTableCell4 = new DevExpress.XtraReports.UI.XRTableCell();
            this.xrTableCell5 = new DevExpress.XtraReports.UI.XRTableCell();
            this.xrTableCell6 = new DevExpress.XtraReports.UI.XRTableCell();
            ((System.ComponentModel.ISupportInitialize)(this.Check_Table)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xrTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // TopMargin
            // 
            this.TopMargin.HeightF = 0F;
            this.TopMargin.Name = "TopMargin";
            // 
            // BottomMargin
            // 
            this.BottomMargin.HeightF = 0F;
            this.BottomMargin.Name = "BottomMargin";
            // 
            // Detail
            // 
            this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel2,
            this.xrLabel1,
            this.Check_Table});
            this.Detail.HeightF = 18F;
            this.Detail.Name = "Detail";
            // 
            // xrLabel2
            // 
            this.xrLabel2.LocationFloat = new DevExpress.Utils.PointFloat(676.8982F, 0F);
            this.xrLabel2.Multiline = true;
            this.xrLabel2.Name = "xrLabel2";
            this.xrLabel2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2F, 2F, 0F, 0F, 100F);
            this.xrLabel2.SizeF = new System.Drawing.SizeF(426.5624F, 18F);
            this.xrLabel2.StylePriority.UseTextAlignment = false;
            this.xrLabel2.Text = "Công thức tính tiêu chuẩn: Iif([TCMin] == [TCMax], FormatString(\'{0:0.###}\', [TCM" +
    "in]), FormatString(\'{0:0.###} - {1:0.###}\', [TCMin],[TCMax]))";
            this.xrLabel2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.xrLabel2.Visible = false;
            // 
            // xrLabel1
            // 
            this.xrLabel1.LocationFloat = new DevExpress.Utils.PointFloat(484.6065F, 0F);
            this.xrLabel1.Multiline = true;
            this.xrLabel1.Name = "xrLabel1";
            this.xrLabel1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2F, 2F, 0F, 0F, 100F);
            this.xrLabel1.SizeF = new System.Drawing.SizeF(168.75F, 18F);
            this.xrLabel1.StylePriority.UseTextAlignment = false;
            this.xrLabel1.Text = "Bỏ border phía trên của từng hàng\r\nVì nó sẽ bị ghi đè lên nhau và đậm hơn";
            this.xrLabel1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.xrLabel1.Visible = false;
            // 
            // Check_Table
            // 
            this.Check_Table.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.Check_Table.BorderWidth = 0.5F;
            this.Check_Table.Font = new DevExpress.Drawing.DXFont("Times New Roman", 4.5F);
            this.Check_Table.KeepTogether = true;
            this.Check_Table.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.Check_Table.Name = "Check_Table";
            this.Check_Table.Padding = new DevExpress.XtraPrinting.PaddingInfo(2F, 2F, 0F, 0F, 100F);
            this.Check_Table.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.xrTableRow17});
            this.Check_Table.SizeF = new System.Drawing.SizeF(386.2761F, 18F);
            this.Check_Table.StylePriority.UseBorders = false;
            this.Check_Table.StylePriority.UseBorderWidth = false;
            this.Check_Table.StylePriority.UseFont = false;
            this.Check_Table.StylePriority.UseTextAlignment = false;
            this.Check_Table.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // xrTableRow17
            // 
            this.xrTableRow17.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.xrTableCell126,
            this.Outer_Diameter_Item,
            this.Outer_Diameter_Size,
            this.Outer_Diameter_Pingauge_Through,
            this.Outer_Diameter_Pingauge_Not_Through,
            this.Outer_Diameter_Criterion});
            this.xrTableRow17.Name = "xrTableRow17";
            this.xrTableRow17.Weight = 0.15438118141615953D;
            // 
            // xrTableCell126
            // 
            this.xrTableCell126.Multiline = true;
            this.xrTableCell126.Name = "xrTableCell126";
            this.xrTableCell126.Tag = "a|b|TenTieuChuan|d|e";
            this.xrTableCell126.Weight = 0.90431522041003709D;
            // 
            // Outer_Diameter_Item
            // 
            this.Outer_Diameter_Item.Multiline = true;
            this.Outer_Diameter_Item.Name = "Outer_Diameter_Item";
            this.Outer_Diameter_Item.Tag = "a|b|Loai_chieudai|d|e";
            this.Outer_Diameter_Item.Weight = 0.71979121689318681D;
            // 
            // Outer_Diameter_Size
            // 
            this.Outer_Diameter_Size.Multiline = true;
            this.Outer_Diameter_Size.Name = "Outer_Diameter_Size";
            this.Outer_Diameter_Size.Tag = "a|b|Loai_size|d|e";
            this.Outer_Diameter_Size.Weight = 0.62539667967024348D;
            // 
            // Outer_Diameter_Pingauge_Through
            // 
            this.Outer_Diameter_Pingauge_Through.Multiline = true;
            this.Outer_Diameter_Pingauge_Through.Name = "Outer_Diameter_Pingauge_Through";
            this.Outer_Diameter_Pingauge_Through.Tag = "a|b|c|d|e";
            this.Outer_Diameter_Pingauge_Through.Weight = 0.84805106149398335D;
            // 
            // Outer_Diameter_Pingauge_Not_Through
            // 
            this.Outer_Diameter_Pingauge_Not_Through.Multiline = true;
            this.Outer_Diameter_Pingauge_Not_Through.Name = "Outer_Diameter_Pingauge_Not_Through";
            this.Outer_Diameter_Pingauge_Not_Through.Tag = "a|b|c|d|e";
            this.Outer_Diameter_Pingauge_Not_Through.Weight = 1.0805191468485749D;
            // 
            // Outer_Diameter_Criterion
            // 
            this.Outer_Diameter_Criterion.Multiline = true;
            this.Outer_Diameter_Criterion.Name = "Outer_Diameter_Criterion";
            this.Outer_Diameter_Criterion.Tag = "a|b|TCMin|d|e";
            this.Outer_Diameter_Criterion.Weight = 0.84439382534559659D;
            // 
            // ReportHeader
            // 
            this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrTable1});
            this.ReportHeader.HeightF = 20F;
            this.ReportHeader.Name = "ReportHeader";
            // 
            // xrTable1
            // 
            this.xrTable1.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrTable1.BorderWidth = 0.5F;
            this.xrTable1.Font = new DevExpress.Drawing.DXFont("Times New Roman", 4.5F);
            this.xrTable1.KeepTogether = true;
            this.xrTable1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.xrTable1.Name = "xrTable1";
            this.xrTable1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2F, 2F, 0F, 0F, 100F);
            this.xrTable1.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.xrTableRow1});
            this.xrTable1.SizeF = new System.Drawing.SizeF(386.2761F, 20F);
            this.xrTable1.StylePriority.UseBorders = false;
            this.xrTable1.StylePriority.UseBorderWidth = false;
            this.xrTable1.StylePriority.UseFont = false;
            this.xrTable1.StylePriority.UseTextAlignment = false;
            this.xrTable1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // xrTableRow1
            // 
            this.xrTableRow1.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.xrTableCell1,
            this.xrTableCell2,
            this.xrTableCell3,
            this.xrTableCell4,
            this.xrTableCell5,
            this.xrTableCell6});
            this.xrTableRow1.Name = "xrTableRow1";
            this.xrTableRow1.Weight = 0.17153466126633193D;
            // 
            // xrTableCell1
            // 
            this.xrTableCell1.Multiline = true;
            this.xrTableCell1.Name = "xrTableCell1";
            this.xrTableCell1.Text = "検査項目\r\nHạng mục kiểm tra";
            this.xrTableCell1.Weight = 0.90431522041003709D;
            // 
            // xrTableCell2
            // 
            this.xrTableCell2.Multiline = true;
            this.xrTableCell2.Name = "xrTableCell2";
            this.xrTableCell2.Text = "品種\r\nChủng loại (cm)";
            this.xrTableCell2.Weight = 0.71979121689318681D;
            // 
            // xrTableCell3
            // 
            this.xrTableCell3.Multiline = true;
            this.xrTableCell3.Name = "xrTableCell3";
            this.xrTableCell3.Text = "Frサイズ\r\nKích cỡ Fr";
            this.xrTableCell3.Weight = 0.62539667967024348D;
            // 
            // xrTableCell4
            // 
            this.xrTableCell4.Multiline = true;
            this.xrTableCell4.Name = "xrTableCell4";
            this.xrTableCell4.Text = "通過検査ピンゲージ　\r\nPingauge xuyên (mm)";
            this.xrTableCell4.Weight = 0.84805106149398335D;
            // 
            // xrTableCell5
            // 
            this.xrTableCell5.Multiline = true;
            this.xrTableCell5.Name = "xrTableCell5";
            this.xrTableCell5.Text = "不通過検査ピンゲージ\r\nPingauge không xuyên (mm)";
            this.xrTableCell5.Weight = 1.0805191468485749D;
            // 
            // xrTableCell6
            // 
            this.xrTableCell6.Multiline = true;
            this.xrTableCell6.Name = "xrTableCell6";
            this.xrTableCell6.Text = "基準\r\nTiêu chuẩn";
            this.xrTableCell6.Weight = 0.84439382534559659D;
            // 
            // StandardsSubreport
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail,
            this.ReportHeader});
            this.Font = new DevExpress.Drawing.DXFont("Times New Roman", 4.5F);
            this.Margins = new DevExpress.Drawing.DXMargins(10F, 10F, 0F, 0F);
            this.PageHeightF = 1653.543F;
            this.PageWidthF = 1169.291F;
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A3;
            this.Version = "25.1";
            ((System.ComponentModel.ISupportInitialize)(this.Check_Table)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xrTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.XtraReports.UI.XRTable Check_Table;
        private DevExpress.XtraReports.UI.XRTableRow xrTableRow17;
        private DevExpress.XtraReports.UI.XRTableCell xrTableCell126;
        private DevExpress.XtraReports.UI.XRTableCell Outer_Diameter_Item;
        private DevExpress.XtraReports.UI.XRTableCell Outer_Diameter_Size;
        private DevExpress.XtraReports.UI.XRTableCell Outer_Diameter_Pingauge_Through;
        private DevExpress.XtraReports.UI.XRTableCell Outer_Diameter_Pingauge_Not_Through;
        private DevExpress.XtraReports.UI.XRTableCell Outer_Diameter_Criterion;
        private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
        private DevExpress.XtraReports.UI.XRTable xrTable1;
        private DevExpress.XtraReports.UI.XRTableRow xrTableRow1;
        private DevExpress.XtraReports.UI.XRTableCell xrTableCell1;
        private DevExpress.XtraReports.UI.XRTableCell xrTableCell2;
        private DevExpress.XtraReports.UI.XRTableCell xrTableCell3;
        private DevExpress.XtraReports.UI.XRTableCell xrTableCell4;
        private DevExpress.XtraReports.UI.XRTableCell xrTableCell5;
        private DevExpress.XtraReports.UI.XRTableCell xrTableCell6;
        private DevExpress.XtraReports.UI.XRLabel xrLabel1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel2;
    }
}
