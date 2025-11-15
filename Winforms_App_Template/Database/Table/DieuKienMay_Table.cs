using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    internal class DieuKienMay_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public DieuKienMay_Table(DbExecutor db) => _db = db; // Contructor lấy db

        public async Task<List<Dieu_kien_may_Model>> Get_Detail_Only_Dieu_Kien_May(
           IEnumerable<int> idInputs, CancellationToken ct = default)
        {
            // Kiểm tra dữ liệu đầu vào: nếu mảng rỗng → trả rỗng (tránh SQL IN ())
            var ids = idInputs?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0) return new List<Dieu_kien_may_Model>();

            var Get_detail_error_query = @"
                SELECT
                     [idInput]
                    ,[NguoiThaoTac]
                    ,[Thoigian]
                    ,[Remark]
                    ,[val99] as Ly_do_kiem_tra  -- Lý do kiểm tra nếu mã kiểm tra là V: khác
                    ,[val100] as MaKT  : Mã kiểm tra
                    ,[val1], [val2], [val3], [val4], [val5], [val6], [val7], [val8], [val9], [val10]
                    ,[val11], [val12], [val13], [val14], [val15], [val16], [val17], [val18], [val19], [val10]
                    ,[val21], [val22], [val23], [val24], [val25], [val26], [val27], [val28], [val29], [val30]
                    ,[val31], [val32], [val33], [val34], [val35], [val36], [val37], [val38], [val39], [val40]
                    ,[val41], [val42], [val43], [val44], [val45], [val46], [val47], [val48], [val49], [val50]
                    ,[val51], [val52], [val53], [val54] --, [val55], [val56], [val57], [val58], [val59], [val74]

                FROM 
                    [MESPlus].[dbo].[tblInputDKM]
                WHERE 
                    [idInput] IN @idInputs
                ORDER BY 
                    [Thoigian]; 
                ";
            var param = new
            {
                idInputs
            };

            // Thực thi
            var rows = (await _db.QueryAsync<Dieu_kien_may_Model>(Get_detail_error_query, param, ct: ct));
            // Trả về danh sách số lượng lỗi trong các lần nhập
            return rows.ToList();
        }

        public async Task<List<Dieu_kien_may_Model>> Get_Detail_Dieu_Kien_May(
           IEnumerable<int> idInputs, CancellationToken ct = default)
        {
            // Kiểm tra dữ liệu đầu vào: nếu mảng rỗng → trả rỗng (tránh SQL IN ())
            var ids = idInputs?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0) return new List<Dieu_kien_may_Model>();

            var Get_detail_error_query = @"
                SELECT
                  d.[idInput]
                , d.[NguoiThaoTac]
                , d.[Thoigian]
                , d.[Remark]
                , d.[val99]  AS Ly_do_kiem_tra     -- Lý do kiểm tra nếu mã kiểm tra là V: khác
                , d.[val100] AS MaKT               -- Mã kiểm tra
                , mb.TenMay_Ban                    -- Tên máy bàn lấy từ tblMay_Ban
                , dbo.func_01_getDisplayName_by_userName(ISNULL(d.[NguoiThaoTac], '')) AS TenNguoiThaoTac             -- Tên hiển thị của người thao tác
                , d.[val1],  d.[val2],  d.[val3],  d.[val4],  d.[val5]
                , d.[val6],  d.[val7],  d.[val8],  d.[val9],  d.[val10]
                , d.[val11], d.[val12], d.[val13], d.[val14], d.[val15]
                , d.[val16], d.[val17], d.[val18], d.[val19], d.[val20]
                , d.[val21], d.[val22], d.[val23], d.[val24], d.[val25]
                , d.[val26], d.[val27], d.[val28], d.[val29], d.[val30]
                , d.[val31], d.[val32], d.[val33], d.[val34], d.[val35]
                , d.[val36], d.[val37], d.[val38], d.[val39], d.[val40]
                , d.[val41], d.[val42], d.[val43], d.[val44], d.[val45]
                , d.[val46], d.[val47], d.[val48], d.[val49], d.[val50]
                , d.[val51], d.[val52], d.[val53], d.[val54], d.[val55]
                , d.[val56], d.[val57], d.[val58], d.[val59], d.[val74]
            FROM 
                [MESPlus].[dbo].[tblInputDKM] AS d
                LEFT JOIN tblNewInput  AS ni ON ni.idInput   = d.idInput
                LEFT JOIN tblMay_Ban   AS mb ON mb.IdMay_ban = ni.IdMay_ban
            WHERE 
                d.[idInput] IN @idInputs   -- hoặc IN (SELECT idInput FROM @idInputs) nếu là TVP
            ORDER BY 
                d.[Thoigian]; 
             ";
            var param = new
            {
                idInputs
            };

            // Thực thi
            var rows = (await _db.QueryAsync<Dieu_kien_may_Model>(Get_detail_error_query, param, ct: ct));
            // Trả về danh sách số lượng lỗi trong các lần nhập
            return rows.ToList();
        }
    }
}
