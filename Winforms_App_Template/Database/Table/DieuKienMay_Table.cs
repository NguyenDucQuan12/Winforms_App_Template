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

        public async Task<List<Dieu_kien_may_Model>> Get_Detail_Dieu_Kien_May(
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
                    ,[val1], [val2], [val3], [val4], [val5], [val6], [val7], [val8], [val9], [val10]
                    ,[val11], [val12], [val13], [val14], [val15], [val16], [val17], [val18], [val19], [val10]
                    ,[val21], [val22], [val23], [val24], [val25], [val26], [val27], [val28], [val29], [val30]
                    ,[val31], [val32], [val33], [val34], [val35], [val36], [val37], [val38], [val39], [val40]
                    ,[val41], [val42], [val43], [val44], [val45], [val46], [val47], [val48], [val49], [val50]
                    --,[val51], [val52], [val53], [val54], [val55], [val56], [val57], [val58], [val59], [val60]

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
    }
}
