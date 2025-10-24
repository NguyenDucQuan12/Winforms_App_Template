using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    internal class Standard_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public Standard_Table(DbExecutor db) => _db = db; // Contructor lấy db

        public async Task<List<Standard_Model>> Get_Detail_Standard(IEnumerable<int> idInputs, CancellationToken ct = default)
        {

            // Kiểm tra dữ liệu đầu vào: nếu mảng rỗng → trả rỗng (tránh SQL IN ())
            var ids = idInputs?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0) return new List<Standard_Model>();

            var Get_detail_standard_query = @"
                SELECT
                    ip.idInput,
                    ip.idStandard,
                    ip.MaTieuChuan,
                    st.TenTieuChuan,
                    st.Loai_size,
                    st.Loai_kytu,
                    st.Loai_chieudai,
                    st.Loai_somay,
                    st.Loai_masp,
                    st.Loai_ten,
                    st.TCMin,
                    st.TCMax
                FROM 
                    tblInput_Standard AS ip
                LEFT JOIN 
                    tblTieuchuan AS st  ON ip.idStandard = st.id
                WHERE 
                    ip.idInput IN @idInputs
                ORDER BY 
                    ip.idStandard; 
                ";
            var param = new
            {
                idInputs
            };

            // Thực thi
            var rows = (await _db.QueryAsync<Standard_Model>(Get_detail_standard_query, param, ct: ct));
            // Trả về danh sách số lượng lỗi trong các lần nhập
            return rows.ToList();
        }
    }
}
