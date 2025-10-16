namespace Winforms_App_Template.Forms
{
    partial class DevExpress_Form : Form
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DevExpress.XtraGrid.GridLevelNode gridLevelNode2 = new DevExpress.XtraGrid.GridLevelNode();
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            repositoryItemDateTimeOffsetEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemDateTimeOffsetEdit();
            duLieuOtTableBindingSource = new BindingSource(components);
            bandedGridView1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
            bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            MaNV = new DevExpress.XtraGrid.Columns.GridColumn();
            Hoten = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn7 = new DevExpress.XtraGrid.Columns.GridColumn();
            bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn10 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn11 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn12 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn13 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn14 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            bandedGridColumn15 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            gridBand10 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand11 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand4 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand6 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand7 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand12 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand8 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            gridBand9 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemDateTimeOffsetEdit1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)duLieuOtTableBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bandedGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            SuspendLayout();
            // 
            // gridControl1
            // 
            gridControl1.Dock = DockStyle.Fill;
            gridLevelNode2.LevelTemplate = bandedGridView1;
            gridLevelNode2.RelationName = "Level1";
            gridControl1.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] { gridLevelNode2 });
            gridControl1.Location = new Point(0, 0);
            gridControl1.MainView = gridView1;
            gridControl1.Name = "gridControl1";
            gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { repositoryItemDateTimeOffsetEdit1 });
            gridControl1.Size = new Size(1008, 577);
            gridControl1.TabIndex = 0;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { bandedGridView1, gridView1 });
            // 
            // repositoryItemDateTimeOffsetEdit1
            // 
            repositoryItemDateTimeOffsetEdit1.AutoHeight = false;
            repositoryItemDateTimeOffsetEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemDateTimeOffsetEdit1.Name = "repositoryItemDateTimeOffsetEdit1";
            // 
            // duLieuOtTableBindingSource
            // 
            duLieuOtTableBindingSource.DataSource = typeof(Database.Table.DuLieuOt_Table);
            // 
            // bandedGridView1
            // 
            bandedGridView1.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] { gridBand10, gridBand11, gridBand3, gridBand7 });
            bandedGridView1.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] { bandedGridColumn1, bandedGridColumn2, bandedGridColumn3, bandedGridColumn4, bandedGridColumn5, bandedGridColumn6, bandedGridColumn7, bandedGridColumn8, bandedGridColumn9, bandedGridColumn10, bandedGridColumn11, bandedGridColumn12, bandedGridColumn13, bandedGridColumn14, bandedGridColumn15 });
            bandedGridView1.GridControl = gridControl1;
            bandedGridView1.Name = "bandedGridView1";
            // 
            // bandedGridColumn1
            // 
            bandedGridColumn1.Caption = "bandedGridColumn1";
            bandedGridColumn1.Name = "bandedGridColumn1";
            bandedGridColumn1.Visible = true;
            bandedGridColumn1.Width = 134;
            // 
            // bandedGridColumn2
            // 
            bandedGridColumn2.Caption = "bandedGridColumn2";
            bandedGridColumn2.Name = "bandedGridColumn2";
            bandedGridColumn2.Visible = true;
            bandedGridColumn2.Width = 134;
            // 
            // gridView1
            // 
            gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { MaNV, Hoten, gridColumn1, gridColumn2, gridColumn3, gridColumn4, gridColumn5, gridColumn6, gridColumn7 });
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
            // 
            // MaNV
            // 
            MaNV.Caption = "Mã nhân viên";
            MaNV.FieldName = "CodeEmp";
            MaNV.Name = "MaNV";
            MaNV.Visible = true;
            MaNV.VisibleIndex = 0;
            // 
            // Hoten
            // 
            Hoten.Caption = "Họ và tên";
            Hoten.FieldName = "ProfileName";
            Hoten.Name = "Hoten";
            Hoten.Visible = true;
            Hoten.VisibleIndex = 1;
            // 
            // gridColumn1
            // 
            gridColumn1.Caption = "Bộ phận";
            gridColumn1.FieldName = "BoPhan";
            gridColumn1.Name = "gridColumn1";
            gridColumn1.Visible = true;
            gridColumn1.VisibleIndex = 2;
            // 
            // gridColumn2
            // 
            gridColumn2.Caption = "Ngày tăng ca";
            gridColumn2.ColumnEdit = repositoryItemDateTimeOffsetEdit1;
            gridColumn2.DisplayFormat.FormatString = "yyyy-MM-dd";
            gridColumn2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gridColumn2.FieldName = "WorkDateRoot";
            gridColumn2.Name = "gridColumn2";
            gridColumn2.Visible = true;
            gridColumn2.VisibleIndex = 3;
            // 
            // gridColumn3
            // 
            gridColumn3.Caption = "Thời gian vào";
            gridColumn3.FieldName = "InTime";
            gridColumn3.Name = "gridColumn3";
            gridColumn3.Visible = true;
            gridColumn3.VisibleIndex = 4;
            // 
            // gridColumn4
            // 
            gridColumn4.Caption = "Thời gian ra";
            gridColumn4.FieldName = "OutTime";
            gridColumn4.Name = "gridColumn4";
            gridColumn4.Visible = true;
            gridColumn4.VisibleIndex = 5;
            // 
            // gridColumn5
            // 
            gridColumn5.Caption = "Số giờ OT";
            gridColumn5.FieldName = "RegisterHours";
            gridColumn5.Name = "gridColumn5";
            gridColumn5.Visible = true;
            gridColumn5.VisibleIndex = 6;
            // 
            // gridColumn6
            // 
            gridColumn6.Caption = "Loại OT";
            gridColumn6.FieldName = "OvertimeTypeName";
            gridColumn6.Name = "gridColumn6";
            gridColumn6.Visible = true;
            gridColumn6.VisibleIndex = 7;
            // 
            // gridColumn7
            // 
            gridColumn7.Caption = "Trạng thái";
            gridColumn7.FieldName = "Status";
            gridColumn7.Name = "gridColumn7";
            gridColumn7.Visible = true;
            gridColumn7.VisibleIndex = 8;
            // 
            // bandedGridColumn3
            // 
            bandedGridColumn3.Caption = "bandedGridColumn3";
            bandedGridColumn3.Name = "bandedGridColumn3";
            bandedGridColumn3.Visible = true;
            bandedGridColumn3.Width = 221;
            // 
            // bandedGridColumn4
            // 
            bandedGridColumn4.Caption = "bandedGridColumn4";
            bandedGridColumn4.Name = "bandedGridColumn4";
            bandedGridColumn4.Visible = true;
            // 
            // bandedGridColumn5
            // 
            bandedGridColumn5.Caption = "bandedGridColumn5";
            bandedGridColumn5.Name = "bandedGridColumn5";
            bandedGridColumn5.Visible = true;
            // 
            // bandedGridColumn6
            // 
            bandedGridColumn6.Caption = "bandedGridColumn6";
            bandedGridColumn6.Name = "bandedGridColumn6";
            bandedGridColumn6.Visible = true;
            // 
            // bandedGridColumn7
            // 
            bandedGridColumn7.Caption = "bandedGridColumn7";
            bandedGridColumn7.Name = "bandedGridColumn7";
            bandedGridColumn7.Visible = true;
            // 
            // bandedGridColumn8
            // 
            bandedGridColumn8.Caption = "bandedGridColumn8";
            bandedGridColumn8.Name = "bandedGridColumn8";
            bandedGridColumn8.Visible = true;
            // 
            // bandedGridColumn9
            // 
            bandedGridColumn9.Caption = "bandedGridColumn9";
            bandedGridColumn9.Name = "bandedGridColumn9";
            bandedGridColumn9.Visible = true;
            // 
            // bandedGridColumn10
            // 
            bandedGridColumn10.Caption = "bandedGridColumn10";
            bandedGridColumn10.Name = "bandedGridColumn10";
            bandedGridColumn10.Visible = true;
            bandedGridColumn10.Width = 223;
            // 
            // bandedGridColumn11
            // 
            bandedGridColumn11.Caption = "bandedGridColumn11";
            bandedGridColumn11.Name = "bandedGridColumn11";
            bandedGridColumn11.Visible = true;
            bandedGridColumn11.Width = 236;
            // 
            // bandedGridColumn12
            // 
            bandedGridColumn12.Caption = "bandedGridColumn12";
            bandedGridColumn12.Name = "bandedGridColumn12";
            bandedGridColumn12.Visible = true;
            bandedGridColumn12.Width = 221;
            // 
            // bandedGridColumn13
            // 
            bandedGridColumn13.Caption = "bandedGridColumn13";
            bandedGridColumn13.Name = "bandedGridColumn13";
            bandedGridColumn13.Visible = true;
            bandedGridColumn13.Width = 229;
            // 
            // bandedGridColumn14
            // 
            bandedGridColumn14.Caption = "bandedGridColumn14";
            bandedGridColumn14.Name = "bandedGridColumn14";
            bandedGridColumn14.Visible = true;
            // 
            // bandedGridColumn15
            // 
            bandedGridColumn15.Caption = "bandedGridColumn15";
            bandedGridColumn15.Name = "bandedGridColumn15";
            bandedGridColumn15.Visible = true;
            // 
            // gridBand10
            // 
            gridBand10.Caption = "gridBand10";
            gridBand10.Columns.Add(bandedGridColumn1);
            gridBand10.Name = "gridBand10";
            gridBand10.VisibleIndex = 0;
            gridBand10.Width = 134;
            // 
            // gridBand11
            // 
            gridBand11.Caption = "gridBand11";
            gridBand11.Columns.Add(bandedGridColumn2);
            gridBand11.Name = "gridBand11";
            gridBand11.VisibleIndex = 1;
            gridBand11.Width = 134;
            // 
            // gridBand3
            // 
            gridBand3.Caption = "gridBand3";
            gridBand3.Children.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] { gridBand4, gridBand5, gridBand6 });
            gridBand3.Name = "gridBand3";
            gridBand3.VisibleIndex = 2;
            gridBand3.Width = 671;
            // 
            // gridBand4
            // 
            gridBand4.Caption = "gridBand4";
            gridBand4.Columns.Add(bandedGridColumn3);
            gridBand4.Name = "gridBand4";
            gridBand4.VisibleIndex = 0;
            gridBand4.Width = 221;
            // 
            // gridBand5
            // 
            gridBand5.Caption = "gridBand5";
            gridBand5.Columns.Add(bandedGridColumn12);
            gridBand5.Name = "gridBand5";
            gridBand5.VisibleIndex = 1;
            gridBand5.Width = 221;
            // 
            // gridBand6
            // 
            gridBand6.Caption = "gridBand6";
            gridBand6.Columns.Add(bandedGridColumn13);
            gridBand6.Name = "gridBand6";
            gridBand6.VisibleIndex = 2;
            gridBand6.Width = 229;
            // 
            // gridBand7
            // 
            gridBand7.Caption = "gridBand7";
            gridBand7.Children.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] { gridBand12, gridBand8, gridBand9 });
            gridBand7.Name = "gridBand7";
            gridBand7.VisibleIndex = 3;
            gridBand7.Width = 534;
            // 
            // gridBand12
            // 
            gridBand12.Caption = "gridBand12";
            gridBand12.Columns.Add(bandedGridColumn4);
            gridBand12.Name = "gridBand12";
            gridBand12.VisibleIndex = 0;
            gridBand12.Width = 75;
            // 
            // gridBand8
            // 
            gridBand8.Caption = "gridBand8";
            gridBand8.Columns.Add(bandedGridColumn10);
            gridBand8.Name = "gridBand8";
            gridBand8.VisibleIndex = 1;
            gridBand8.Width = 223;
            // 
            // gridBand9
            // 
            gridBand9.Caption = "gridBand9";
            gridBand9.Columns.Add(bandedGridColumn11);
            gridBand9.Name = "gridBand9";
            gridBand9.VisibleIndex = 2;
            gridBand9.Width = 236;
            // 
            // DevExpress_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 577);
            Controls.Add(gridControl1);
            Name = "DevExpress_Form";
            Text = "DevExpress_Form";
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemDateTimeOffsetEdit1).EndInit();
            ((System.ComponentModel.ISupportInitialize)duLieuOtTableBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)bandedGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private BindingSource duLieuOtTableBindingSource;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateTimeOffsetEdit repositoryItemDateTimeOffsetEdit1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn MaNV;
        private DevExpress.XtraGrid.Columns.GridColumn Hoten;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn7;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridView bandedGridView1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn2;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn4;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn3;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn12;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn13;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn10;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn11;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn5;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn6;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn7;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn8;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn9;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn14;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn bandedGridColumn15;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand10;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand11;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand3;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand4;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand5;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand6;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand7;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand12;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand8;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand9;
    }
}