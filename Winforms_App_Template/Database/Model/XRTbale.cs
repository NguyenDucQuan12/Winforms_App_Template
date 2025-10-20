// ==========================
// Winforms_App_Template/Database/Model/InspectionTableFactory.cs
// ==========================
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System.Drawing;

namespace Winforms_App_Template.Database.Model
{
    public static class InspectionTableFactory
    {
        // Tạo một ô nhanh
        private static XRTableCell Cell(string text, float w, bool bold = false, bool center = true)
        {
            return new XRTableCell
            {
                Text = text,
                WidthF = w,
                Multiline = true,
                CanGrow = true,
                Padding = new PaddingInfo(2, 2, 2, 2, 100f),
                Font = new Font("Arial Unicode MS", bold ? 9f : 8.5f, bold ? FontStyle.Bold : FontStyle.Regular),
                TextAlignment = center ? TextAlignment.MiddleCenter : TextAlignment.MiddleLeft,
                Borders = BorderSide.Left | BorderSide.Top | BorderSide.Right | BorderSide.Bottom
            };
        }

        // Placeholder cho hàng dưới của một ô RowSpan=2
        private static XRTableCell Placeholder(float w)
        {
            return new XRTableCell
            {
                WidthF = w,
                Borders = BorderSide.None,
                Text = string.Empty
            };
        }

        // Header 2 tầng (theo hình)
        public static XRTable CreateHeader(float totalWidth)
        {
            // Phân bổ bề rộng (tổng = 100%)
            float wReason = totalWidth * 0.08f;  // Lý do kiểm tra
            float wTimeWorker = totalWidth * 0.12f;  // Giờ/Ngày/Ng thao tác
            float wMachine = totalWidth * 0.06f;  // Số máy
            float wTubeGroup = totalWidth * 0.12f;  // Nhóm "Số lượng dùng / Số ống cắt"
            float wThickGauge = totalWidth * 0.09f;  // Mã thước dày
            float wOuterD = totalWidth * 0.09f;  // D ngoài (mm)
            float wPin098 = totalWidth * 0.09f;  // Pin gauge 0.98
            float wInnerJudge = totalWidth * 0.12f;  // Đường kính trong (4F/4KF)
            float wCutState = totalWidth * 0.10f;  // Trạng thái cắt (10 ống)
            float wCutLen = totalWidth * 0.09f;  // Chiều dài cắt (3 ống)
            float wAcceptance = totalWidth * 0.04f;  // Kết quả xác nhận tồn lưu

            float wTubeUsed = wTubeGroup / 2f;    // cột con 1
            float wTubeCut = wTubeGroup / 2f;    // cột con 2

            var table = new XRTable
            {
                WidthF = totalWidth,
                Borders = BorderSide.All
            };
            table.BeginInit();

            // ===== Row 1 (Header cấp 1) =====
            var r1 = new XRTableRow { HeightF = 28f };

            var c_reason = Cell("検査理由\nLý do kiểm tra", wReason, bold: true); c_reason.RowSpan = 2;
            var c_timeWorker = Cell("作業時間・日・担当者\nGiờ, Ngày/ tháng thao tác , Người thao tác", wTimeWorker, bold: true); c_timeWorker.RowSpan = 2;
            var c_machine = Cell("生産設備\nSố máy sản xuất", wMachine, bold: true); c_machine.RowSpan = 2;

            // Nhóm 2 cột: Số lượng dùng / Số ống cắt
            var c_tubeGroup = Cell("長尺チューブ使用数・カット数\nSố lượng ống dài sử dụng / Số ống cắt được (本sp)",
                                    wTubeGroup, bold: true);

            var c_thickGauge = Cell("【シックネスゲージ】測定レンジ\nMã quan lý thickness gauge", wThickGauge, bold: true); c_thickGauge.RowSpan = 2;
            var c_outerD = Cell("ダイレータ長尺チューブの外径\nĐường kính ngoài ống dài (mm)", wOuterD, bold: true); c_outerD.RowSpan = 2;
            var c_pin098 = Cell("[0.98mmピンゲージ]測定レンジ\nMã pingauge 0.98mm", wPin098, bold: true); c_pin098.RowSpan = 2;
            var c_innerJudge = Cell("内径判定（4F&4KF）\nĐường kính trong (loại 4F & 4KF)\nTiêu chuẩn: 4F: 不通過/Không xuyên | 4KF: 通過/Xuyên", wInnerJudge, bold: true); c_innerJudge.RowSpan = 2;
            var c_cutState = Cell("カット状態（10本確認）\nTrạng thái cắt (10 ống)", wCutState, bold: true); c_cutState.RowSpan = 2;
            var c_cutLen = Cell("カット長（3本確認）\nChiều dài cắt (3 ống) (mm)", wCutLen, bold: true); c_cutLen.RowSpan = 2;
            var c_accept = Cell("測定機管理\nKết quả xác nhận tồn lưu", wAcceptance, bold: true); c_accept.RowSpan = 2;

            r1.Cells.AddRange(new[] {
                c_reason, c_timeWorker, c_machine,
                c_tubeGroup,
                c_thickGauge, c_outerD, c_pin098, c_innerJudge, c_cutState, c_cutLen, c_accept
            });

            // ===== Row 2 (Header cấp 2) =====
            var r2 = new XRTableRow { HeightF = 24f };

            // placeholders cho các ô đã RowSpan=2
            r2.Cells.AddRange(new[]
            {
                Placeholder(wReason),
                Placeholder(wTimeWorker),
                Placeholder(wMachine),

                // Hai cột con dưới nhóm
                Cell("投入数\nSố lượng sử dụng (本sp)", wTubeUsed, bold:false),
                Cell("カット数\nSố ống cắt (本sp)", wTubeCut, bold:false),

                Placeholder(wThickGauge),
                Placeholder(wOuterD),
                Placeholder(wPin098),
                Placeholder(wInnerJudge),
                Placeholder(wCutState),
                Placeholder(wCutLen),
                Placeholder(wAcceptance)
            });

            table.Rows.AddRange(new[] { r1, r2 });
            table.EndInit();
            return table;
        }

