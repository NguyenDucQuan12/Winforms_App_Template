using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Winforms_App_Template.Database
{
    /// <summary>
    /// Cho phép 'dotnet ef migrations ...' tạo AppDbContext khi chạy bên ngoài app.
    /// Tại đây ta chỉ rõ: "config nằm ở Database/appsettings.json".
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        // Hàm này sẽ được EF Core gọi khi cần tạo DbContext ở design-time (migration, update-database)
        public AppDbContext CreateDbContext(string[] args)
        {
            // Lấy thư mục hiện tại (thường là thư mục project)
            var cfg = new ConfigurationBuilder()
                // Đặt base path là thư mục hiện tại
                .SetBasePath(Directory.GetCurrentDirectory())
                // Nạp file cấu hình JSON (chứa chuỗi kết nối)
                .AddJsonFile(path: "Database/appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // Lấy chuỗi kết nối tên "SqlServer" từ file cấu hình
            var cs = cfg.GetConnectionString("SqlServer");

            // Tạo options cho DbContext, cấu hình dùng SQL Server
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(cs, sql =>
                {
                    // Cho phép tự động retry khi mất kết nối tạm thời
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
                })
                .Options;

            // Trả về instance AppDbContext đã cấu hình
            return new AppDbContext(options);
        }
    }
}