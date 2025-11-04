// ThreadedLoadingHost.cs
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Winforms_App_Template.Loading
{
    public sealed class ThreadedLoadingHost : IDisposable
    {
        private readonly Form _owner;
        private readonly CancellationTokenSource _cts;
        private readonly string _caption;
        private readonly Image? _gif;
        private readonly bool _disableOwner;

        private Thread? _uiThread;
        private ApplicationContext? _context;
        private LoadingDialog? _dlg;
        private readonly ManualResetEventSlim _ready = new(false);
        private bool _disposed;

        private ThreadedLoadingHost(Form owner, CancellationTokenSource cts, string caption, Image? gif, bool disableOwner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _cts = cts ?? throw new ArgumentNullException(nameof(cts));
            _caption = caption ?? "Đang xử lý...";
            _gif = gif;
            _disableOwner = disableOwner;
        }

        public static ThreadedLoadingHost Start(Form owner, CancellationTokenSource cts, string caption, Image? gif, bool disableOwner)
        {
            var host = new ThreadedLoadingHost(owner, cts, caption, gif, disableOwner);
            host.StartInternal();
            return host;
        }

        private void StartInternal()
        {
            // 🔒 Khoá owner PHẢI thực hiện trên chính UI thread của owner
            if (_disableOwner)
            {
                if (_owner.IsHandleCreated && _owner.InvokeRequired)
                    _owner.BeginInvoke(new Action(() => _owner.Enabled = false));
                else
                    _owner.Enabled = false;
            }

            _uiThread = new Thread(() =>
            {
                Application.SetCompatibleTextRenderingDefault(false);
                _dlg = new LoadingDialog(_cts, _caption, _gif);
                _context = new ApplicationContext(_dlg);
                _ready.Set();              // báo đã sẵn sàng
                Application.Run(_context); // message loop riêng cho popup
            })
            { IsBackground = true, Name = "LoadingDialogThread" };
            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Start();

            _ready.Wait();                 // đợi dialog tạo xong
            WireOwnerCentering();
            CenterToOwnerClient();         // căn giữa lần đầu
        }

        public void SetCaption(string text)
        {
            if (_dlg == null || _dlg.IsDisposed) return;
            try { _dlg.BeginInvoke(new Action(() => _dlg.SetCaption(text))); } catch { }
        }

        public void CenterToOwnerClient()
        {
            if (_dlg == null || _dlg.IsDisposed) return;
            if (_owner.IsDisposed) return;
            // Lấy client-rect của owner trên UI thread của owner
            Rectangle clientScreen;
            if (_owner.IsHandleCreated && _owner.InvokeRequired)
            {
                clientScreen = (Rectangle)_owner.Invoke(new Func<Rectangle>(() => _owner.RectangleToScreen(_owner.ClientRectangle)));
            }
            else
            {
                clientScreen = _owner.RectangleToScreen(_owner.ClientRectangle);
            }

            try
            {
                _dlg.BeginInvoke(new Action(() =>
                {
                    if (_dlg.IsDisposed) return;
                    int x = clientScreen.Left + (clientScreen.Width - _dlg.Width) / 2;
                    int y = clientScreen.Top + (clientScreen.Height - _dlg.Height) / 2;
                    _dlg.Location = new Point(x, y);
                }));
            }
            catch { }
        }

        public void Close()
        {
            if (_dlg == null || _dlg.IsDisposed) return;
            try
            {
                _dlg.BeginInvoke(new Action(() =>
                {
                    try { _dlg.SafeClose(); } catch { }
                    try { _context?.ExitThread(); } catch { }
                }));
            }
            catch { }
        }

        private void WireOwnerCentering()
        {
            _owner.Move += OwnerChanged_Recenter;
            _owner.Resize += OwnerChanged_Recenter;
            _owner.LocationChanged += OwnerChanged_Recenter;
            _owner.Layout += OwnerChanged_Recenter;
            _owner.FormClosed += (_, __) => Close();
        }
        private void OwnerChanged_Recenter(object? s, EventArgs e) => CenterToOwnerClient();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { Close(); } catch { }

            // 🔓 Mở khoá owner CŨNG phải marshal về UI thread của owner
            if (_disableOwner && !_owner.IsDisposed)
            {
                if (_owner.IsHandleCreated && _owner.InvokeRequired)
                    _owner.BeginInvoke(new Action(() => _owner.Enabled = true));
                else
                    _owner.Enabled = true;
            }
        }
    }
}
