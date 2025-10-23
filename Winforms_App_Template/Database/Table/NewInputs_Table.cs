using Dapper;
using DevExpress.Pdf.ContentGeneration.Interop;
using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    internal class NewInputs_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public NewInputs_Table(DbExecutor db) => _db = db; // Contructor lấy db

        public async Task<Catongtho_HeaderModel> Get_Header_catthoong(int IdCongDoan, string ItemNumber, string LotNo, int So_Me, CancellationToken ct = default)
        {
            var get_header_catthoong_query = @"
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
            var rows = (await _db.QueryAsync<Catongtho_HeaderModel>(get_header_catthoong_query, param, ct: ct)).ToList();
            // Trả dòng đầu nếu có, hoặc null nếu không có
            return rows.FirstOrDefault();
        }
        public async Task<List<Catthoong_Row>> Get_Cat_Ong_Tho(int IdCongDoan, string ItemNumber, string LotNo, int So_Me, CancellationToken ct = default)
        {
            var cat_ong_tho_query = @"
                SELECT 
                        ni.idInput,
                        ld.MaKT,
                        mb.TenMay_Ban,
                        ni.SLSudung,
                        ni.StartTime,
                        ni.NguoiTT,
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
                        ni.Remark               -- Ghi chú cho mỗi dòng thao tác
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
            var rows = (await _db.QueryAsync<Catthoong_Row>(cat_ong_tho_query, param, ct: ct)).ToList();

            return rows;
        }
    }
}
