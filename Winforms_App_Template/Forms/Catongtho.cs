using DevExpress.Drawing.Printing;
using DevExpress.Utils;
using DevExpress.XtraEditors;                   // TextEdit, SimpleButton
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;    // BandedGridView, BandedGridColumn
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraLayout;                   // LayoutControl, LayoutControlGroup
using DevExpress.XtraLayout.Utils;             // LayoutMode.Table
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing; // ImageSource
using DevExpress.XtraPrintingLinks;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;                 // PaperKind, Margins
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Database.Table;

namespace Winforms_App_Template.Forms
{
    public partial class Catongtho : Form
    {
        private readonly NewInputs_Table _repo;         // Repository Dapper
        private readonly BindingSource _bs = new();     // Nguồn binding cho Grid
        private CancellationTokenSource? _cts;          // Hủy tải dữ liệu

        private MayBan_Table _mayRepo;                          // repo danh mục máy
        private DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit _repMay = null!;
        private List<MayBan_Model> _listMay = new();

        // == Thanh điều khiển trên cùng (LayoutControl + editors + buttons) ==
        private LayoutControl _layoutTop = null!;
        private TextEdit _txtCongDoan = null!;
        private TextEdit _txtItem = null!;
        private TextEdit _txtLot = null!;
        private TextEdit _txtSoMe = null!;
        private SimpleButton _btnLoad = null!;
        private SimpleButton _btnCancel = null!;
        private SimpleButton _btnExportPdf = null!;
        private SimpleButton _btnPrintPdf = null!;

        public  Catongtho(DbExecutor? db = null)
        {
            InitializeComponent();

            var executor = db ?? new DbExecutor();
            _repo = new NewInputs_Table(executor);
            _mayRepo = new MayBan_Table(executor);
            //await InitMayLookupAsync();     // nếu ctor không async, gọi ở Shown event (đoạn dưới có ví dụ)

            // 1) Gắn nguồn dữ liệu cho GridControl
            gridControl1.DataSource = _bs;                 // BandedGridView sẽ lấy dữ liệu từ _bs
            gridControl1.MainView = advBandedGridView1;       // đảm bảo dùng BandedGridView

            // 3) Dựng layout trên cùng (gồm các ô nhập + nút)
            BuildTopLayoutBar();

            // 4) Ánh xạ FieldName cho các cột có sẵn trong Designer
            WireGridFieldNames();

            // 5) Gắn sự kiện
            _btnLoad.Click += async (_, __) => await LoadDataAsync();
            _btnCancel.Click += (_, __) => _cts?.Cancel();
            _btnExportPdf.Click += (s, e) => btnShowReport_Click(s, e);
            _btnPrintPdf.Click += (_, __) => PrintWholeLayoutAsImage(layoutControl1);

            // Ẩn panel "Drag a column header here to group by that column"
            advBandedGridView1.OptionsView.ShowGroupPanel = false;

            // Không cho kéo thả để group (ẩn luôn thao tác)
            advBandedGridView1.OptionsCustomization.AllowGroup = false;

            // (tuỳ) Ẩn menu group trong context menu
            advBandedGridView1.OptionsMenu.EnableGroupPanelMenu = false;
            advBandedGridView1.OptionsMenu.ShowGroupSortSummaryItems = false;

        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                await InitMayLookupAsync();   // nạp danh mục máy 1 lần
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi nạp danh mục máy");
            }
        }


