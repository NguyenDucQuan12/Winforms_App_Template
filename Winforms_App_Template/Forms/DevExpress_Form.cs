using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Database.Table;

namespace Winforms_App_Template.Forms
{
    public partial class DevExpress_Form : Form
    {
        // == Hạ tầng ==
        private readonly DuLieuOt_Table? _repo;        // Repository nghiệp vụ (nullable để hỗ trợ Designer ctor)
        private CancellationTokenSource? _cts;         // Hủy tải

        // -- Controls thêm vào bằng code (có thể chuyển sang Designer nếu muốn)
        private readonly DateTimePicker _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm:ss" };
        private readonly DateTimePicker _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm:ss" };
        private readonly TextBox _txtCodeEmp = new TextBox { Width = 150, PlaceholderText = "CodeEmp (optional)" };
        private readonly NumericUpDown _numPage = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1, Width = 70 };
        private readonly NumericUpDown _numSize = new NumericUpDown { Minimum = 1, Maximum = 1000, Value = 100, Width = 70 };
        private readonly Button _btnLoad = new Button { Text = "Load" };

        // Constructor cho Designer (phải có để VS Designer hoạt động)
        public DevExpress_Form(DbExecutor? db = null)
        {
            // Repo: dùng DbExecutor (nếu chưa DI) — connString lấy từ DbConfig
            var executor = db ?? new DbExecutor();
            _repo = new DuLieuOt_Table(executor);

            InitializeComponent();

            // Khởi tạo UI bổ sung (dùng FlowLayoutPanel ở trên cùng của form)
            InitializeRuntimeControls();
        }

        // Constructor thực tế (sử dụng DI / runtime) — truyền repo vào để dùng trong LoadDataAsync
        public DevExpress_Form(DuLieuOt_Table repo) : this()
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        // Hàm thêm các control và đoạn wiring event — tách ra để code rõ ràng
        private void InitializeRuntimeControls()
        {
            // Tạo panel top để chứa filter controls
            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,               // đặt ở trên cùng form
                Height = 40,                        // chiều cao panel
                Padding = new Padding(8),
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false
            };

            // Add control với label rõ ràng — comment ngắn cho từng dòng
            top.Controls.Add(new Label { Text = "From:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
            _dtFrom.Value = DateTime.Today.AddDays(-7); // mặc định: 7 ngày trước
            top.Controls.Add(_dtFrom);

            top.Controls.Add(new Label { Text = "To:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
            _dtTo.Value = DateTime.Today.AddDays(1).AddSeconds(-1); // mặc định: tới cuối ngày hôm nay
            top.Controls.Add(_dtTo);

            top.Controls.Add(new Label { Text = "CodeEmp:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
            top.Controls.Add(_txtCodeEmp);

            top.Controls.Add(new Label { Text = "Page:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
            top.Controls.Add(_numPage);

            top.Controls.Add(new Label { Text = "Size:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
            top.Controls.Add(_numSize);

            // Nút Load: click sẽ gọi LoadDataAsync
            _btnLoad.Click += async (_, __) => await LoadDataAsync();
            top.Controls.Add(_btnLoad);

            // Thêm panel top vào Controls của form (đặt trước gridControl nếu gridControl đã được thêm trong Designer)
            Controls.Add(top);

            // Nếu gridControl1 đã được tạo trong Designer, nó sẽ nằm ở dưới panel
            // Nếu không, bạn cần thêm GridControl/DevExpress tạo ở Designer.
        }

        // Sự kiện nút demo (nếu còn)
        private async void button1_Click(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        // Tải dữ liệu bất đồng bộ, đọc giá trị từ UI và gán vào PageRequest
        private async Task LoadDataAsync()
        {
            // Nếu chưa có repo (ví dụ chạy từ Designer), báo và return
            if (_repo == null)
            {
                MessageBox.Show(this, "Repository chưa được khởi tạo. Khi chạy thực tế hãy khởi tạo form với DI (DevLieuOt_Table).", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Tạo CancellationTokenSource cục bộ cho lần tải này
                using var cts = new CancellationTokenSource();
                _cts = cts; // lưu để có thể hủy từ UI khác nếu cần

                // Đọc trang/size từ control
                int page = (int)_numPage.Value;        // trang hiện tại
                int size = (int)_numSize.Value;        // số dòng / trang

                // Đọc filter CodeEmp (Nếu rỗng => null để repo ignore filter)
                string? codeEmp = string.IsNullOrWhiteSpace(_txtCodeEmp.Text) ? null : _txtCodeEmp.Text.Trim();

                // Đọc ngày từ DateTimePicker; nếu bạn muốn cho phép rỗng, có thể thêm checkbox để enable/disable filter ngày
                DateTime? fromDate = _dtFrom.Checked ? _dtFrom.Value : _dtFrom.Value; // ví dụ: luôn dùng giá trị; chỉnh theo UI nếu cần
                DateTime? toDate = _dtTo.Checked ? _dtTo.Value : _dtTo.Value;

                // Tạo PageRequest dùng kiểu DateTime? đúng chuẩn (không dùng chuỗi)
                var req = new PageRequest
                {
                    Page = page,                     // trang
                    Size = size,                     // kích thước trang
                    SortBy = "WorkDateRoot",         // cột sắp xếp (repo bảo vệ whitelist)
                    Desc = true,                     // sắp giảm dần
                    CodeEmp = codeEmp ?? "",         // truyền chuỗi rỗng nếu null; repo có thể kiểm tra
                    FromDate = fromDate,             // gán DateTime? trực tiếp
                    ToDate = toDate                  // gán DateTime? trực tiếp
                };

                // Tải trang từ repository (có thể là Dapper + SQL server)
                var pageResult = await _repo.QueryPageAsync(req, cts.Token);

                // Bind dữ liệu vào DevExpress GridControl:
                // - Dùng BindingList để Grid có thể phản ánh thay đổi (nếu cần)
                // - Nếu list lớn, cân nhắc virtual mode / server-side paging
                var list = new BindingList<DuLieuOt_Model>(pageResult.Items.ToList());

                // Nếu bạn dùng gridControl1 (DevExpress), gán DataSource cho GridControl (không cho GridView trực tiếp)
                gridControl1.BeginUpdate();                       // bắt đầu block update để tránh flicker
                gridControl1.DataSource = list;                   // gán datasource
                gridControl1.EndUpdate();                         // kết thúc update

                // Bạn có thể cập nhật label/trạng thái
                // ví dụ: lblMeta.Text = $"Total={pageResult.Total} | Page={page} | Size={size}";
            }
            catch (OperationCanceledException)
            {
                // Tải bị hủy — thường không cần show lỗi
            }
            catch (Exception ex)
            {
                // Hiện lỗi cho user và log
                MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Giải phóng CancellationTokenSource nếu còn
                _cts?.Dispose();
                _cts = null;
            }
        }

        // Nếu muốn hủy tải từ một nút khác, gọi _cts?.Cancel();
        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }
    }
}