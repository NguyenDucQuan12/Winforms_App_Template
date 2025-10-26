namespace Winforms_App_Template.Forms
{
    partial class Catongtho
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
            ID_Cong_Doan_Text = new DevExpress.XtraEditors.TextEdit();
            behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager(components);
            tablePanel1 = new DevExpress.Utils.Layout.TablePanel();
            So_Me_Text = new DevExpress.XtraEditors.TextEdit();
            label6 = new Label();
            Lot_No_Text = new DevExpress.XtraEditors.TextEdit();
            Item_Number_Text = new DevExpress.XtraEditors.TextEdit();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            Export_Document_Button = new Button();
            tablePanel2 = new DevExpress.Utils.Layout.TablePanel();
            ((System.ComponentModel.ISupportInitialize)ID_Cong_Doan_Text.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)behaviorManager1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tablePanel1).BeginInit();
            tablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)So_Me_Text.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Lot_No_Text.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Item_Number_Text.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tablePanel2).BeginInit();
            tablePanel2.SuspendLayout();
            SuspendLayout();
            // 
            // ID_Cong_Doan_Text
            // 
            tablePanel1.SetColumn(ID_Cong_Doan_Text, 2);
            ID_Cong_Doan_Text.Dock = DockStyle.Fill;
            ID_Cong_Doan_Text.EditValue = "68";
            ID_Cong_Doan_Text.Location = new Point(224, 20);
            ID_Cong_Doan_Text.Name = "ID_Cong_Doan_Text";
            tablePanel1.SetRow(ID_Cong_Doan_Text, 0);
            ID_Cong_Doan_Text.Size = new Size(165, 38);
            ID_Cong_Doan_Text.TabIndex = 0;
            ID_Cong_Doan_Text.ToolTip = "ID công đoạn cần xuất báo cáo";
            // 
            // tablePanel1
            // 
            tablePanel1.AutoScroll = true;
            tablePanel1.AutoSize = true;
            tablePanel1.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] { new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 0F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 58.75F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F) });
            tablePanel1.Controls.Add(So_Me_Text);
            tablePanel1.Controls.Add(label6);
            tablePanel1.Controls.Add(Lot_No_Text);
            tablePanel1.Controls.Add(Item_Number_Text);
            tablePanel1.Controls.Add(label5);
            tablePanel1.Controls.Add(label4);
            tablePanel1.Controls.Add(label3);
            tablePanel1.Controls.Add(label2);
            tablePanel1.Controls.Add(label1);
            tablePanel1.Controls.Add(ID_Cong_Doan_Text);
            tablePanel1.Dock = DockStyle.Top;
            tablePanel1.Location = new Point(0, 0);
            tablePanel1.Name = "tablePanel1";
            tablePanel1.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] { new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 45.4285355F), new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 42.5714569F), new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F), new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F) });
            tablePanel1.Size = new Size(1440, 176);
            tablePanel1.TabIndex = 1;
            tablePanel1.UseSkinIndents = true;
            // 
            // So_Me_Text
            // 
            tablePanel1.SetColumn(So_Me_Text, 8);
            So_Me_Text.Dock = DockStyle.Fill;
            So_Me_Text.EditValue = "1";
            So_Me_Text.Location = new Point(1253, 20);
            So_Me_Text.Name = "So_Me_Text";
            tablePanel1.SetRow(So_Me_Text, 0);
            So_Me_Text.Size = new Size(165, 38);
            So_Me_Text.TabIndex = 9;
            So_Me_Text.ToolTip = "Số mẻ";
            // 
            // label6
            // 
            label6.AutoSize = true;
            tablePanel1.SetColumn(label6, 7);
            label6.Dock = DockStyle.Fill;
            label6.Location = new Point(1081, 17);
            label6.Name = "label6";
            tablePanel1.SetRow(label6, 0);
            label6.Size = new Size(165, 45);
            label6.TabIndex = 8;
            label6.Text = "Số mẻ: ";
            label6.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Lot_No_Text
            // 
            tablePanel1.SetColumn(Lot_No_Text, 6);
            Lot_No_Text.Dock = DockStyle.Fill;
            Lot_No_Text.EditValue = "250505G01";
            Lot_No_Text.Location = new Point(910, 20);
            Lot_No_Text.Name = "Lot_No_Text";
            tablePanel1.SetRow(Lot_No_Text, 0);
            Lot_No_Text.Size = new Size(165, 38);
            Lot_No_Text.TabIndex = 7;
            Lot_No_Text.ToolTip = "Số lô ";
            // 
            // Item_Number_Text
            // 
            tablePanel1.SetColumn(Item_Number_Text, 4);
            Item_Number_Text.Dock = DockStyle.Fill;
            Item_Number_Text.EditValue = "CRS25C60N10W";
            Item_Number_Text.Location = new Point(567, 20);
            Item_Number_Text.Name = "Item_Number_Text";
            tablePanel1.SetRow(Item_Number_Text, 0);
            Item_Number_Text.Size = new Size(165, 38);
            Item_Number_Text.TabIndex = 6;
            Item_Number_Text.ToolTip = "Item Number";
            // 
            // label5
            // 
            label5.AutoSize = true;
            tablePanel1.SetColumn(label5, 5);
            label5.Dock = DockStyle.Fill;
            label5.Location = new Point(738, 17);
            label5.Name = "label5";
            tablePanel1.SetRow(label5, 0);
            label5.Size = new Size(165, 45);
            label5.TabIndex = 5;
            label5.Text = "Lot No: ";
            label5.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            label4.AutoSize = true;
            tablePanel1.SetColumn(label4, 3);
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(395, 17);
            label4.Name = "label4";
            tablePanel1.SetRow(label4, 0);
            label4.Size = new Size(165, 45);
            label4.TabIndex = 4;
            label4.Text = "Item Number: ";
            label4.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            label3.AutoSize = true;
            tablePanel1.SetColumn(label3, 1);
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(22, 17);
            label3.Name = "label3";
            tablePanel1.SetRow(label3, 0);
            label3.Size = new Size(196, 45);
            label3.TabIndex = 3;
            label3.Text = "ID Công Đoạn: ";
            label3.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            tablePanel1.SetColumn(label2, 1);
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(22, 17);
            label2.Name = "label2";
            tablePanel1.SetRow(label2, 0);
            label2.Size = new Size(196, 45);
            label2.TabIndex = 2;
            label2.Text = "ID Công Đoạn: ";
            label2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            label1.AutoSize = true;
            tablePanel1.SetColumn(label1, 1);
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(22, 17);
            label1.Name = "label1";
            tablePanel1.SetRow(label1, 0);
            label1.Size = new Size(196, 45);
            label1.TabIndex = 1;
            label1.Text = "ID Công Đoạn: ";
            label1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Export_Document_Button
            // 
            tablePanel2.SetColumn(Export_Document_Button, 4);
            Export_Document_Button.Location = new Point(634, 92);
            Export_Document_Button.Name = "Export_Document_Button";
            tablePanel2.SetRow(Export_Document_Button, 2);
            Export_Document_Button.Size = new Size(191, 85);
            Export_Document_Button.TabIndex = 2;
            Export_Document_Button.Text = "Xuất báo cáo";
            Export_Document_Button.UseVisualStyleBackColor = true;
            Export_Document_Button.Click += Export_Document_Button_Click;
            // 
            // tablePanel2
            // 
            tablePanel2.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] { new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 0F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 55F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 50F) });
            tablePanel2.Controls.Add(Export_Document_Button);
            tablePanel2.Dock = DockStyle.Fill;
            tablePanel2.Location = new Point(0, 176);
            tablePanel2.Name = "tablePanel2";
            tablePanel2.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] { new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 0F), new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F), new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 184.285736F), new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F) });
            tablePanel2.Size = new Size(1440, 796);
            tablePanel2.TabIndex = 3;
            tablePanel2.UseSkinIndents = true;
            // 
            // Catongtho
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1440, 972);
            Controls.Add(tablePanel2);
            Controls.Add(tablePanel1);
            Margin = new Padding(5, 6, 5, 6);
            Name = "Catongtho";
            Text = "app";
            ((System.ComponentModel.ISupportInitialize)ID_Cong_Doan_Text.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)behaviorManager1).EndInit();
            ((System.ComponentModel.ISupportInitialize)tablePanel1).EndInit();
            tablePanel1.ResumeLayout(false);
            tablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)So_Me_Text.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)Lot_No_Text.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)Item_Number_Text.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)tablePanel2).EndInit();
            tablePanel2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraEditors.TextEdit ID_Cong_Doan_Text;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.Utils.Layout.TablePanel tablePanel1;
        private Label label1;
        private Label label3;
        private Label label2;
        private DevExpress.XtraEditors.TextEdit Lot_No_Text;
        private DevExpress.XtraEditors.TextEdit Item_Number_Text;
        private Label label5;
        private Label label4;
        private Button Export_Document_Button;
        private DevExpress.XtraEditors.TextEdit So_Me_Text;
        private Label label6;
        private DevExpress.Utils.Layout.TablePanel tablePanel2;
    }
}