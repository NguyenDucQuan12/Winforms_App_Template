using System;
using System.Collections.Generic;
using System.ComponentModel;               // BindingList<T>
using System.Drawing;                      // Padding, FontStyle (nút đang chọn)
using System.Linq;                         // ToList()
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Database.Table;

namespace Winforms_App_Template.Forms
{
    /// <summary>
    /// Form hiển thị OT phân trang:
    /// - Filter: CodeEmp, From/To, PageSize
    /// - Nút: First, Prev, dải số trang (1 2 3 … k … N), Next, Last
    /// - Nhấn nút → gọi Repo QueryPageAsync → bind lưới
    /// </summary>
    public partial class Phan_Trang : Form
    {
        // == UI: bộ filter ==
        private readonly TextBox _txtCodeEmp = new() { Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right };
        private readonly DateTimePicker _dtFrom = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd" };
        private readonly DateTimePicker _dtTo = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd" };
        private readonly NumericUpDown _nudSize = new() { Minimum = 1, Maximum = 500, Value = 100 };

        private readonly Button _btnLoad = new() { Text = "Tải dữ liệu (F5)" };
        private readonly Button _btnCancel = new() { Text = "Hủy" };

        // == Lưới dữ liệu ==
        private readonly DataGridView _grid = new()
        {
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            Dock = DockStyle.Fill
        };

        // == Thanh phân trang (Dock ở đáy Form) ==
        // Panel chứa toàn bộ pager (bottom)
        private readonly FlowLayoutPanel _pagerPanel = new()
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            Padding = new Padding(8),
            WrapContents = false
        };

        // Cụm nút điều hướng trái
        private readonly Button _btnFirst = new() { Text = "⏮ First" };
        private readonly Button _btnPrev = new() { Text = "◀ Prev" };

        // Host chứa các nút số trang
        private readonly FlowLayoutPanel _btnNumbersHost = new()
        {
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(12, 0, 12, 0)
        };

        // Cụm nút điều hướng phải
        private readonly Button _btnNext = new() { Text = "Next ▶" };
        private readonly Button _btnLast = new() { Text = "Last ⏭" };

        // Nhãn thông tin trang/tổng
        private readonly Label _lblPageInfo = new() { AutoSize = true, Margin = new Padding(12, 8, 0, 0) };

        // Nhãn trạng thái tải/lỗi
        private readonly Label _lblStatus = new() { AutoSize = true };

        // == Hạ tầng ==
        private readonly DuLieuOt_Table _repo;        // Repository nghiệp vụ
        private CancellationTokenSource? _cts;         // Hủy tải

        // == Trạng thái phân trang hiện tại ==
        private int _currentPage = 1;                  // Trang đang xem
        private int PageSize => (int)_nudSize.Value;   // Page size lấy trực tiếp từ UI
        private long _totalItems = 0;                  // Tổng số dòng (để tính total pages)
        private int _totalPages = 0;                   // Tổng số trang (ceil(total/pageSize))

        public Phan_Trang(DbExecutor? db = null)
        {
            Text = "Danh sách OT nhân viên";
            Width = 1100;
            Height = 680;
            StartPosition = FormStartPosition.CenterScreen;

            // Repo: dùng DbExecutor (nếu chưa DI) — connString lấy từ DbConfig
            var executor = db ?? new DbExecutor();
            _repo = new DuLieuOt_Table(executor);

            // ====== Panel filter (Top) ======
            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 10,
                RowCount = 2,
                AutoSize = true,
                Padding = new Padding(8)
            };

            // Hàng 1: label + inputs
            top.Controls.Add(new Label { Text = "Mã NV:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            top.Controls.Add(_txtCodeEmp, 1, 0);
            top.SetColumnSpan(_txtCodeEmp, 2);

            top.Controls.Add(new Label { Text = "Từ ngày:", AutoSize = true, Anchor = AnchorStyles.Left }, 3, 0);
            top.Controls.Add(_dtFrom, 4, 0);

            top.Controls.Add(new Label { Text = "Đến ngày:", AutoSize = true, Anchor = AnchorStyles.Left }, 5, 0);
            top.Controls.Add(_dtTo, 6, 0);

            top.Controls.Add(new Label { Text = "Size:", AutoSize = true, Anchor = AnchorStyles.Left }, 7, 0);
            top.Controls.Add(_nudSize, 8, 0);

            top.Controls.Add(_btnLoad, 9, 0);

            // Hàng 2: status + nút hủy
            top.Controls.Add(_lblStatus, 0, 1);
            top.SetColumnSpan(_lblStatus, 9);
            top.Controls.Add(_btnCancel, 9, 1);

            // ====== Grid: cột định nghĩa thủ công để format đẹp ======
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Mã NV",
                DataPropertyName = nameof(DuLieuOt_Model.CodeEmp),
                Width = 100
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Họ tên",
                DataPropertyName = nameof(DuLieuOt_Model.ProfileName),
                Width = 200
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Bộ phận",
                DataPropertyName = nameof(DuLieuOt_Model.BoPhan),
                Width = 150
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Ngày LV",
                DataPropertyName = nameof(DuLieuOt_Model.WorkDateRoot),
                DefaultCellStyle = { Format = "yyyy-MM-dd" },
                Width = 110
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vào",
                DataPropertyName = nameof(DuLieuOt_Model.InTime),
                DefaultCellStyle = { Format = @"hh\:mm" },   // TimeSpan? → HH:mm
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Ra",
                DataPropertyName = nameof(DuLieuOt_Model.OutTime),
                DefaultCellStyle = { Format = @"hh\:mm" },
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Giờ ĐK",
                DataPropertyName = nameof(DuLieuOt_Model.RegisterHours),
                DefaultCellStyle = { Format = "0.##" },
                Width = 70
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Loại OT",
                DataPropertyName = nameof(DuLieuOt_Model.OvertimeTypeName),
                Width = 120
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Trạng thái",
                DataPropertyName = nameof(DuLieuOt_Model.Status),
                Width = 120
            });

