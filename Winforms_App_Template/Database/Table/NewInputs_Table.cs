using Dapper;
using DevExpress.CodeParser;
using DevExpress.Pdf.ContentGeneration.Interop;
using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;
using static DevExpress.XtraBars.Docking2010.Views.BaseRegistrator;

namespace Winforms_App_Template.Database.Table
{
    internal class NewInputs_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public NewInputs_Table(DbExecutor db) => _db = db; // Contructor lấy db

        public async Task<Report_Header_Model?> Get_Report_Header(int IdCongDoan, string ItemNumber, string LotNo, int So_Me, CancellationToken ct = default)
        {
            var get_report_header_query = @"
                SELECT
                    CAST(ni.idCongDoan AS nvarchar(20)) AS ID_Congdoan, 
                    cd.TenCongDoan                        AS Name_Congdoan,
                    cd.MaCongDoan                         AS Code_Congdoan,
                    ni.ItemNumber                         AS Category_Code,
                    ni.LotNo                              AS Lotno_Congdoan,
                    ni.So_Me                              AS Batch_Number,     
                    SUM(ni.OKQty)                         AS OK_Qty_Total,
                    SUM(ni.NGQty)                         AS NG_Qty_Total
                FROM 
                    tblNewInput AS ni
                LEFT JOIN 
                    tblCongDoan AS cd
                ON 
                    ni.idCongDoan = cd.idCongDoan
                WHERE
                    ni.idCongDoan = @IdCongDoan AND
                    ni.ItemNumber = @ItemNumber AND
                    ni.LotNo      = @LotNo AND
                    ni.So_Me      = @So_Me
                GROUP BY
                    ni.idCongDoan, cd.TenCongDoan, cd.MaCongDoan, ni.LotNo, ni.So_Me, ni.ItemNumber; 
                ";
            var param = new
            {
                IdCongDoan,
                ItemNumber,
                LotNo,
                So_Me
            };

            // Thực thi
            var rows = (await _db.QueryAsync<Report_Header_Model>(get_report_header_query, param, ct: ct)).ToList();
            // Trả dòng đầu nếu có, hoặc null nếu không có
            return rows.FirstOrDefault();
        }
        public async Task<List<New_Input_Row>> Get_Detail_Table(int IdCongDoan, string ItemNumber, string LotNo, int So_Me, CancellationToken ct = default)
        {
            var cat_ong_tho_query = @"
                SELECT 
                        ni.idInput,             -- ID Form nhập dữ liệu
                        ld.MaKT,                -- Mã kiểm tra
                        mb.TenMay_Ban,          -- Tên máy bàn
                        ni.SLSudung,            -- Số lượng sử dụng
                        ni.StartTime,           -- Thời gian bắt đầu
                        ni.NguoiTT,             -- Người thao tác
                        ni.Remark,              -- Ghi chú cho mỗi dòng thao tác
                        ni.OKQty,               -- Số lượng hàng phù hợp
                        ni.NGQty,               -- Số lượng hàng không phù hợp
                        ni.val1,                -- Số lượng ống dài sử dụng
                        ni.val2,                -- Số lượng ống dài cắt được
                        ni.val3,                -- Mã quản lý thicness gauge
                        ni.val4,                -- Đường kính ngoài ống dài
                        ni.val5,                -- Đường kính ngoài ống dài yes no
                        ni.val6,                -- Mã pingauge 098mm
                        ni.val7,                -- Đường kính trong loại 4Fr, 4KFr xuyên (yes no)
                        ni.val8,                -- Đường kính trong loại 4Fr, 4KFr không xuyên (yes no)
                        ni.val9,                -- Trạng thái cắt 10 ống
                        ni.val10,               -- Mã thước sử dụng
                        ni.val11,               -- Thước sử dụng 1
                        ni.val12,               -- Thước sử dụng 2
                        ni.val13,               -- Thước sử dụng 3
                        ni.val14,               -- Thước sử dụng yes no
                        ni.val15,               -- Kết quả xác nhận tồn lưu yes no
                        ni.val16,               -- Để cho các mục đích trong tương lai
                        ni.val17,               -- Để cho các mục đích trong tương lai
                        ni.val18,               -- Để cho các mục đích trong tương lai
                        ni.val19,               -- Để cho các mục đích trong tương lai
                        ni.val20,               -- Để cho các mục đích trong tương lai
                        ni.val21,               -- Để cho các mục đích trong tương lai
                        ni.val22,               -- Để cho các mục đích trong tương lai
                        ni.val23,               -- Để cho các mục đích trong tương lai
                        ni.val24,               -- Để cho các mục đích trong tương lai
                        ni.val25,               -- Để cho các mục đích trong tương lai
                        ni.val26,               -- Để cho các mục đích trong tương lai
                        ni.val27,               -- Để cho các mục đích trong tương lai
                        ni.val28,               -- Để cho các mục đích trong tương lai
                        ni.val29,               -- Để cho các mục đích trong tương lai
                        ni.val30,               -- Để cho các mục đích trong tương lai
                        ni.val31,               -- Để cho các mục đích trong tương lai
                        ni.val32                -- Để cho các mục đích trong tương lai

                FROM
                        tblNewInput AS ni
                LEFT JOIN 
                        tblMay_Ban        AS mb ON mb.IdMay_ban   = ni.IdMay_ban
                LEFT JOIN 
                        tblLydoKT         AS ld ON ld.idLydoKT    = ni.idLydoKT
                WHERE
                        ni.IdCongDoan = @IdCongDoan AND
                        ni.ItemNumber = @ItemNumber AND
                        ni.LotNo      = @LotNo      AND
                        ni.So_Me      = @So_Me
                ORDER BY
                        ni.StartTime DESC;
            ";

            var param = new
            {
                IdCongDoan,
                ItemNumber,
                LotNo,
                So_Me
            };

            // Thực thi
            var rows = (await _db.QueryAsync<New_Input_Row>(cat_ong_tho_query, param, ct: ct)).ToList();

            return rows;
        }

