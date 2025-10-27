// Database/Table/ReportLayoutVersions_Table.cs
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    /// <summary>
    /// Repo thao tác với lịch sử layout (append-only):
    ///  - AddVersionAsync   : INSERT bản ghi mới, trả về Version mới
    ///  - GetLatestAsync    : lấy bản mới nhất
    ///  - GetByVersionAsync : lấy đúng version chỉ định
    ///  - GetLatestVersionAsync : lấy số version mới nhất
    /// </summary>
    internal sealed class ReportLayoutVersions_Table
    {
        private readonly DbExecutor _db;
        public ReportLayoutVersions_Table(DbExecutor db) => _db = db;

        /// <summary>
        /// Thêm một phiên bản mới (gọi SP dbo.ReportLayout_AddVersion), trả về Version.
        /// </summary>
        public async Task<int> AddVersionAsync(
            string reportName,
            string updatedBy,
            byte[] contentGz,
            byte[] sha256,
            int commandTimeoutSeconds = 30,
            CancellationToken ct = default)
        {
            // SP phải kết thúc bằng:  SELECT @NewVersion AS Version;
            var version = await _db.ExecuteScalarAsync<int>(
                sql: "dbo.ReportLayout_AddVersion",
                param: new { ReportName = reportName, UpdatedBy = updatedBy, ContentGz = contentGz, Sha256 = sha256 },
                commandType: CommandType.StoredProcedure,
                ct: ct).ConfigureAwait(false);

            return version;
        }

        /// <summary>
        /// Lấy bản MỚI NHẤT của 1 report (gọi SP dbo.ReportLayout_GetLatest).
        /// Trả null nếu chưa có bản nào.
        /// </summary>
        public async Task<ReportLayoutVersion_Model?> GetLatestAsync(
            string reportName,
            CancellationToken ct = default)
        {
            var row = await _db.QuerySingleOrDefaultAsync<ReportLayoutVersion_Model>(
                sql: "dbo.ReportLayout_GetLatest",
                param: new { ReportName = reportName },
                commandType: CommandType.StoredProcedure,
                ct: ct).ConfigureAwait(false);

            return row;
        }

        /// <summary>
        /// Lấy đúng 1 phiên bản theo số Version (nếu cần khôi phục).
        /// </summary>
        public async Task<ReportLayoutVersion_Model?> GetByVersionAsync(
            string reportName,
            int version,
            int commandTimeoutSeconds = 30,
            CancellationToken ct = default)
        {
            var row = await _db.QuerySingleOrDefaultAsync<ReportLayoutVersion_Model>(
                sql: "dbo.ReportLayout_Get",
                param: new { ReportName = reportName, Version = version },
                commandType: CommandType.StoredProcedure,
                ct: ct).ConfigureAwait(false);

            return row;
        }

        /// <summary>
        /// Lấy số Version mới nhất (để polling/hiển thị).
        /// </summary>
        public async Task<int?> GetLatestVersionAsync(
            string reportName,
            int commandTimeoutSeconds = 15,
            CancellationToken ct = default)
        {
            var ver = await _db.ExecuteScalarAsync<int?>(
                sql: "dbo.ReportLayout_GetLatestVersion",
                param: new { ReportName = reportName },
                commandType: CommandType.StoredProcedure,
                ct: ct).ConfigureAwait(false);

            return ver;
        }
    }
}
