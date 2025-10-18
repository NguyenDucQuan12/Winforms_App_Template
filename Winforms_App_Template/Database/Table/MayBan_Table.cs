using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{

    internal class MayBan_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public MayBan_Table(DbExecutor db) => _db = db; // Contructor lấy db

        public async Task<List<MayBan_Model>> LoadMayBanAsync(
            int? idCongDoan = null, CancellationToken ct = default)
        {
            var sql = @"
            SELECT
                 idMay_Ban   AS IdMayBan,
                 idCongDoan  AS IdCongDoan,
                 TenMay_Ban  AS TenMayBan,
                 MaMay_Ban   AS MaMayBan,
                 MoTa        AS MoTa,
                 Active      AS Active,
                 PLC         AS PLC
            FROM dbo.tblMay_Ban
            WHERE (@idCongDoan IS NULL OR idCongDoan = @idCongDoan)
            ORDER BY TenMay_Ban;";

            return (await _db.QueryAsync<MayBan_Model>(sql, new { idCongDoan }, ct: ct)).ToList();
         }
    }
}
