using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Winforms_App_Template.Database.Model;

/// <summary>
/// Xây DataTable tổng kết cuối cùng (CRS25..., RS25...)
/// từ dữ liệu các công đoạn (blocks).
/// </summary>
public static class FinalLotSummaryBuilder
{
    /// <summary>
    /// Xây DataTable tổng kết cho subreport.
    /// Mỗi dòng tương ứng 1 loại: CRS25..., RS25...
    /// </summary>
    /// <param name="itemNumber">
    /// Mã sản phẩm – dùng để quyết định sẽ tạo dòng CRS25 hay RS25.
    /// </param>
    /// <param name="blocks">
    /// Map: Id công đoạn -> Data_Step_Model đã được FetchAllStepsAsync chuẩn bị.
    /// </param>
    public static DataTable BuildFinalSummaryTable(
        string itemNumber,
        IReadOnlyDictionary<int, Data_Step_Model> blocks)
    {
        // 1) Tạo DataTable kết quả với các cột cần thiết
        var table = new DataTable("FinalSummary");

        // Cột "Loai" : nhãn hiển thị, ví dụ "CRS25...", "RS25..."
        table.Columns.Add("Loai", typeof(string));

        // Cột "SoLuongSuDung" : tổng số lượng sử dụng
        table.Columns.Add("SoLuongSuDung", typeof(int));

        // Cột "SoHangPhuHop" : tổng số hàng phù hợp
        table.Columns.Add("SoHangPhuHop", typeof(int));

        // Cột "SoHangKhongPH" : tổng số hàng không phù hợp
        table.Columns.Add("SoHangKhongPH", typeof(int));

        // Chuẩn hoá ItemNumber về uppercase để so sánh
        var itemUpper = (itemNumber ?? string.Empty).ToUpperInvariant();

        // ---------------------------------------------------
        // DÒNG CRS25...
        // ---------------------------------------------------
        if (itemUpper.StartsWith("CRS"))
        {
            // Tính toán 1 dòng theo công thức CRS25
            var crsRow = BuildCrsRow(blocks, itemNumber);

            // Thêm dòng vào DataTable
            table.Rows.Add(
                crsRow.Loai,
                crsRow.SoLuongSuDung,
                crsRow.SoHangPhuHop,
                crsRow.SoHangKhongPH);
        }

        // ---------------------------------------------------
        // DÒNG RS25... (hoặc CS25...) – tuỳ business của bạn
        // ---------------------------------------------------
        if (itemUpper.StartsWith("RS") || itemUpper.StartsWith("CS"))
        {
            // Tính toán 1 dòng theo công thức RS25
            var rsRow = BuildRsRow(blocks, itemNumber);

            table.Rows.Add(
                rsRow.Loai,
                rsRow.SoLuongSuDung,
                rsRow.SoHangPhuHop,
                rsRow.SoHangKhongPH);
        }

        return table;
    }

    // ====== struct nội bộ lưu kết quả 1 dòng ======

    private readonly struct SummaryRow
    {
        public SummaryRow(string loai, int used, int ok, int ng)
        {
            Loai = loai;
            SoLuongSuDung = used;
            SoHangPhuHop = ok;
            SoHangKhongPH = ng;
        }

        public string Loai { get; }
        public int SoLuongSuDung { get; }
        public int SoHangPhuHop { get; }
        public int SoHangKhongPH { get; }
    }

    // ====== Công thức cho CRS25... ======

    private static SummaryRow BuildCrsRow(
        IReadOnlyDictionary<int, Data_Step_Model> blocks,
        string itemNumber)
    {
        // Id công đoạn theo mô tả:
        //  - 68  : Cắt ống
        //  - 144 : Kiểm tra ống sau cắt
        const int StepCatOng = 68;
        const int StepKiemTraSauCatOng = 144;

        // Hàm local lấy block nếu tồn tại
        Data_Step_Model? TryGet(int id) =>
            blocks.TryGetValue(id, out var b) ? b : null;

        var catOng = TryGet(StepCatOng);
        var ktSauCat = TryGet(StepKiemTraSauCatOng);

        // 1) Số lượng sử dụng:
        //    "Cộng số lượng đầu vào ở vị trí 'cắt ống' của các mẻ với nhau"
        //    Ở đây mình dùng TotalSLSudung của step 68.
        var used = catOng?.TotalSLSudung ?? 0;

        // 2) Số hàng phù hợp:
        //    "Cộng số lượng hàng phù hợp ở vị trí 'kiểm tra ống sau cắt'"
        //    → dùng TotalOKQty của step 144.
        var ok = ktSauCat?.TotalOKQty ?? 0;

        // 3) Số hàng không phù hợp:
        //    "Cộng số lượng hàng không phù hợp ở vị trí 'cắt ống' & 'kiểm tra ống sau cắt'"
        //    → TotalNGQty của step 68 + step 144.
        var ng = (catOng?.TotalNGQty ?? 0)
               + (ktSauCat?.TotalNGQty ?? 0);

        // Nhãn hiển thị – tuỳ bạn, mình ghép ItemNumber cho dễ trace
        var label = $"CRS25 ({itemNumber})";

        return new SummaryRow(label, used, ok, ng);
    }

    // ====== Công thức cho RS25... ======

    private static SummaryRow BuildRsRow(
        IReadOnlyDictionary<int, Data_Step_Model> blocks,
        string itemNumber)
    {
        // Id công đoạn:
        //  - 70  : Cắm chốt
        //  - 76  : Kiểm tra lần cuối (đóng vai trò "Kiểm tra công đoạn")
        const int StepCamChot = 70;
        const int StepKiemTraCongDoan = 76;

        // Các công đoạn nằm trong chuỗi
        // "từ vị trí cắm chốt - Kiểm tra lần cuối"
        var stepIdsFromCamChotToLast = new[] { 70, 71, 175, 72, 73, 74, 75, 76 };

        Data_Step_Model? TryGet(int id) =>
            blocks.TryGetValue(id, out var b) ? b : null;

        var camChot = TryGet(StepCamChot);
        var ktCongDoan = TryGet(StepKiemTraCongDoan);

        // 1) Số lượng sử dụng:
        //    "Cộng số lượng đầu vào ở vị trí 'cắm chốt' "
        //    → dùng TotalSLSudung của step 70.
        var used = camChot?.TotalSLSudung ?? 0;

        // 2) Số hàng phù hợp:
        //    "Cộng 'số lượng hàng chuyển sang công đoạn sau'
        //     của các mẻ ở chỗ 'Kiểm tra công đoạn'"
        //
        //    Theo Report_Header_Model:
        //       OK_Qty_Total = tổng số lượng hàng chuyển công đoạn sau.
        var ok = ktCongDoan?.Header.OK_Qty_Total
                 ?? ktCongDoan?.TotalOKQty
                 ?? 0;

        // 3) Số hàng không phù hợp:
        //    "Cộng ở cột 'tổng số lượng hàng không phù hợp cả mẻ'
        //     từ vị trí cắm chốt - Kiểm tra lần cuối"
        //
        //    → tổng TotalNGQty của các step 70,71,175,72,73,74,75,76.
        var ng = 0;
        foreach (var id in stepIdsFromCamChotToLast)
        {
            if (!blocks.TryGetValue(id, out var b)) continue;
            ng += b.TotalNGQty;
        }

        var label = $"RS25 ({itemNumber})";

        return new SummaryRow(label, used, ok, ng);
    }
}