            // ====== Pager Panel (Bottom) — add theo thứ tự: First, Prev, btnNumbersHost, Next, Last, PageInfo ======
            _pagerPanel.Controls.Add(_btnFirst);
            _pagerPanel.Controls.Add(_btnPrev);
            _pagerPanel.Controls.Add(_btnNumbersHost);
            _pagerPanel.Controls.Add(_btnNext);
            _pagerPanel.Controls.Add(_btnLast);
            _pagerPanel.Controls.Add(_lblPageInfo);

            // ====== Add vào Form theo thứ tự Dock: Top -> Bottom -> Fill ======
            Controls.Add(_grid);         // Fill chiếm phần còn lại
            Controls.Add(_pagerPanel);   // Bottom nằm dưới Grid (z-order: add trước grid thì grid che; nên add grid trước rồi add bottom)
            Controls.Add(top);           // Top dính trên

            // ====== Sự kiện ======
            Load += (_, __) => InitDefaultDates();                     // mặc định chọn tháng hiện tại
            _btnLoad.Click += async (_, __) => { _currentPage = 1; await LoadDataAsync(); };
            _btnCancel.Click += (_, __) => _cts?.Cancel();

            _btnFirst.Click += async (_, __) => await GoToPageAsync(1);
            _btnPrev.Click += async (_, __) => await GoToPageAsync(Math.Max(1, _currentPage - 1));
            _btnNext.Click += async (_, __) => await GoToPageAsync(Math.Max(1, Math.Min(_totalPages, _currentPage + 1)));
            _btnLast.Click += async (_, __) => await GoToPageAsync(_totalPages <= 0 ? 1 : _totalPages);

