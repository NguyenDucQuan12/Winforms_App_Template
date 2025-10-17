namespace Winforms_App_Template.Forms
{
    partial class test
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
            pivotGridControl1 = new DevExpress.XtraPivotGrid.PivotGridControl();
            pivotGridField1 = new DevExpress.XtraPivotGrid.PivotGridField();
            pivotGridField2 = new DevExpress.XtraPivotGrid.PivotGridField();
            pivotGridField3 = new DevExpress.XtraPivotGrid.PivotGridField();
            pivotGridField4 = new DevExpress.XtraPivotGrid.PivotGridField();
            ((System.ComponentModel.ISupportInitialize)pivotGridControl1).BeginInit();
            SuspendLayout();
            // 
            // pivotGridControl1
            // 
            pivotGridControl1.Fields.AddRange(new DevExpress.XtraPivotGrid.PivotGridField[] { pivotGridField1, pivotGridField2, pivotGridField3, pivotGridField4 });
            pivotGridControl1.Location = new Point(243, 65);
            pivotGridControl1.Name = "pivotGridControl1";
            pivotGridControl1.OptionsData.DataProcessingEngine = DevExpress.XtraPivotGrid.PivotDataProcessingEngine.Optimized;
            pivotGridControl1.Size = new Size(400, 200);
            pivotGridControl1.TabIndex = 0;
            // 
            // pivotGridField1
            // 
            pivotGridField1.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            pivotGridField1.AreaIndex = 2;
            pivotGridField1.Name = "pivotGridField1";
            pivotGridField1.SortOrder = DevExpress.XtraPivotGrid.PivotSortOrder.Descending;
            // 
            // pivotGridField2
            // 
            pivotGridField2.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            pivotGridField2.AreaIndex = 3;
            pivotGridField2.Name = "pivotGridField2";
            pivotGridField2.SortOrder = DevExpress.XtraPivotGrid.PivotSortOrder.Descending;
            // 
            // pivotGridField3
            // 
            pivotGridField3.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            pivotGridField3.AreaIndex = 1;
            pivotGridField3.Name = "pivotGridField3";
            pivotGridField3.Width = 129;
            // 
            // pivotGridField4
            // 
            pivotGridField4.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            pivotGridField4.AreaIndex = 0;
            pivotGridField4.Name = "pivotGridField4";
            // 
            // test
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pivotGridControl1);
            Name = "test";
            Text = "test";
            ((System.ComponentModel.ISupportInitialize)pivotGridControl1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraPivotGrid.PivotGridControl pivotGridControl1;
        private DevExpress.XtraPivotGrid.PivotGridField pivotGridField1;
        private DevExpress.XtraPivotGrid.PivotGridField pivotGridField2;
        private DevExpress.XtraPivotGrid.PivotGridField pivotGridField3;
        private DevExpress.XtraPivotGrid.PivotGridField pivotGridField4;
    }
}