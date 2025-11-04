namespace Winforms_App_Template.Loading
{
    partial class LoadingDialog
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
            tablePanel1 = new DevExpress.Utils.Layout.TablePanel();
            _btnCancel = new Button();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)tablePanel1).BeginInit();
            tablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // tablePanel1
            // 
            tablePanel1.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] { new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 51.29F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 48.41F), new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 55.3F) });
            tablePanel1.Controls.Add(_btnCancel);
            tablePanel1.Controls.Add(pictureBox1);
            tablePanel1.Dock = DockStyle.Fill;
            tablePanel1.Location = new Point(0, 0);
            tablePanel1.Margin = new Padding(0);
            tablePanel1.Name = "tablePanel1";
            tablePanel1.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] { new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 264F), new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F) });
            tablePanel1.Size = new Size(294, 305);
            tablePanel1.TabIndex = 0;
            // 
            // _btnCancel
            // 
            tablePanel1.SetColumn(_btnCancel, 1);
            _btnCancel.Dock = DockStyle.Fill;
            _btnCancel.Location = new Point(100, 267);
            _btnCancel.Name = "_btnCancel";
            tablePanel1.SetRow(_btnCancel, 1);
            _btnCancel.Size = new Size(86, 35);
            _btnCancel.TabIndex = 1;
            _btnCancel.Text = "Hủy";
            _btnCancel.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            tablePanel1.SetColumn(pictureBox1, 0);
            tablePanel1.SetColumnSpan(pictureBox1, 3);
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = Properties.Resources.loading_gif;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Margin = new Padding(0);
            pictureBox1.Name = "pictureBox1";
            tablePanel1.SetRow(pictureBox1, 0);
            pictureBox1.Size = new Size(294, 264);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // LoadingDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(294, 305);
            ControlBox = false;
            Controls.Add(tablePanel1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoadingDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            TopMost = true;
            ((System.ComponentModel.ISupportInitialize)tablePanel1).EndInit();
            tablePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.Utils.Layout.TablePanel tablePanel1;
        private Button _btnCancel;
        private PictureBox pictureBox1;
    }
}