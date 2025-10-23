using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    internal class Input_Error_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public Input_Error_Table(DbExecutor db) => _db = db; // Contructor lấy db

        public async Task<List<Input_Error_Model>> Get_Detail_Error(IEnumerable<int> idInputs, CancellationToken ct = default)
        {

            // Kiểm tra dữ liệu đầu vào: nếu mảng rỗng → trả rỗng (tránh SQL IN ())
            var ids = idInputs?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0) return new List<Input_Error_Model>();

            var Get_detail_error_query = @"
                SELECT
                    a.[id],
                    a.[idInput],
                    a.[idError],
                    a.[Qty],
                    a.[Remark],
                    a.[NguoiThaoTac],
                    a.[ThoiGian],
                    b.[TenLoi]
                FROM 
                    [MESPlus].[dbo].[tblInput_Error] AS a
                LEFT JOIN 
                    [MESPlus].[dbo].[tblErrorMaster] AS b  ON b.[idError] = a.[idError]
                WHERE 
                    a.[idInput] IN @idInputs
                ORDER BY 
                    a.[idInput], a.[idError]; 
                ";
            var param = new
            {
                idInputs
            };

            // Thực thi
            var rows = (await _db.QueryAsync<Input_Error_Model>(Get_detail_error_query, param, ct: ct));
            // Trả về danh sách số lượng lỗi trong các lần nhập
            return rows.ToList();
        }
    }
}
