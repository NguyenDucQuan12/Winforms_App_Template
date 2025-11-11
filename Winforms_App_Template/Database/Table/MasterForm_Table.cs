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

        public async Task<MasterForm_Model?> Get_Latest_MasterForm(int IdCongDoan, CancellationToken ct = default)
        {
            var sql = @"
                SELECT TOP 1 *
                FROM [MESPlus].[dbo].[tblMasterForm]
                WHERE idCongDoan = @IdCongDoan
                ORDER BY Ver DESC;
            ";
            var param = new
            {
                IdCongDoan
            };

            // Thực thi
            var rows = (await _db.QueryAsync<MasterForm_Model>(sql, param, ct: ct));

            return rows.FirstOrDefault();
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
