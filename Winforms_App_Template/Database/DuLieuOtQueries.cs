using Microsoft.EntityFrameworkCore;
using Winforms_App_Template.Database.Entities;

namespace Winforms_App_Template.Database
{
    /// <summary>
    /// Các hàm truy vấn cho DuLieuOT (PK = CodeEmp).
    /// Chú ý: Với PK như vậy, mỗi nhân viên chỉ có 1 record OT (mô hình tổng).
    /// </summary>
    public static class DuLieuOtQueries
    {
        /// <summary>
        /// Lấy 1 OT theo CodeEmp (PK).
        /// </summary>
        public static Task<DuLieuOt?> GetByCodeAsync(AppDbContext db, string codeEmp, CancellationToken ct = default)
            => db.DuLieuOT.FindAsync(new object[] { codeEmp }, ct).AsTask();

        /// <summary>
        /// Phân trang + tìm kiếm + lọc khoảng ngày WorkDateRoot.
        /// </summary>
        public static async Task<(int total, List<DuLieuOt> items)> GetPageAsync(
            AppDbContext db,
            string? keyword,
            string? boPhan,
            DateTime? fromDate,
            DateTime? toDate,
            int page, int pageSize,
            CancellationToken ct = default)
        {
            IQueryable<DuLieuOt> q = db.DuLieuOT.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                q = q.Where(x => x.CodeEmp.Contains(k) || x.ProfileName.Contains(k) || x.BoPhan.Contains(k) || x.OvertimeTypeName.Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(boPhan))
                q = q.Where(x => x.BoPhan == boPhan);

            if (fromDate.HasValue) q = q.Where(x => x.WorkDateRoot >= fromDate.Value.Date);
            if (toDate.HasValue) q = q.Where(x => x.WorkDateRoot <= toDate.Value.Date);

            q = q.OrderByDescending(x => x.WorkDateRoot).ThenBy(x => x.CodeEmp);

            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (total, items);
        }

        /// <summary>
        /// Upsert theo CodeEmp.
        /// </summary>
        public static async Task UpsertAsync(AppDbContext db, DuLieuOt model, CancellationToken ct = default)
        {
            var existing = await db.DuLieuOT.FindAsync(model.CodeEmp, model.WorkDateRoot, ct);
            if (existing == null)
            {
                await db.DuLieuOT.AddAsync(model, ct);
            }
            else
            {
                // Cập nhật các trường
                existing.ProfileName = model.ProfileName;
                existing.BoPhan = model.BoPhan;
                existing.WorkDateRoot = model.WorkDateRoot;
                existing.InTime = model.InTime;
                existing.OutTime = model.OutTime;
                existing.RegisterHours = model.RegisterHours;
                existing.OvertimeTypeName = model.OvertimeTypeName;
                existing.Status = model.Status;
            }
            await db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Cập nhật trạng thái OT theo CodeEmp.
        /// </summary>
        public static async Task<bool> UpdateStatusAsync(AppDbContext db, string codeEmp, string newStatus, CancellationToken ct = default)
        {
            var x = await db.DuLieuOT.FindAsync(new object[] { codeEmp }, ct);
            if (x == null) return false;
            x.Status = newStatus;
            await db.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Xóa OT theo CodeEmp.
        /// </summary>
        public static async Task<bool> DeleteAsync(AppDbContext db, string codeEmp, CancellationToken ct = default)
        {
            var x = await db.DuLieuOT.FindAsync(new object[] { codeEmp }, ct);
            if (x == null) return false;
            db.DuLieuOT.Remove(x);
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
