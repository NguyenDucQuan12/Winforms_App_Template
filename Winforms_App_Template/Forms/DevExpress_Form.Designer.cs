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
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            repositoryItemDateTimeOffsetEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemDateTimeOffsetEdit();
            duLieuOtTableBindingSource = new BindingSource(components);
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
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemDateTimeOffsetEdit1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)duLieuOtTableBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            SuspendLayout();
            // 
            // gridControl1
            // 
            gridControl1.Dock = DockStyle.Fill;
            gridControl1.Location = new Point(0, 0);
            gridControl1.MainView = gridView1;
            gridControl1.Name = "gridControl1";
            gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { repositoryItemDateTimeOffsetEdit1 });
            gridControl1.Size = new Size(800, 450);
            gridControl1.TabIndex = 0;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
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
            // DevExpress_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(gridControl1);
            Name = "DevExpress_Form";
            Text = "DevExpress_Form";
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemDateTimeOffsetEdit1).EndInit();
            ((System.ComponentModel.ISupportInitialize)duLieuOtTableBindingSource).EndInit();
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
    }
}