        // Hai hàng dữ liệu mẫu cho 1 "bản ghi"
        public static XRTable CreateSampleDetailTwoRows(float totalWidth)
        {
            float wReason = totalWidth * 0.08f;
            float wTimeWorker = totalWidth * 0.12f;
            float wMachine = totalWidth * 0.06f;
            float wTubeGroup = totalWidth * 0.12f;
            float wThickGauge = totalWidth * 0.09f;
            float wOuterD = totalWidth * 0.09f;
            float wPin098 = totalWidth * 0.09f;
            float wInnerJudge = totalWidth * 0.12f;
            float wCutState = totalWidth * 0.10f;
            float wCutLen = totalWidth * 0.09f;
            float wAcceptance = totalWidth * 0.04f;

            float wTubeUsed = wTubeGroup / 2f;
            float wTubeCut = wTubeGroup / 2f;

            var tbl = new XRTable { WidthF = totalWidth, Borders = BorderSide.All };
            tbl.BeginInit();

            // ===== Detail Row 1 =====
            var d1 = new XRTableRow { HeightF = 22f };

            var d_reason = Cell("Định kỳ", wReason); d_reason.RowSpan = 2;
            var d_time = Cell("09:30 2025-10-20\nNguyễn A", wTimeWorker); d_time.RowSpan = 2;
            var d_mach = Cell("V-01", wMachine); d_mach.RowSpan = 2;

            var d_used = Cell("100", wTubeUsed);                    // Số lượng sử dụng
            var d_thick = Cell("PR-IK-0001", wThickGauge); d_thick.RowSpan = 2;
            var d_od = Cell("⌀ 2.80", wOuterD); d_od.RowSpan = 2;
            var d_pin = Cell("OK", wPin098); d_pin.RowSpan = 2;
            var d_inner = Cell("通過 / Xuyên", wInnerJudge); d_inner.RowSpan = 2;
            var d_state = Cell("OK", wCutState); d_state.RowSpan = 2;
            var d_len = Cell("100 / 100 / 100", wCutLen); d_len.RowSpan = 2;
            var d_acc = Cell("OK", wAcceptance); d_acc.RowSpan = 2;

            d1.Cells.AddRange(new[] { d_reason, d_time, d_mach, d_used,  // 4 cell đầu
                                      d_thick, d_od, d_pin, d_inner, d_state, d_len, d_acc });

            // ===== Detail Row 2 =====
            var d2 = new XRTableRow { HeightF = 22f };

            // placeholders tương ứng cho các ô đã RowSpan=2
            var ph_reason = Placeholder(wReason);
            var ph_time = Placeholder(wTimeWorker);
            var ph_mach = Placeholder(wMachine);

            // cột con thứ 2 của nhóm: Số ống cắt
            var d_cut = Cell("30", wTubeCut);

            var ph_thick = Placeholder(wThickGauge);
            var ph_od = Placeholder(wOuterD);
            var ph_pin = Placeholder(wPin098);
            var ph_inner = Placeholder(wInnerJudge);
            var ph_state = Placeholder(wCutState);
            var ph_len = Placeholder(wCutLen);
            var ph_acc = Placeholder(wAcceptance);

            d2.Cells.AddRange(new[] { ph_reason, ph_time, ph_mach, d_cut,
                                      ph_thick, ph_od, ph_pin, ph_inner, ph_state, ph_len, ph_acc });

            tbl.Rows.AddRange(new[] { d1, d2 });
            tbl.EndInit();
            return tbl;
        }
    }
}