        /// <summary>
        /// Lấy bảng tổng sản phẩm cho TOÀN BỘ CÁC MẺ trong 1 lô (CRS25 / RS25).
        /// Dữ liệu lấy từ stored procedure dbo.usp_GetLotSummary.
        /// </summary>
        public async Task<List<Lot_Summary_Row>> Get_Lot_Summary(
            string itemNumber,
            string lotNo,
            CancellationToken ct = default)
        {
            // Tên procedure
            const string procName = "dbo.usp_GetLotSummary";

            // Tham số truyền vào SP
            var param = new
            {
                ItemNumber = itemNumber,
                ItemNumberRS = itemNumber.Substring(1),
                LotNo = lotNo
            };

            // Gọi Dapper qua DbExecutor (thi hành SP)
            var rows = await _db.QueryAsync<Lot_Summary_Row>(
                sql: procName,
                param: param,
                commandType: CommandType.StoredProcedure,
                ct: ct);

            // Chuyển sang List để dễ dùng
            return rows.ToList();
        }
    }
}

/// Tạo Procedue
//CREATE PROCEDURE dbo.usp_GetLotSummary
//(
//      @ItemNumber NVARCHAR(50)   -- Ví dụ: 'CRS25-001'
//     ,@ItemNumberRS NVARCHAR(50)   --Ví dụ: 'RS25-001'
//     ,@LotNo NVARCHAR(50)   --Ví dụ: 'L2024-001'
//)
//AS
//BEGIN
//    SET NOCOUNT ON;

