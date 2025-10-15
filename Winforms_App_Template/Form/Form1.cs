using Microsoft.EntityFrameworkCore;           // ToListAsync, EF APIs
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Entities;

namespace Winforms_App_Template
{
    /// <summary>
    /// MainForm: chứa 2 tab quản lý ThongTinNhanVien & DuLieuOT.
    /// </summary>
    public partial class Form1 : Form
    {
        private readonly AppDbContext _db;     // DbContext inject từ Program.cs

        // ===== TabControl tổng =====
        private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };

        // ===== Tab 1: Nhân viên =====
        private readonly BindingSource _bsNv = new BindingSource();
        private readonly DataGridView _gridNv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly TextBox _txtNvSearch = new TextBox { Width = 180, PlaceholderText = "Tìm (Code/Name/Dept)..." };
        private readonly TextBox _txtNvDept = new TextBox { Width = 120, PlaceholderText = "Bộ phận (BoPhan)" };
        private readonly TextBox _txtNvStatus = new TextBox { Width = 120, PlaceholderText = "StatusSyn" };
        private readonly DateTimePicker _dtNvHireFrom = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _dtNvHireTo = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short };
        private readonly NumericUpDown _numNvPage = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1, Width = 60 };
        private readonly NumericUpDown _numNvSize = new NumericUpDown { Minimum = 1, Maximum = 1000, Value = 10, Width = 60 };
        private readonly Button _btnNvLoad = new Button { Text = "Load" };
        private readonly Button _btnNvUpsert = new Button { Text = "Upsert" };
        private readonly Button _btnNvDelete = new Button { Text = "Delete" };
        private readonly Label _lblNvMeta = new Label { AutoSize = true, Padding = new Padding(12, 8, 0, 0) };

        // ===== Tab 2: Dữ liệu OT =====
        private readonly BindingSource _bsOt = new BindingSource();
        private readonly DataGridView _gridOt = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly TextBox _txtOtSearch = new TextBox { Width = 180, PlaceholderText = "Tìm (Code/Name/Dept/Type)..." };
        private readonly TextBox _txtOtDept = new TextBox { Width = 120, PlaceholderText = "Bộ phận (BoPhan)" };
        private readonly DateTimePicker _dtOtFrom = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _dtOtTo = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short };
        private readonly NumericUpDown _numOtPage = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1, Width = 60 };
        private readonly NumericUpDown _numOtSize = new NumericUpDown { Minimum = 1, Maximum = 1000, Value = 10, Width = 60 };
        private readonly Button _btnOtLoad = new Button { Text = "Load" };
        private readonly Button _btnOtUpsert = new Button { Text = "Upsert" };
        private readonly Button _btnOtUpdateStatus = new Button { Text = "Update Status" };
        private readonly Button _btnOtDelete = new Button { Text = "Delete" };
        private readonly Label _lblOtMeta = new Label { AutoSize = true, Padding = new Padding(12, 8, 0, 0) };

        public Form1(AppDbContext db)
        {
            _db = db;

            // ====== Cấu hình Form tổng ======
            Text = "WinForms + EF Core | PK=CodeEmp | NV & OT";
            Width = 1200;
            Height = 800;

            // ====== Lắp TabControl vào Form ======
            Controls.Add(_tabs);

            // Tạo 2 tab
            var tabNv = new TabPage("Nhân viên");
            var tabOt = new TabPage("Dữ liệu OT");
            _tabs.TabPages.Add(tabNv);
            _tabs.TabPages.Add(tabOt);

            // ====== UI Tab Nhân viên ======
            {
                // Panel trên: filter + thao tác
                var top = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Height = 44,
                    Padding = new Padding(8),
                    FlowDirection = FlowDirection.LeftToRight
                };

                // Nhãn gợi ý (đẹp mắt, tự giãn)
                top.Controls.Add(new Label { Text = "Search:", AutoSize = true, Padding = new Padding(0, 12, 0, 0) });
                top.Controls.Add(_txtNvSearch);
                top.Controls.Add(new Label { Text = "Dept:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_txtNvDept);
                top.Controls.Add(new Label { Text = "Status:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_txtNvStatus);
                top.Controls.Add(new Label { Text = "Hire From:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_dtNvHireFrom);
                top.Controls.Add(new Label { Text = "To:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_dtNvHireTo);
                top.Controls.Add(new Label { Text = "Page:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_numNvPage);
                top.Controls.Add(new Label { Text = "Size:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_numNvSize);

                top.Controls.Add(_btnNvLoad);
                top.Controls.Add(_btnNvUpsert);
                top.Controls.Add(_btnNvDelete);
                top.Controls.Add(_lblNvMeta);

                // Grid: cấu hình cột data-binding
                _gridNv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                _gridNv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                _gridNv.MultiSelect = false;
                _gridNv.DataSource = _bsNv;

                _gridNv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CodeEmp", HeaderText = "CodeEmp", Width = 120 });
                _gridNv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProfileName", HeaderText = "Name", Width = 200 });
                _gridNv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Gender", HeaderText = "Gender", Width = 80 });
                _gridNv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DateHire", HeaderText = "DateHire", Width = 120 });
                _gridNv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BoPhan", HeaderText = "BoPhan", Width = 160 });
                _gridNv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DateQuit", HeaderText = "DateQuit", Width = 120 });
                _gridNv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StatusSyn", HeaderText = "StatusSyn", Width = 120 });

                // Lắp vào Tab
                tabNv.Controls.Add(_gridNv);
                tabNv.Controls.Add(top);

                // Sự kiện nút
                _btnNvLoad.Click += async (_, __) => await LoadNvPageAsync();
                _btnNvUpsert.Click += async (_, __) => await UpsertNvAsync();
                _btnNvDelete.Click += async (_, __) => await DeleteNvAsync();
            }

            // ====== UI Tab Dữ liệu OT ======
            {
                var top = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Height = 44,
                    Padding = new Padding(8),
                    FlowDirection = FlowDirection.LeftToRight
                };

                top.Controls.Add(new Label { Text = "Search:", AutoSize = true, Padding = new Padding(0, 12, 0, 0) });
                top.Controls.Add(_txtOtSearch);
                top.Controls.Add(new Label { Text = "Dept:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_txtOtDept);
                top.Controls.Add(new Label { Text = "From:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_dtOtFrom);
                top.Controls.Add(new Label { Text = "To:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_dtOtTo);
                top.Controls.Add(new Label { Text = "Page:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_numOtPage);
                top.Controls.Add(new Label { Text = "Size:", AutoSize = true, Padding = new Padding(12, 12, 0, 0) });
                top.Controls.Add(_numOtSize);

                top.Controls.Add(_btnOtLoad);
                top.Controls.Add(_btnOtUpsert);
                top.Controls.Add(_btnOtUpdateStatus);
                top.Controls.Add(_btnOtDelete);
                top.Controls.Add(_lblOtMeta);

                _gridOt.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                _gridOt.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                _gridOt.MultiSelect = false;
                _gridOt.DataSource = _bsOt;

                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CodeEmp", HeaderText = "CodeEmp", Width = 120 });
                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProfileName", HeaderText = "Name", Width = 180 });
                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BoPhan", HeaderText = "BoPhan", Width = 120 });
                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "WorkDateRoot", HeaderText = "WorkDateRoot", Width = 120 });
                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InTime", HeaderText = "InTime", Width = 100 });
                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OutTime", HeaderText = "OutTime", Width = 100 });
                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RegisterHours", HeaderText = "RegisterHours", Width = 120 });
                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OvertimeTypeName", HeaderText = "OT Type", Width = 120 });
                _gridOt.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", Width = 100 });

                tabOt.Controls.Add(_gridOt);
                tabOt.Controls.Add(top);

                _btnOtLoad.Click += async (_, __) => await LoadOtPageAsync();
                _btnOtUpsert.Click += async (_, __) => await UpsertOtAsync();
                _btnOtUpdateStatus.Click += async (_, __) => await UpdateOtStatusAsync();
                _btnOtDelete.Click += async (_, __) => await DeleteOtAsync();
            }

            // Tải trang đầu tiên cho mỗi tab
            Shown += async (_, __) =>
            {
                await LoadNvPageAsync();
                await LoadOtPageAsync();
            };
        }

        // ========== NV: Load/Upsert/Delete ==========

        private async Task LoadNvPageAsync()
        {
            int page = (int)_numNvPage.Value;
            int size = (int)_numNvSize.Value;

            var (total, items) = await ThongTinNhanVienQueries.GetPageAsync(
                _db,
                keyword: _txtNvSearch.Text,
                boPhan: string.IsNullOrWhiteSpace(_txtNvDept.Text) ? null : _txtNvDept.Text.Trim(),
                statusSyn: string.IsNullOrWhiteSpace(_txtNvStatus.Text) ? null : _txtNvStatus.Text.Trim(),
                hireFrom: _dtNvHireFrom.Value.Date,
                hireTo: _dtNvHireTo.Value.Date,
                page: page,
                pageSize: size);

            _bsNv.DataSource = items;
            _lblNvMeta.Text = $"  Total={total} | Page={page} | Size={size}";
        }

        private async Task UpsertNvAsync()
        {
            // Demo: popup đơn giản bằng InputBox kém tiện → Ở thực tế bạn làm form editor riêng.
            // Ở đây ta upsert 1 record mẫu bằng code để minh hoạ.
            var code = Prompt("CodeEmp?", "NV Upsert") ?? "E001";
            if (string.IsNullOrWhiteSpace(code)) return;

            var dto = new ThongTinNhanVien
            {
                CodeEmp = code.Trim(),
                ProfileName = Prompt("ProfileName?", "NV Upsert") ?? "Nguyen Van A",
                Gender = Prompt("Gender?", "NV Upsert") ?? "Nam",
                DateHire = DateTime.TryParse(Prompt("DateHire (yyyy-MM-dd)?", "NV Upsert"), out var dh) ? dh : null,
                BoPhan = Prompt("BoPhan?", "NV Upsert") ?? "IT",
                DateQuit = DateTime.TryParse(Prompt("DateQuit (yyyy-MM-dd)?", "NV Upsert"), out var dq) ? dq : null,
                StatusSyn = Prompt("StatusSyn?", "NV Upsert") ?? "Active"
            };

            await ThongTinNhanVienQueries.UpsertAsync(_db, dto);
            await LoadNvPageAsync();
            MessageBox.Show(this, "NV Upsert OK", "Info");
        }

        private async Task DeleteNvAsync()
        {
            if (_bsNv.Current == null) return;
            var code = GetCurrentCell(_bsNv, "CodeEmp");
            if (code == null) return;

            if (MessageBox.Show(this, $"Delete NV {code}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var ok = await ThongTinNhanVienQueries.DeleteAsync(_db, code);
                if (!ok) MessageBox.Show(this, "Not found", "Warn");
                await LoadNvPageAsync();
            }
        }

        // ========== OT: Load/Upsert/UpdateStatus/Delete ==========

        private async Task LoadOtPageAsync()
        {
            int page = (int)_numOtPage.Value;
            int size = (int)_numOtSize.Value;

            var (total, items) = await DuLieuOtQueries.GetPageAsync(
                _db,
                keyword: _txtOtSearch.Text,
                boPhan: string.IsNullOrWhiteSpace(_txtOtDept.Text) ? null : _txtOtDept.Text.Trim(),
                fromDate: _dtOtFrom.Value.Date,
                toDate: _dtOtTo.Value.Date,
                page: page,
                pageSize: size);

            _bsOt.DataSource = items;
            _lblOtMeta.Text = $"  Total={total} | Page={page} | Size={size}";
        }

        private async Task UpsertOtAsync()
        {
            var code = Prompt("CodeEmp?", "OT Upsert") ?? "E001";
            if (string.IsNullOrWhiteSpace(code)) return;

            var dto = new DuLieuOt
            {
                CodeEmp = code.Trim(),
                ProfileName = Prompt("ProfileName?", "OT Upsert") ?? "Nguyen Van A",
                BoPhan = Prompt("BoPhan?", "OT Upsert") ?? "IT",
                WorkDateRoot = DateTime.TryParse(Prompt("WorkDateRoot (yyyy-MM-dd)?", "OT Upsert"), out var d) ? d.Date : DateTime.Today,
                InTime = TimeSpan.TryParse(Prompt("InTime (HH:mm)?", "OT Upsert"), out var tIn) ? tIn : (TimeSpan?)null,
                OutTime = TimeSpan.TryParse(Prompt("OutTime (HH:mm)?", "OT Upsert"), out var tOut) ? tOut : (TimeSpan?)null,
                RegisterHours = decimal.TryParse(Prompt("RegisterHours?", "OT Upsert"), out var rh) ? rh : null,
                OvertimeTypeName = Prompt("OvertimeTypeName?", "OT Upsert") ?? "Weekday",
                Status = Prompt("Status?", "OT Upsert") ?? "Pending"
            };

            await DuLieuOtQueries.UpsertAsync(_db, dto);
            await LoadOtPageAsync();
            MessageBox.Show(this, "OT Upsert OK", "Info");
        }

        private async Task UpdateOtStatusAsync()
        {
            var code = Prompt("CodeEmp to update status?", "OT Status") ?? "E001";
            if (string.IsNullOrWhiteSpace(code)) return;

            var status = Prompt("New Status?", "OT Status") ?? "Approved";
            var ok = await DuLieuOtQueries.UpdateStatusAsync(_db, code.Trim(), status.Trim());
            if (!ok) MessageBox.Show(this, "Not found", "Warn");
            await LoadOtPageAsync();
        }

        private async Task DeleteOtAsync()
        {
            if (_bsOt.Current == null) return;
            var code = GetCurrentCell(_bsOt, "CodeEmp");
            if (code == null) return;

            if (MessageBox.Show(this, $"Delete OT of {code}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var ok = await DuLieuOtQueries.DeleteAsync(_db, code);
                if (!ok) MessageBox.Show(this, "Not found", "Warn");
                await LoadOtPageAsync();
            }
        }

        // ===== Helpers: Prompt input và lấy cell hiện tại =====

        private static string? Prompt(string text, string caption)
        {
            // Hộp thoại input cực đơn giản (demo). Thực tế bạn nên làm form editor chuẩn.
            var form = new Form { Width = 420, Height = 160, Text = caption, StartPosition = FormStartPosition.CenterParent };
            var lbl = new Label { Left = 10, Top = 20, Text = text, AutoSize = true };
            var txt = new TextBox { Left = 10, Top = 50, Width = 380 };
            var ok = new Button { Text = "OK", Left = 220, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Left = 310, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };
            form.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
            form.AcceptButton = ok; form.CancelButton = cancel;

            return form.ShowDialog() == DialogResult.OK ? txt.Text : null;
        }

        private static string? GetCurrentCell(BindingSource bs, string propName)
        {
            var cur = bs.Current;
            if (cur == null) return null;
            var prop = cur.GetType().GetProperty(propName);
            return prop?.GetValue(cur)?.ToString();
        }
    }
}