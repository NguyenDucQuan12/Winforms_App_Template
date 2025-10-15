using Microsoft.EntityFrameworkCore;
using Winforms_App_Template.Database.Entities;

namespace Winforms_App_Template.Database
{
    /// <summary>
    /// Các hàm truy vấn cho bảng ThongTinNhanVien (PK = CodeEmp).
    /// </summary>
    public static class ThongTinNhanVienQueries
    {
        /// <summary>
        /// Lấy 1 nhân viên theo CodeEmp (PK).
        /// </summary>
        public static Task<ThongTinNhanVien?> GetByCodeAsync(AppDbContext db, string codeEmp, CancellationToken ct = default)
            => db.ThongTinNhanVien.FindAsync(new object[] { codeEmp }, ct).AsTask();

        /// <summary>
        /// Phân trang + tìm kiếm (theo code/name/department), lọc trạng thái, khoảng ngày vào làm.
        /// </summary>
        public static async Task<(int total, List<ThongTinNhanVien> items)> GetPageAsync(
            AppDbContext db,
            string? keyword,
            string? boPhan,
            string? statusSyn,
            DateTime? hireFrom,
            DateTime? hireTo,
            int page, int pageSize,
            CancellationToken ct = default)
        {
            IQueryable<ThongTinNhanVien> q = db.ThongTinNhanVien.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                q = q.Where(x => x.CodeEmp.Contains(k) || x.ProfileName.Contains(k) || x.BoPhan.Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(boPhan))
                q = q.Where(x => x.BoPhan == boPhan);

            if (!string.IsNullOrWhiteSpace(statusSyn))
                q = q.Where(x => x.StatusSyn == statusSyn);

            if (hireFrom.HasValue) q = q.Where(x => x.DateHire >= hireFrom.Value.Date);
            if (hireTo.HasValue) q = q.Where(x => x.DateHire <= hireTo.Value.Date);

            q = q.OrderBy(x => x.ProfileName).ThenBy(x => x.CodeEmp);

            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (total, items);
        }

        /// <summary>
        /// Upsert theo PK CodeEmp. Nếu có -> update; không có -> insert.
        /// </summary>
        public static async Task UpsertAsync(AppDbContext db, ThongTinNhanVien model, CancellationToken ct = default)
        {
            // Tìm nhanh theo PK
            var existing = await db.ThongTinNhanVien.FindAsync(new object[] { model.CodeEmp }, ct);
            if (existing == null)
            {
                await db.ThongTinNhanVien.AddAsync(model, ct); // Thêm mới
            }
            else
            {
                // Cập nhật từng trường (tránh overwrite null không mong muốn nếu cần)
                existing.ProfileName = model.ProfileName;
                existing.Gender = model.Gender;
                existing.DateHire = model.DateHire;
                existing.BoPhan = model.BoPhan;
                existing.DateQuit = model.DateQuit;
                existing.StatusSyn = model.StatusSyn;
            }
            await db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Xóa theo PK CodeEmp (hard delete). Nếu bạn muốn soft-delete, thêm cột IsDeleted và filter.
        /// </summary>
        public static async Task<bool> DeleteAsync(AppDbContext db, string codeEmp, CancellationToken ct = default)
        {
            var x = await db.ThongTinNhanVien.FindAsync(new object[] { codeEmp }, ct);
            if (x == null) return false;
            db.ThongTinNhanVien.Remove(x);
            await db.SaveChangesAsync(ct);
            return true;
        }
    }
}
