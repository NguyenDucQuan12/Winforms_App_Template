using System;
using System.Windows.Forms;

namespace Winforms_App_Template.Utils
{
    /// <summary>
    /// Dùng bằng cú pháp "using (new UiBlocker(this)) { ... }"
    /// Khi khởi tạo sẽ: form.Enabled = false → khoá toàn bộ control của form.
    /// Khi Dispose sẽ: bật lại Enabled = true.
    /// </summary>
    public readonly struct UiBlocker : IDisposable
    {
        private readonly Form _owner;

        public UiBlocker(Form owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            // Vô hiệu toàn bộ control của form chính (người dùng không bấm được gì)
            _owner.Enabled = false;
        }

        public void Dispose()
        {
            try
            {
                if (_owner != null && !_owner.IsDisposed)
                    _owner.Enabled = true; // bật lại tương tác
            }
            catch { /* an toàn */ }
        }
    }
}