            KeyPreview = true;
            KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.F5) { _currentPage = 1; await LoadDataAsync(); }
                if (e.KeyCode == Keys.Right && _currentPage < _totalPages) await GoToPageAsync(_currentPage + 1);
                if (e.KeyCode == Keys.Left && _currentPage > 1) await GoToPageAsync(_currentPage - 1);
            };
        }

        private void InitDefaultDates()
        {
            // Gợi ý: lọc theo tháng hiện tại để tránh đổ cả bảng
            var now = DateTime.Today;
            var first = new DateTime(now.Year, now.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            _dtFrom.Value = first;
            _dtTo.Value = last;
            _lblStatus.Text = "Nhấn 'Tải dữ liệu' để bắt đầu…";
            UpdatePagerUI(1, 1, 0); // Khởi tạo pager trống
        }

        /// <summary>
        /// Điều hướng tới trang bất kỳ (nút số, next/prev/first/last gọi vào đây).
        /// </summary>
        private async Task GoToPageAsync(int page)
        {
            // Chặn vượt biên
            if (page < 1) page = 1;
            if (_totalPages > 0 && page > _totalPages) page = _totalPages;

            _currentPage = page;
            await LoadDataAsync();
        }

        /// <summary>
        /// Tải dữ liệu theo _currentPage, PageSize, filter; bind lên lưới; update pager.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource();
                _cts = cts;
                ToggleUiLoading(true);
                _lblStatus.Text = "Đang tải dữ liệu…";

                // Lấy filter từ UI
                string? code = string.IsNullOrWhiteSpace(_txtCodeEmp.Text) ? null : _txtCodeEmp.Text.Trim();
                var from = _dtFrom.Value.Date;
                var to = _dtTo.Value.Date;
                int size = PageSize;

                // Gói tham số trang + filter
                var req = new PageRequest
                {
                    Page = _currentPage,     // 👈 dùng trang hiện tại
                    Size = size,
                    SortBy = "WorkDateRoot", // mặc định sắp xếp theo ngày (bạn có thể cho UI chọn)
                    Desc = true,
                    CodeEmp = code,
                    FromDate = from,
                    ToDate = to
                };

                // Gọi Repo → trả PageResult<DuLieuOt_Model>
                var page = await _repo.QueryPageAsync(req, cts.Token);

                // Cập nhật state tổng để render pager
                _currentPage = page.Page;                // Repo đã chuẩn hóa, lấy về cho chắc
                _totalItems = page.Total;
                _totalPages = page.Size > 0 ? (int)Math.Ceiling(page.Total / (double)page.Size) : 0;

                // Bind dữ liệu (Items là danh sách dòng)
                _grid.DataSource = new BindingList<DuLieuOt_Model>(page.Items.ToList());

                // Cập nhật pager UI
                UpdatePagerUI(_currentPage, _totalPages, _totalItems);

                _lblStatus.Text = $"Tải xong: {page.Items.Count} dòng (trang {page.Page}/{_totalPages}, size={page.Size}, tổng={page.Total}).";
            }
            catch (OperationCanceledException)
            {
                _lblStatus.Text = "Đã hủy tải.";
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi khi tải dữ liệu.";
                MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUiLoading(false);
                _cts?.Dispose();
                _cts = null;
            }
        }

        /// <summary>
        /// Bật/tắt UI khi đang tải → tránh bấm liên tục.
        /// </summary>
        private void ToggleUiLoading(bool loading)
        {
            _btnLoad.Enabled = !loading;
            _btnCancel.Enabled = loading;
            _txtCodeEmp.Enabled = !loading;
            _dtFrom.Enabled = !loading;
            _dtTo.Enabled = !loading;
            _nudSize.Enabled = !loading;
            _btnFirst.Enabled = !loading && _currentPage > 1;
            _btnPrev.Enabled = !loading && _currentPage > 1;
            _btnNext.Enabled = !loading && _currentPage < _totalPages;
            _btnLast.Enabled = !loading && _currentPage < _totalPages;
            UseWaitCursor = loading;
        }

        /// <summary>
        /// Vẽ lại thanh phân trang: nút First/Prev/Next/Last + dải số trang + nhãn "Page X/Y (Total N)".
        /// </summary>
        private void UpdatePagerUI(int currentPage, int totalPages, long totalItems)
        {
            // 1) Cập nhật enable/disable điều hướng lớn
            _btnFirst.Enabled = currentPage > 1 && totalPages > 0;
            _btnPrev.Enabled = currentPage > 1 && totalPages > 0;
            _btnNext.Enabled = currentPage < totalPages && totalPages > 0;
            _btnLast.Enabled = currentPage < totalPages && totalPages > 0;

            // 2) Cập nhật nhãn thông tin tổng quan
            if (totalPages <= 0)
                _lblPageInfo.Text = "Page 0/0 (Total 0)";
            else
                _lblPageInfo.Text = $"Page {currentPage}/{totalPages}  (Total {totalItems:N0})";

            // 3) Dải nút số trang: 1 2 3 … k … N
            _btnNumbersHost.SuspendLayout();
            _btnNumbersHost.Controls.Clear();

            foreach (var item in BuildPageItems(currentPage, totalPages))
            {
                if (item is int pageNo)
                {
                    var btn = new Button
                    {
                        Text = pageNo.ToString(),
                        AutoSize = true,
                        Margin = new Padding(2, 2, 2, 2)
                    };

                    // Nút trang hiện tại bôi đậm, disable
                    if (pageNo == currentPage)
                    {
                        btn.Font = new Font(btn.Font, FontStyle.Bold);
                        btn.Enabled = false;
                    }

                    // Bắt sự kiện click → nhảy đến trang
                    btn.Click += async (_, __) => await GoToPageAsync(pageNo);

                    _btnNumbersHost.Controls.Add(btn);
                }
                else
                {
                    // Dấu "…" ngăn cách
                    _btnNumbersHost.Controls.Add(new Label
                    {
                        Text = "…",
                        AutoSize = true,
                        Margin = new Padding(6, 6, 6, 0)
                    });
                }
            }

            _btnNumbersHost.ResumeLayout();
        }

        /// <summary>
        /// Sinh danh sách item cho dải nút số trang.
        /// Trả về chuỗi gồm: [1, 2, ..., current-2..current+2, ..., total-1, total]
        /// chèn "…" (object marker) khi bỏ qua dải dài.
        /// </summary>
        private static IEnumerable<object> BuildPageItems(int current, int total)
        {
            // Không có trang
            if (total <= 0) yield break;

            // Luôn có trang 1
            yield return 1;

            // Nếu tổng = 1 thì xong
            if (total == 1) yield break;

            // Hiển thị trang 2 nếu sát cạnh (tránh 1 … 3)
            if (current > 3) yield return "…";
            else if (total >= 2) yield return 2;

            // Các trang quanh current: current-2 .. current+2
            int start = Math.Max(3, current - 2);
            int end = Math.Min(total - 2, current + 2);
            for (int i = start; i <= end; i++)
                yield return i;

            // Chèn "…" nếu còn khoảng cách tới (total-1)
            if (current < total - 2) yield return "…";
            else if (total - 1 > 2) yield return total - 1;

            // Luôn có trang cuối (total)
            yield return total;
        }
    }
}
