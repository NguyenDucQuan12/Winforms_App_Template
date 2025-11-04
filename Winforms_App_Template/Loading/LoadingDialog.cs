using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms_App_Template.Loading
{
    public partial class LoadingDialog : Form
    {
        // Giữ tham chiếu tới CTS do caller truyền vào, để nút Hủy có thể Cancel thật sự.
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// Khởi tạo popup.
        /// </summary>
        /// <param name="cts">CTS do luồng công việc bên ngoài cung cấp</param>
        /// <param name="caption">Tiêu đề form (ví dụ "Đang xử lý...")</param>
        /// <param name="gif">Tuỳ chọn: ảnh GIF thay thế, nếu null dùng ảnh trong Designer</param>
        public LoadingDialog(CancellationTokenSource cts, string? caption = "Đang xử lý...", Image? gif = null)
        {
            // Lưu CTS hoặc báo lỗi nếu null
            _cts = cts ?? throw new ArgumentNullException(nameof(cts));

            // Khởi tạo UI theo layout (tablePanel1/pictureBox1/_btnCancel)
            InitializeComponent();

            // ===== Cấu hình chung của form =====

            // Ẩn toàn bộ nút hệ thống (Close/Min/Max) ở caption bar
            ControlBox = false;     // ẩn nút X + menu hệ thống
            MinimizeBox = false;     // ẩn nút thu nhỏ
            MaximizeBox = false;     // ẩn nút phóng to
            ShowIcon = false;     // không hiển thị icon nhỏ
            ShowInTaskbar = false;     // không hiện taskbar
            TopMost = true;      // nằm trên form cha (tránh bị che)

            // ===== Loại bỏ mọi khoảng trống quanh TablePanel =====
            // DevExpress TablePanel mặc định có "skin indents" → tạo padding 12px như bạn thấy (Location (13,12)).
            // Tắt nó đi + đưa mọi padding/margin về 0.
            tablePanel1.UseSkinIndents = false;             // QUAN TRỌNG: bỏ padding nội bộ theo skin
            tablePanel1.Padding = new Padding(0);           // Không đệm viền trong
            tablePanel1.Margin = new Padding(0);           // Không đệm viền ngoài
            this.Padding = new Padding(0);           // Form cũng không có padding
            this.AutoSize = false;                    // Tránh autosize gây nảy layout

            StartPosition = FormStartPosition.CenterParent; // bật giữa form gọi

            // Đặt tiêu đề nếu có truyền vào
            Text = string.IsNullOrWhiteSpace(caption) ? "Đang xử lý..." : caption;

            // ===== Cấu hình GIF =====

            // Bảo đảm PictureBox luôn co giãn phủ khung, giữ tỉ lệ (tránh méo ảnh)
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // Nếu caller truyền GIF riêng -> dùng GIF đó; nếu không, giữ ảnh từ Designer (Properties.Resources.loading_gif)
            if (gif != null)
                pictureBox1.Image = gif;

            // ===== Cấu hình nút Hủy =====
            // Gắn handler: khi bấm Hủy -> vô hiệu nút (tránh double click), gửi tín hiệu Cancel
            _btnCancel.Click += (_, __) =>
            {
                _btnCancel.Enabled = false;    // khoá tránh bấm thêm lần nữa
                try { _cts.Cancel(); } catch { /* an toàn: CTS có thể đã bị Dispose */ }
            };

            // ===== Bắt phím ESC = Hủy =====
            KeyPreview = true;                 // form bắt phím trước khi control con nhận
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape && _btnCancel.Enabled)
                {
                    _btnCancel.PerformClick(); // mô phỏng click nút
                    e.Handled = true;          // đã xử lý ESC
                }
            };
        }

        /// <summary>
        /// Khi form được hiển thị lần đầu → gắn theo dõi owner và căn giữa ngay.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            AttachOwnerCentering();                       // gắn các event của owner để luôn bám giữa
            CenterToOwnerClient();                        // căn giữa lần đầu
        }

        /// <summary>
        /// ĐẶt popup luôn nằm giữa vùng client của owner.
        /// </summary>
        private void AttachOwnerCentering()
        {
            if (Owner == null) return;

            // Khi owner di chuyển/đổi size/layout → recenter popup
            Owner.LocationChanged += OwnerChanged_Recenter;
            Owner.SizeChanged += OwnerChanged_Recenter;
            Owner.Resize += OwnerChanged_Recenter;
            Owner.Layout += OwnerChanged_Recenter;

            // Nếu owner đóng thì popup cũng tự đóng theo
            Owner.FormClosed += (_, __) => { if (!IsDisposed) SafeClose(); };
        }

        private void OwnerChanged_Recenter(object? sender, EventArgs e) => CenterToOwnerClient();

        private void CenterToOwnerClient()
        {
            if (Owner == null || Owner.IsDisposed) return;

            // Lấy hình chữ nhật "vùng client" của owner, convert sang toạ độ màn hình
            Rectangle clientScreen = Owner.RectangleToScreen(Owner.ClientRectangle);

            // Tính toạ độ để popup nằm chính giữa vùng client đó
            int x = clientScreen.Left + (clientScreen.Width - Width) / 2;
            int y = clientScreen.Top + (clientScreen.Height - Height) / 2;

            // Đặt vị trí mới
            Location = new Point(x, y);
        }

        /// <summary>
        /// Chặn người dùng tự đóng (Alt+F4) để bắt buộc họ dùng nút Hủy.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Nếu người dùng cố đóng bằng hành động "UserClosing" (Alt+F4, menu), ta từ chối
            if (e.CloseReason == CloseReason.UserClosing && Visible)
            {
                // Ta KHÔNG tự đóng tại đây, chỉ cho phép Hủy (Cancel) hoặc caller Close() chủ động
                e.Cancel = true;
                // Có thể hiển thị gợi ý:
                // MessageBox.Show("Vui lòng bấm nút Hủy để dừng tiến trình.");
            }
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Cập nhật caption trong lúc chạy (tuỳ chọn).
        /// Gọi an toàn từ mọi thread.
        /// </summary>
        public void SetCaption(string text)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action<string>(SetCaption), text); return; }
            Text = text;
        }

        /// <summary>
        /// Đóng form an toàn từ mọi thread.
        /// </summary>
        public void SafeClose()
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action(SafeClose)); return; }
            Close();
        }
    }
}