//-------------------------------------------------------------
//--C R S 2 5 ...
//    --  - Số lượng sử dụng   : tổng SLSudung ở công đoạn 68 (cắt ống)
//    --  - Số hàng phù hợp    : tổng OKQty ở công đoạn 144 (kiểm tra ống sau cắt)
//    --  - Số hàng không phù hợp
//    --        = tổng NGQty ở cả 2 công đoạn 68 và 144
//    -------------------------------------------------------------
//    ; WITH CRS AS
//    (
//        SELECT
//            Loai                 = N'CRS25',
//            So_luong_su_dung = SUM(
//                                        CASE
//                                            WHEN ni.idCongDoan = 68
//                                            THEN ISNULL(ni.SLSudung, 0)
//                                            ELSE 0
//                                        END
//                                     ),
//            So_hang_phu_hop = SUM(
//                                        CASE
//                                            WHEN ni.idCongDoan = 144
//                                            THEN ISNULL(ni.OKQty, 0)
//                                            ELSE 0
//                                        END
//                                     ),
//            So_hang_khong_phu_hop = SUM(
//                                        CASE
//                                            WHEN ni.idCongDoan IN(68, 144)
//                                            THEN ISNULL(ni.NGQty, 0)
//                                            ELSE 0
//                                        END
//                                      )
//        FROM tblNewInput AS ni
//        WHERE ni.ItemNumber = @ItemNumber
//          AND ni.LotNo      = @LotNo
//    ),

//    -------------------------------------------------------------
//    --  R S 2 5 ...
//    --  - Số lượng sử dụng   : tổng SLSudung ở công đoạn 70 (cắm chốt)
//    --  - Số hàng phù hợp    : tổng SLChuyenCDSau ở công đoạn 76 
//    --                         (Kiểm tra lần cuối / Kiểm tra công đoạn)
//    --  - Số hàng không phù hợp:
//    --        = tổng NGQty của các công đoạn 70,71,175,72,73,74,75,76
//    -------------------------------------------------------------
//    RS AS
//    (
//        SELECT
//            Loai                 = N'RS25',
//            So_luong_su_dung = SUM(
//                                        CASE
//                                            WHEN ni.idCongDoan = 70
//                                            THEN ISNULL(ni.SLSudung, 0)
//                                            ELSE 0
//                                        END
//                                     ),

//            -- Số hàng phù hợp = "số lượng hàng chuyển sang công đoạn sau"
//            -- ở công đoạn kiểm tra công đoạn (ở đây giả sử idCongDoan = 76)
//            So_hang_phu_hop      = SUM(
//                                        CASE 
//                                            WHEN ni.idCongDoan = 76 
//                                            THEN ISNULL(ni.OKQty, 0) 
//                                            ELSE 0 
//                                        END
//                                     ),

//            -- Tổng NG của chuỗi công đoạn từ cắm chốt -> kiểm tra lần cuối
//            So_hang_khong_phu_hop = SUM(
//                                        CASE 
//                                            WHEN ni.idCongDoan IN (70,71,175,72,73,74,75,76)
//                                            THEN ISNULL(ni.NGQty, 0) 
//                                            ELSE 0 
//                                        END
//                                      )
//        FROM tblNewInput AS ni
//        WHERE ni.ItemNumber = @ItemNumberRS
//          AND ni.LotNo      = @LotNo
//    )

//    -------------------------------------------------------------
//    --  Trả về 2 dòng: CRS25..., RS25... cho lô @ItemNumber / @LotNo
//    -------------------------------------------------------------
//    SELECT
//        ItemNumber            = @ItemNumber,
//        LotNo = @LotNo,
//        Loai,
//        So_luong_su_dung = ISNULL(So_luong_su_dung, 0),
//        So_hang_phu_hop = ISNULL(So_hang_phu_hop, 0),
//        So_hang_khong_phu_hop = ISNULL(So_hang_khong_phu_hop, 0)
//    FROM CRS

//    UNION ALL

//    SELECT
//        ItemNumber            = @ItemNumberRS,
//        LotNo = @LotNo,
//        Loai,
//        So_luong_su_dung = ISNULL(So_luong_su_dung, 0),
//        So_hang_phu_hop = ISNULL(So_hang_phu_hop, 0),
//        So_hang_khong_phu_hop = ISNULL(So_hang_khong_phu_hop, 0)
//    FROM RS;
//END
//GO
///