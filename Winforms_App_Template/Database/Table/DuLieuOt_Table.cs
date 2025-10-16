using Dapper;
using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    public class DuLieuOt_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public DuLieuOt_Table(DbExecutor db) => _db = db; // Contructor lấy db

        // Whitelist các cột được phép ORDER BY để chống SQL injection ở dynamic ORDER BY
        private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
        {
            "WorkDateRoot", "CodeEmp", "RegisterHours", "InTime", "OutTime", "ProfileName", "BoPhan", "Status"
        };

        public async Task<PageResult<DuLieuOt_Model>> QueryPageAsync(PageRequest req, CancellationToken ct = default)
        {
            // Chuẩn hoá tham số trang (tránh âm/0 và tránh page-size quá lớn)
            var page = Math.Max(1, req.Page);                                       // đảm bảo >= 1
            var size = Math.Clamp(req.Size, 1, 500);                                // giới hạn size tối đa
            var offset = (page - 1) * size;                                         // OFFSET

            // Xác định cột sắp xếp an toàn
            var sortBy = AllowedSort.Contains(req.SortBy ?? "") ? req.SortBy! : "WorkDateRoot";
            var sortDir = req.Desc ? "DESC" : "ASC";

            // Xây WHERE động theo filter – mọi giá trị đều đi qua parameters (không ghép chuỗi trị liệu)
            var where = new StringBuilder("WHERE 1=1");
            if (!string.IsNullOrWhiteSpace(req.CodeEmp))
                where.AppendLine(" AND CodeEmp = @CodeEmp");
            if (req.FromDate.HasValue)
                where.AppendLine(" AND WorkDateRoot >= @FromDate");
            if (req.ToDate.HasValue)
                where.AppendLine(" AND WorkDateRoot < @ToDatePlusOne"); // dùng < ngày+1 để inclusive

            // Câu SELECT phân trang
            var sql = $@"
                SELECT
                    CodeEmp, ProfileName, BoPhan, WorkDateRoot, InTime, OutTime, RegisterHours, OvertimeTypeName, Status
                FROM dbo.DuLieuOT
                {where}
                ORDER BY {sortBy} {sortDir}
                OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;";

            // Câu COUNT tổng (có thể tắt nếu không cần)
            var sqlCount = $@"
                SELECT COUNT_BIG(1)
                FROM dbo.DuLieuOT
                {where};";

            // Đóng gói tham số – chỉ là dữ liệu; ORDER BY đã whitelist sẵn ở trên
            var param = new
            {
                req.CodeEmp,
                req.FromDate,
                ToDatePlusOne = req.ToDate?.Date.AddDays(1), // inclusive
                Offset = offset,
                Size = size
            };

            // Thực thi
            var rows = (await _db.QueryAsync<DuLieuOt_Model>(sql, param, ct: ct)).ToList();
            var total = (await _db.QueryAsync<long>(sqlCount, param, ct: ct)).Single(); // lấy đúng 1 giá trị

            return new PageResult<DuLieuOt_Model>
            {
                Items = rows,
                Page = page,
                Size = size,
                Total = total
            };
        }
    }
}