        // Nạp lookup + gán vào cột IdMayBan
        private async Task InitMayLookupAsync(int? idCongDoanFilter = null, CancellationToken ct = default)
        {
            // 1) Nạp danh mục máy
            _listMay = await _mayRepo.LoadMayBanAsync(idCongDoanFilter, ct);

            // 2) Tạo RepositoryItemLookUpEdit (dropdown hiển thị tên)
            _repMay = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit
            {
                DataSource = _listMay,                                // nguồn dữ liệu lookup
                ValueMember = nameof(MayBan_Model.IdMayBan),          // giá trị thật trong cell (ID)
                DisplayMember = nameof(MayBan_Model.TenMayBan),       // text hiển thị
                NullText = "",                                        // nếu null thì để trống
                BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup
            };
            // Thêm cột hiển thị trong popup
            _repMay.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo(
                nameof(MayBan_Model.TenMayBan), "Tên máy"));
            _repMay.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo(
                nameof(MayBan_Model.MaMayBan), "Mã"));

            // 3) Đăng ký vào Repository của GridControl (để GC quản vòng đời)
            gridControl1.RepositoryItems.Add(_repMay);

            // 4) Gán editor cho cột ID máy (NHỚ: FieldName phải là NewInput_Row.IdMayBan)
            var colIdMay = advBandedGridView1.Columns[nameof(NewInput_Row.IdMay_ban)];
            if (colIdMay != null)
            {
                colIdMay.ColumnEdit = _repMay;                         // render tên thay cho ID
                colIdMay.OptionsColumn.AllowEdit = false;              // chỉ xem
                colIdMay.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.DisplayText; // lọc theo tên
                colIdMay.SortMode = DevExpress.XtraGrid.ColumnSortMode.DisplayText;   // sắp theo tên
            }
        }


        /// <summary>
        /// Tạo thanh điều khiển ở phía trên bằng DevExpress LayoutControl (Table Layout Mode).
        /// </summary>
        private void BuildTopLayoutBar()
        {
            // Tạo LayoutControl và dock lên TOP (đặt trước grid)
            _layoutTop = new LayoutControl
            {
                Dock = DockStyle.Top,
                Height = 100 // chiều cao thanh điều khiển; tùy bạn
            };
            this.Controls.Add(_layoutTop);
            _layoutTop.BringToFront();

            // Root group ở chế độ Table để dễ xếp cột/hàng
            var root = new LayoutControlGroup
            {
                EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True,
                GroupBordersVisible = false,
                LayoutMode = DevExpress.XtraLayout.Utils.LayoutMode.Table
            };
            _layoutTop.Root = root;

            // 8 cột theo %: 4 cặp Label/Editor + khoảng trống + 4 nút
            // Bạn có thể thay đổi tỷ lệ theo bề ngang form của bạn.
            root.OptionsTableLayoutGroup.ColumnDefinitions.AddRange(new[]
            {
                new ColumnDefinition() { SizeType = SizeType.AutoSize },  // Lbl Công đoạn
                new ColumnDefinition() { SizeType = SizeType.Percent, Width = 12 }, // Edit
                new ColumnDefinition() { SizeType = SizeType.AutoSize },  // Lbl Item
                new ColumnDefinition() { SizeType = SizeType.Percent, Width = 12 }, // Edit
                new ColumnDefinition() { SizeType = SizeType.AutoSize },  // Lbl Lot
                new ColumnDefinition() { SizeType = SizeType.Percent, Width = 12 }, // Edit
                new ColumnDefinition() { SizeType = SizeType.AutoSize },  // Lbl Số mẻ
                new ColumnDefinition() { SizeType = SizeType.Percent, Width = 12 }, // Edit
                new ColumnDefinition() { SizeType = SizeType.Percent, Width = 28 }, // Spacer rộng
                new ColumnDefinition() { SizeType = SizeType.AutoSize },  // Load
                new ColumnDefinition() { SizeType = SizeType.AutoSize },  // Hủy
                new ColumnDefinition() { SizeType = SizeType.AutoSize },  // Export PDF
                new ColumnDefinition() { SizeType = SizeType.AutoSize },  // Print
            });

            // 2 hàng: hàng 0 chứa inputs + buttons; hàng 1 có thể để thêm filter phụ (hiện để Absolute 0)
            root.OptionsTableLayoutGroup.RowDefinitions.AddRange(new[]
            {
                new RowDefinition() { SizeType = SizeType.AutoSize },
                new RowDefinition() { SizeType = SizeType.Absolute, Height = 0 }
            });

            // === Editors: Numeric TextEdit với mask cho int ===
            _txtCongDoan = new TextEdit
            {
                Properties = {
                    Mask = { MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric, EditMask = "d0", UseMaskAsDisplayFormat = true },
                    NullValuePrompt = "Id công đoạn", NullValuePromptShowForEmptyValue = true
                }
            };
            _txtItem = new TextEdit
            {
                Properties = {
                    Mask = { EditMask = "d0", UseMaskAsDisplayFormat = true },
                    NullValuePrompt = "ItemNumber", NullValuePromptShowForEmptyValue = true
                }
            };
            _txtLot = new TextEdit
            {
                Properties = {
                    Mask = { EditMask = "d0", UseMaskAsDisplayFormat = true },
                    NullValuePrompt = "LotNo", NullValuePromptShowForEmptyValue = true
                }
            };
            _txtSoMe = new TextEdit
            {
                Properties = {
                    Mask = { MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric, EditMask = "d0", UseMaskAsDisplayFormat = true },
                    NullValuePrompt = "Số mẻ", NullValuePromptShowForEmptyValue = true
                }
            };

            // Gắn dữ liệu mẫu
            _txtCongDoan.Text = "68";
            _txtItem.Text = "CRS25C60G16W";
            _txtLot.Text = "220905G01";
            _txtSoMe.Text = "1";

            // === Buttons ===
            _btnLoad = new SimpleButton { Text = "Tải dữ liệu" };
            _btnCancel = new SimpleButton { Text = "Hủy", Enabled = false };
            _btnExportPdf = new SimpleButton { Text = "Xuất PDF" };
            _btnPrintPdf = new SimpleButton { Text = "In / Preview" };

            // === Add items vào layout, set vị trí theo Table (ColumnIndex/RowIndex) ===
            // Label “Công đoạn”
            var itLblCd = _layoutTop.AddItem("Công đoạn:", null);
            itLblCd.TextVisible = true;
            itLblCd.AppearanceItemCaption.Font = new Font("Segoe UI", 9);
            itLblCd.OptionsTableLayoutItem.RowIndex = 0;
            itLblCd.OptionsTableLayoutItem.ColumnIndex = 0;

            // Editor Công đoạn
            var itCd = _layoutTop.AddItem(string.Empty, _txtCongDoan);
            itCd.TextVisible = false;
            itCd.OptionsTableLayoutItem.RowIndex = 0;
            itCd.OptionsTableLayoutItem.ColumnIndex = 1;

            // Label Item
            var itLblItem = _layoutTop.AddItem("Item:", null);
            itLblItem.TextVisible = true;
            itLblItem.AppearanceItemCaption.Font = new Font("Segoe UI", 9);
            itLblItem.OptionsTableLayoutItem.RowIndex = 0;
            itLblItem.OptionsTableLayoutItem.ColumnIndex = 2;

            var itItem = _layoutTop.AddItem(string.Empty, _txtItem);
            itItem.TextVisible = false;
            itItem.OptionsTableLayoutItem.RowIndex = 0;
            itItem.OptionsTableLayoutItem.ColumnIndex = 3;

            // Label Lot
            var itLblLot = _layoutTop.AddItem("Lot:", null);
            itLblLot.TextVisible = true;
            itLblLot.AppearanceItemCaption.Font = new Font("Segoe UI", 9);
            itLblLot.OptionsTableLayoutItem.RowIndex = 0;
            itLblLot.OptionsTableLayoutItem.ColumnIndex = 4;

            var itLot = _layoutTop.AddItem(string.Empty, _txtLot);
            itLot.TextVisible = false;
            itLot.OptionsTableLayoutItem.RowIndex = 0;
            itLot.OptionsTableLayoutItem.ColumnIndex = 5;

            // Label Số mẻ
            var itLblSoMe = _layoutTop.AddItem("Số mẻ:", null);
            itLblSoMe.TextVisible = true;
            itLblSoMe.AppearanceItemCaption.Font = new Font("Segoe UI", 9);
            itLblSoMe.OptionsTableLayoutItem.RowIndex = 0;
            itLblSoMe.OptionsTableLayoutItem.ColumnIndex = 6;

            var itSoMe = _layoutTop.AddItem(string.Empty, _txtSoMe);
            itSoMe.TextVisible = false;
            itSoMe.OptionsTableLayoutItem.RowIndex = 0;
            itSoMe.OptionsTableLayoutItem.ColumnIndex = 7;

            // Spacer (khoảng trống đẩy buttons sang phải)
            var itSpacer = new EmptySpaceItem();
            root.AddItem(itSpacer);
            itSpacer.AllowHotTrack = false;
            itSpacer.OptionsTableLayoutItem.RowIndex = 0;
            itSpacer.OptionsTableLayoutItem.ColumnIndex = 8;

            // Buttons
            var itBtnLoad = _layoutTop.AddItem(string.Empty, _btnLoad);
            itBtnLoad.TextVisible = false;
            itBtnLoad.OptionsTableLayoutItem.RowIndex = 0;
            itBtnLoad.OptionsTableLayoutItem.ColumnIndex = 9;

            var itBtnCancel = _layoutTop.AddItem(string.Empty, _btnCancel);
            itBtnCancel.TextVisible = false;
            itBtnCancel.OptionsTableLayoutItem.RowIndex = 0;
            itBtnCancel.OptionsTableLayoutItem.ColumnIndex = 10;

            var itBtnExport = _layoutTop.AddItem(string.Empty, _btnExportPdf);
            itBtnExport.TextVisible = false;
            itBtnExport.OptionsTableLayoutItem.RowIndex = 0;
            itBtnExport.OptionsTableLayoutItem.ColumnIndex = 11;

            var itBtnPrint = _layoutTop.AddItem(string.Empty, _btnPrintPdf);
            itBtnPrint.TextVisible = false;
            itBtnPrint.OptionsTableLayoutItem.RowIndex = 0;
            itBtnPrint.OptionsTableLayoutItem.ColumnIndex = 12;
        }

        /// <summary>
        /// Gán FieldName cho các cột đã tạo trong Designer để map property từ NewInput_Row.
        /// </summary>
        private void WireGridFieldNames()
        {
            // CÁCH 1 (khuyến nghị): nếu bạn biết tên biến cột trong Designer
            bdglydokiemtra.FieldName = nameof(NewInput_Row.IdLydoKT);
            bdgngaythaotac.FieldName = nameof(NewInput_Row.StartTime);
            bdgnguoithaotac.FieldName = nameof(NewInput_Row.NguoiTT);
            bdgsomaythaotac.FieldName = nameof(NewInput_Row.IdMay_ban);
            bdgsoluongongdaisudung.FieldName = nameof(NewInput_Row.val1);
            bdgmaquanlythickness.FieldName = nameof(NewInput_Row.val3);
            bdgduongkinhngoaiongdai.FieldName = nameof(NewInput_Row.val4);
            bdgmapingauge.FieldName = nameof(NewInput_Row.val6);
            bdgduongkinhtrong.FieldName = nameof(NewInput_Row.val7);
            bdgtrangthaicat.FieldName = nameof(NewInput_Row.val9);
            bdgchieudaicat.FieldName = nameof(NewInput_Row.val11);
            bdgthuocsudung.FieldName = nameof(NewInput_Row.val10);
            bdgsoluongsudung.FieldName = nameof(NewInput_Row.SLSudung);
            //bdgcatvat.FieldName = nameof(NewInput_Row.);
            //bdgbep.FieldName = nameof(NewInput_Row);
            //bdgbavia.FieldName = nameof(NewInput_Row);
            //bdgroi.FieldName = nameof(NewInput_Row);
            //bdgchieudaingoaitieuchuan.FieldName = nameof(NewInput_Row);
            //bdgkhac.FieldName = nameof(NewInput_Row);
            //bdgxacnhantonluu.FieldName = nameof(NewInput_Row);

            // CÁCH 2: map theo Caption đang có trong Designer (linh hoạt khi tên biến auto)
            //void MapByCaption(string caption, string property)
            //{
            //    var col = advBandedGridView1.Columns.Cast<BandedGridColumn>()
            //               .FirstOrDefault(c => string.Equals(c.Caption, caption, StringComparison.OrdinalIgnoreCase));
            //    if (col != null) col.FieldName = property;
            //}

            //// Ví dụ mapping – sửa lại CAPTION cho đúng caption bạn đã đặt trong Designer
            //MapByCaption("ID", nameof(NewInput_Row.IdCongDoan));
            //MapByCaption("Bắt đầu", nameof(NewInput_Row.StartTime));
            //MapByCaption("Người TT", nameof(NewInput_Row.NguoiTT));
            //MapByCaption("Ghi chú", nameof(NewInput_Row.Remark));

            //MapByCaption("Công đoạn", nameof(NewInput_Row.IdCongDoan));
            //MapByCaption("Mã máy", nameof(NewInput_Row.IdMayBan));
            //MapByCaption("Số máy", nameof(NewInput_Row.SoMay));
            //MapByCaption("Item", nameof(NewInput_Row.ItemNumber));
            //MapByCaption("Lot", nameof(NewInput_Row.LotNo));
            //MapByCaption("Số Mẻ", nameof(NewInput_Row.SoMe));

            //MapByCaption("SL sử dụng", nameof(NewInput_Row.SLSuDung));
            //MapByCaption("OK", nameof(NewInput_Row.OKQty));
            //MapByCaption("NG", nameof(NewInput_Row.NGQty));

            //// Định dạng thêm cho cột số & ngày (nếu cột tồn tại)
            //void FormatNumber(string property, string fmt)
            //{
            //    var col = advBandedGridView1.Columns[property];
            //    if (col != null)
            //    {
            //        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            //        col.DisplayFormat.FormatString = fmt;       // "n0" hoặc "n2"
            //        col.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            //    }
            //}
            //FormatNumber(nameof(NewInput_Row.SLSuDung), "n2");
            //FormatNumber(nameof(NewInput_Row.OKQty), "n0");
            //FormatNumber(nameof(NewInput_Row.NGQty), "n0");

            //var dtCol = advBandedGridView1.Columns[nameof(NewInput_Row.StartTime)];
            //if (dtCol != null)
            //{
            //    dtCol.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            //    dtCol.DisplayFormat.FormatString = "dd/MM/yyyy HH:mm";
            //}
        }

        // Ví dụ: gọi từ một Form (ví dụ khi nhấn nút)
        private void btnShowReport_Click(object sender, EventArgs e)
        {
            string connStr = "Server=.;Database=YourDB;Trusted_Connection=True;";
            // hoặc từ appsettings.json

            //var rows = await InspectionRepository.GetRowsAsync(connStr, top: 20);
            var rows = new List<Catthoong_Model>
            {
                new Catthoong_Model {
                    Reason="Định kỳ",
                    TimeAndWorker="09:30 2025-10-20\nNguyễn A",
                    Machine="V-01",
                    QtyUsed=100, QtyCut=30,
                    ThickGauge="PR-IK-0001",
                    OuterDiameter="⌀ 2.80",
                    Pin098="OK",
                    InnerJudge="通過 / Xuyên",
                    CutState="OK",
                    CutLengths="100 / 100 / 100",
                    Acceptance="OK",
                    NG_CatVat=0, NG_Bep=0, NG_Bavia=1, NG_Roi=0, NG_LengthOut=0, NG_Khac=0
                }
                //},
                //new Catthoong_Model {
                //    Reason="Bất thường",
                //    TimeAndWorker="10:15 2025-10-20\nLê B",
                //    Machine="V-02",
                //    QtyUsed=90, QtyCut=25,
                //    ThickGauge="PR-IK-0014",
                //    OuterDiameter="⌀ 2.79",
                //    Pin098="NG",
                //    InnerJudge="不通過 / Không xuyên",
                //    CutState="NG",
                //    CutLengths="98 / 99 / 100",
                //    Acceptance="NG",
                //    NG_CatVat=0, NG_Bep=2, NG_Bavia=0, NG_Roi=1, NG_LengthOut=1, NG_Khac=0
                //}
            };

            var rpt = new Testreport();
            rpt.DataSource = rows;     // List<InspectionRow>
                                       // rpt.DataMember = null;  // không cần cho List<T>

            // (nếu bạn chưa set ExpressionBindings trong Designer,
            //  có thể set bằng code như ví dụ dưới)
            // rpt.FindControl("cellReason", true).ExpressionBindings.Add(
            //     new ExpressionBinding("BeforePrint", "Text", "[Reason]"));

            new ReportPrintTool(rpt).ShowRibbonPreviewDialog();
        }
        private void ApplyGridPrintSettingsRecursive(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is GridControl gc)
                {
                    switch (gc.MainView)
                    {
                        case AdvBandedGridView abv:
                            ConfigureAdvBanded(abv);
                            break;

                        case BandedGridView bv:
                            ConfigureBanded(bv);
                            break;

                        case GridView gv:
                            ConfigureGrid(gv);
                            break;

                        case ColumnView:
                            // Fallback: KHÔNG set RowAutoHeight/AppearancePrint.Row ở đây
                            // vì lớp base ColumnView không có các thuộc tính đó.
                            break;
                    }
                }

                // đi sâu các control con
                ApplyGridPrintSettingsRecursive(child);
            }
        }

        private void ConfigureAdvBanded(AdvBandedGridView v)
        {
            // In/Export: co chiều ngang để vừa trang
            v.OptionsPrint.AutoWidth = true;
            v.OptionsPrint.PrintHeader = true;
            v.OptionsPrint.PrintBandHeader = true;

            // Hiển thị & in văn bản dài: wrap + auto height
            v.OptionsView.RowAutoHeight = true;                              // chỉ có trên AdvBandedGridView
            v.Appearance.Row.TextOptions.WordWrap = WordWrap.Wrap;           // on-screen
            v.AppearancePrint.Row.TextOptions.WordWrap = WordWrap.Wrap;      // khi in/xuất
        }

        private void ConfigureBanded(BandedGridView v)
        {
            v.OptionsPrint.AutoWidth = true;
            v.OptionsPrint.PrintHeader = true;
            v.OptionsPrint.PrintBandHeader = true;

            v.OptionsView.RowAutoHeight = true;
            v.Appearance.Row.TextOptions.WordWrap = WordWrap.Wrap;
            v.AppearancePrint.Row.TextOptions.WordWrap = WordWrap.Wrap;
        }

        private void ConfigureGrid(GridView v)
        {
            v.OptionsPrint.AutoWidth = true;
            v.OptionsPrint.PrintHeader = true;

            v.OptionsView.RowAutoHeight = true;
            v.Appearance.Row.TextOptions.WordWrap = WordWrap.Wrap;
            v.AppearancePrint.Row.TextOptions.WordWrap = WordWrap.Wrap;
        }

        private void PrintWholeLayoutAsImage(LayoutControl layoutRoot)
        {
            // 1) Kiểm tra thư viện in ấn
            if (!layoutRoot.IsPrintingAvailable)
            {
                MessageBox.Show("Thiếu DevExpress.XtraPrinting", "Error");
                return;
            }
            // 2) Cấu hình GridControl bên trong để phần lưới rõ nét (in vector)
            foreach (Control c in layoutRoot.Controls)
                ApplyGridPrintSettingsRecursive(c);

            // 2) (Tùy chọn) In theo bố cục y như UI (WYSIWYG)
            //    Ở phiên bản của bạn, dùng OptionsPrint (không phải OptionsPrintControl)
            layoutRoot.OptionsPrint.OldPrinting = true;

            // 3) Tạo hệ in ấn + link tới chính LayoutControl
            using var ps = new PrintingSystem();
            var link = new PrintableComponentLink(ps)
            {
                Component = layoutRoot,             // LayoutControl là IPrintable
                PaperKind = DXPaperKind.A3,         // KHỔ GIẤY A3
                Landscape = true,                   // In ngang (false = dọc)
                Margins = new Margins(20, 20, 20, 20) // Lề trái/phải/trên/dưới (đơn vị 1/100 inch)
            };

            // 4) Tạo tài liệu và mở preview
            link.CreateDocument();
            new PrintTool(ps).ShowRibbonPreviewDialog();

            // 1) Đảm bảo control đã có handle & layout xong
            //if (!layoutRoot.IsHandleCreated)
            //    layoutRoot.CreateControl();
            //layoutRoot.PerformLayout();
            //layoutRoot.Refresh();

            //// 2) Lấy kích thước vẽ
            //var rect = layoutRoot.DisplayRectangle;
            //int w = Math.Max(1, rect.Width);
            //int h = Math.Max(1, rect.Height);
            //if (w <= 1 || h <= 1)
            //    throw new InvalidOperationException("Layout chưa sẵn sàng để chụp (kích thước quá nhỏ).");

            //// 3) Chụp bitmap
            //using var bmpRaw = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            //layoutRoot.DrawToBitmap(bmpRaw, new Rectangle(Point.Empty, new Size(w, h)));

            //// 4) Clone ra ảnh độc lập và giữ sống nó cho tới sau preview
            //var bmp = (Bitmap)bmpRaw.Clone();
            //try
            //{
            //    // 5) Đưa vào XtraReport và scale theo khổ giấy
            //    var rpt = new XtraReport
            //    {
            //        PaperKind = DXPaperKind.A3,
            //        Landscape = true,
            //        Margins = new System.Drawing.Printing.Margins(10, 10, 10, 10)
            //    };
            //    float pageW = rpt.PageWidth - rpt.Margins.Left - rpt.Margins.Right;
            //    float scale = pageW / w;

            //    var detail = new DetailBand { HeightF = h * scale };
            //    rpt.Bands.Add(detail);

            //    var pic = new XRPictureBox
            //    {
            //        Image = bmp, // <-- dùng System.Drawing.Image thay vì ImageSource.FromImage(...)
            //        Sizing = ImageSizeMode.StretchImage,
            //        LocationF = new DevExpress.Utils.PointFloat(0, 0),
            //        SizeF = new System.Drawing.SizeF(w * scale, h * scale)
            //    };
            //    detail.Controls.Add(pic);

            //    rpt.CreateDocument();
            //    rpt.ShowRibbonPreview();
            //}
            //finally
            //{
            //    bmp.Dispose();
            //}
        }


        // <summary>Xuất PDF trực tiếp từ GridControl(nhanh, đúng A4/A3).</summary>
        private void PrintPreview()
        {
            // 1) Cấu hình in cho View (fit chiều ngang)
            advBandedGridView1.OptionsPrint.PrintHeader = true;
            advBandedGridView1.OptionsPrint.PrintBandHeader = true;
            advBandedGridView1.OptionsPrint.AutoWidth = true;    // co cột để vừa trang

            // 2) Tạo PrintingSystem + Link
            using var ps = new PrintingSystem();
            var link = new PrintableComponentLink(ps)
            {
                Component = gridControl1,
                PaperKind = DXPaperKind.A4,                      // <-- DXPaperKind (không phải System.Drawing)
                Landscape = true,
                Margins = new Margins(20, 20, 20, 20)
                // Không còn AutoFitToPagesWidth ở bản này
            };

            link.CreateDocument();                               // render tài liệu
            new PrintTool(ps).ShowRibbonPreviewDialog();         // preview
        }
        /// <summary>
        /// In nhiều gridcontrol
        /// </summary>
        private void PrintAllGridsInLayout()
        {
            using var ps = new PrintingSystem();
            var composite = new CompositeLink(ps)
            {
                PaperKind = DXPaperKind.A3,
                Landscape = true,
                Margins = new Margins(20, 20, 20, 20)
            };

            // Ví dụ trong layout còn gridControl2, gridControl3...
            void AddGrid(DevExpress.XtraGrid.GridControl gc)
            {
                var l = new PrintableComponentLink(ps) { Component = gc };
                composite.Links.Add(l);
            }

            AddGrid(gridControl1);
            AddGrid(gridControl2);
            AddGrid(gridControl3);

            composite.CreateDocument();
            new PrintTool(ps).ShowRibbonPreviewDialog();
        }

        //private void PrintWholeLayoutAsImage()
        //{
        //    // Fallback to printing a bitmap of the Form using System.Drawing.Printing
        //    using var bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
        //    this.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));

        //    var pd = new PrintDocument();
        //    pd.DefaultPageSettings.Landscape = true;
        //    pd.PrintPage += (s, e) =>
        //    {
        //        var marginBounds = e.MarginBounds;
        //        float scale = Math.Min((float)marginBounds.Width / bmp.Width, (float)marginBounds.Height / bmp.Height);
        //        var destW = (int)(bmp.Width * scale);
        //        var destH = (int)(bmp.Height * scale);
        //        var destX = marginBounds.Left + (marginBounds.Width - destW) / 2;
        //        var destY = marginBounds.Top + (marginBounds.Height - destH) / 2;
        //        e.Graphics.DrawImage(bmp, new Rectangle(destX, destY, destW, destH));
        //        e.HasMorePages = false;
        //    };

        //    using var preview = new PrintPreviewDialog { Document = pd };
        //    preview.ShowDialog(this);
        //}


        private void ExportPdf()
        {
            // Giữ nguyên cấu hình Print của View
            advBandedGridView1.OptionsPrint.AutoWidth = true;

            using var ps = new PrintingSystem();
            var link = new PrintableComponentLink(ps)
            {
                Component = gridControl1,
                PaperKind = DXPaperKind.A4,
                Landscape = true,
                Margins = new Margins(20, 20, 20, 20)
            };
            link.CreateDocument();

            using var dlg = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = "BaoCao.pdf" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
                link.ExportToPdf(dlg.FileName);                  // xuất PDF
        }

        /// <summary>
        /// Tải dữ liệu từ DB và bind vào grid.
        /// </summary>
        private async Task LoadDataAsync()
        {
            // Hủy job cũ nếu còn
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                _btnLoad.Enabled = false;
                _btnCancel.Enabled = true;
                UseWaitCursor = true;

                // Lấy filter từ UI (ví dụ TextBox); ở đây demo giá trị hardcode
                int idCongDoan = int.Parse(_txtCongDoan.Text);
                string itemNumber = _txtItem.Text;
                string lotNo =_txtLot.Text;
                int soMe = int.Parse(_txtSoMe.Text);

                await InitMayLookupAsync(idCongDoan);   // nạp lookup theo công đoạn

                // Gọi Repository
                var result = await _repo.Get_Cat_Ong_Tho(idCongDoan, itemNumber, lotNo, soMe, _cts.Token);

                // Bind vào grid (Rows là List<NewInput_Row>)
                _bs.DataSource = new BindingList<NewInput_Row>(result.Rows);

                // Tự fit cột cho đẹp
                advBandedGridView1.BestFitColumns();
            }
            catch (OperationCanceledException)
            {
                // người dùng bấm Hủy
                // có thể hiện status nếu muốn
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                _btnLoad.Enabled = true;
                _btnCancel.Enabled = false;
            }
        }
    }
}
