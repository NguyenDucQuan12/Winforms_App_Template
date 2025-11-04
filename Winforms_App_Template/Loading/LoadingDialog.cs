using DevExpress.XtraRichEdit.Model;
using System;
using System.Drawing;               // Image, Icon, Size, Point, ContentAlignment
using System.Windows.Forms;         // Form, PictureBox, Button, Label
using System.IO;

namespace Winforms_App_Template.Loading
{
    /// <summary>
    /// Popup loading cực gọn:
    /// - Hiển thị ảnh GIF động (đặt trong Resources hoặc load từ file).
    /// - Chỉ có 1 nút Cancel để người dùng huỷ.
    /// - Không có nút đóng/phóng to/thu nhỏ → buộc người dùng chỉ có thể bấm Cancel.
    /// - Không tự đóng khi bấm Cancel; chỉ phát tín hiệu Cancel (CTS) và thay đổi text → 
    ///   popup sẽ tự đóng khi công việc kết thúc (ta Close() trong finally).
    /// </summary>
    public sealed class LoadingDialog : Form
    {
        private readonly PictureBox _pic;
        private readonly Button _btnCancel;
        private readonly CancellationTokenSource _cts;

        public LoadingDialog(CancellationTokenSource cts, string? message = null)
        {
            _cts = cts ?? throw new ArgumentNullException(nameof(cts));

            // ===== Form =====
            FormBorderStyle = FormBorderStyle.FixedDialog; // có viền nhẹ
            StartPosition = FormStartPosition.CenterParent;
            ControlBox = false;                       // ẩn X / maximize / minimize
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            TopMost = true;
            Text = message ?? "Đang xử lý...";
            ClientSize = new Size(420, 300);
            DoubleBuffered = true;

            // ===== Layout: 2 hàng (GIF, nút) =====
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // GIF chiếm hết
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f)); // Hàng của nút
            Controls.Add(layout);

            // ===== GIF =====
            _pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom // co giãn vừa khung, giữ tỉ lệ
            };
            // Ưu tiên resource:
            try { _pic.Image = Properties.Resources.loading_gif; }
            catch
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "loading.gif");
                if (File.Exists(path)) _pic.Image = Image.FromFile(path);
            }
            layout.Controls.Add(_pic, 0, 0);

            // ===== Khu nút Cancel (canh giữa) =====
            var panelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            panelButtons.Controls.Add((_btnCancel = new Button
            {
                Text = "Cancel",
                AutoSize = true,
                Anchor = AnchorStyles.None,
                Margin = new Padding(0)
            }));
            panelButtons.Layout += (_, __) =>
            {
                // canh giữa nút theo chiều ngang và dọc
                panelButtons.Padding = new Padding(
                    Math.Max(0, (panelButtons.ClientSize.Width - _btnCancel.Width) / 2),
                    Math.Max(0, (panelButtons.ClientSize.Height - _btnCancel.Height) / 2),
                    0, 0);
            };
            layout.Controls.Add(panelButtons, 0, 1);

            // ===== Cancel =====
            _btnCancel.Click += (_, __) =>
            {
                _btnCancel.Enabled = false;
                try { _cts.Cancel(); } catch { }
            };

            // Cho phép nhấn ESC = Cancel
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape && _btnCancel.Enabled)
                {
                    _btnCancel.PerformClick();
                    e.Handled = true;
                }
            };
        }

        public void SafeClose()
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action(SafeClose)); return; }
            Close();
        }
    }
}
