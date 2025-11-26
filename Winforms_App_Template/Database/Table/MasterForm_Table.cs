using DevExpress.CodeParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    internal class MasterForm_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public MasterForm_Table(DbExecutor db) => _db = db; // Contructor lấy db

        /// <summary>
        /// Truy vấn danh sách các công đoạn có ver là lớn nhất cho từng công đoạn
        /// </summary>
        /// <param name="IdCongDoan"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<List<MasterForm_Model>> Get_Latest_MasterForm_Main(IEnumerable<int> IdCongDoan, CancellationToken ct = default)
        {
            var sql = @"
                SELECT *
                FROM (
                    SELECT 
                        mf.*,
                        ROW_NUMBER() OVER (PARTITION BY mf.idCongDoan ORDER BY mf.Ver DESC) AS rn
                    FROM [MESPlus].[dbo].[tblMasterForm] AS mf
                    WHERE mf.idCongDoan IN @IdCongDoan
                ) AS t
                WHERE t.rn = 1
                ORDER BY t.idCongDoan;
            ";
            var param = new
            {
                IdCongDoan
            };

            // Thực thi
            var rows = (await _db.QueryAsync<MasterForm_Model>(sql, param, ct: ct));

            return rows.ToList();
        }

        public async Task<List<MasterForm_Model>> Get_Latest_MasterForm_DKM(IEnumerable<int> IdCongDoan, CancellationToken ct = default)
        {
            var sql = @"
                SELECT *
                FROM (
                    SELECT 
                        mf.*,
                        ROW_NUMBER() OVER (PARTITION BY mf.idCongDoan ORDER BY mf.Ver DESC) AS rn
                    FROM [MESPlus].[dbo].[tblMasterForm] AS mf
                    WHERE 
                        mf.idCongDoan IN @IdCongDoan
                        AND mf.isDieuKienMay = 1
                ) AS t
                WHERE t.rn = 1
                ORDER BY t.idCongDoan;
            ";
            var param = new
            {
                IdCongDoan
            };

            // Thực thi
            var rows = (await _db.QueryAsync<MasterForm_Model>(sql, param, ct: ct));

            return rows.ToList();
        }

        public async Task<MasterForm_Model?> Get_MasterForm_ByVer(int IdCongDoan, int Ver, CancellationToken ct = default)
        {
            var sql = @"
                SELECT *
                FROM [MESPlus].[dbo].[tblMasterForm]
                WHERE idCongDoan = @IdCongDoan AND Ver = @Ver;
            ";
            var param = new
            {
                IdCongDoan,
                Ver
            };

            var result = (await _db.QueryAsync<MasterForm_Model>(sql, param, ct:ct));
            return result.FirstOrDefault();
        }
    }
}
