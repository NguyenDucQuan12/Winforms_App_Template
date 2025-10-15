using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Winforms_App_Template.Database.Entities;

namespace Winforms_App_Template.Database
{
    public class AppDbContext : DbContext
    {
        /// DbSet = tập đối tượng để LINQ query và SaveChanges
        public DbSet<ThongTinNhanVien> ThongTinNhanVien => Set<ThongTinNhanVien>();
        public DbSet<DuLieuOt> DuLieuOT => Set<DuLieuOt>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ======== ÁNH XẠ CHI TIẾT (FLUENT API) ========
            // ===== ThongTinNhanVien =====
            var nv = mb.Entity<ThongTinNhanVien>();
            nv.ToTable("ThongTinNhanVien", "dbo");  // map tên bảng + schema
            nv.HasKey(x => x.CodeEmp);              // PK = CodeEmp

            nv.Property(x => x.CodeEmp)
              .HasColumnName("CodeEmp")
              .HasMaxLength(50)
              .IsRequired();

            nv.Property(x => x.ProfileName)
              .HasColumnName("ProfileName")
              .HasMaxLength(200);

            nv.Property(x => x.Gender)
              .HasColumnName("Gender")
              .HasMaxLength(20);

            nv.Property(x => x.DateHire)
              .HasColumnName("DateHire")
              .HasColumnType("date");

            nv.Property(x => x.BoPhan)
              .HasColumnName("BoPhan")
              .HasMaxLength(100);

            nv.Property(x => x.DateQuit)
              .HasColumnName("DateQuit")
              .HasColumnType("date");

            nv.Property(x => x.StatusSyn)
              .HasColumnName("StatusSyn")
              .HasMaxLength(50);

            // Chỉ mục truy vấn thường dùng (ví dụ theo bộ phận, trạng thái)
            nv.HasIndex(x => new { x.BoPhan, x.StatusSyn });

            // ===== DuLieuOT =====
            var ot = mb.Entity<DuLieuOt>();
            ot.ToTable("DuLieuOT", "dbo");
            // Khóa ghép
            ot.HasKey(x => new { x.CodeEmp, x.WorkDateRoot });

            ot.Property(x => x.CodeEmp)
                .HasColumnName("CodeEmp")
                .HasMaxLength(50)
                .IsRequired();

            ot.Property(x => x.WorkDateRoot)
                .HasColumnName("WorkDateRoot")
                .HasColumnType("date")
                .IsRequired();

            ot.Property(x => x.ProfileName)
              .HasColumnName("ProfileName")
              .HasMaxLength(200);

            ot.Property(x => x.BoPhan)
              .HasColumnName("BoPhan")
              .HasMaxLength(100);

            ot.Property(x => x.InTime)
              .HasColumnName("InTime")
              .HasColumnType("time");

            ot.Property(x => x.OutTime)
              .HasColumnName("OutTime")
              .HasColumnType("time");

            ot.Property(x => x.RegisterHours)
              .HasColumnName("RegisterHours")
              .HasColumnType("decimal(10,2)");

            ot.Property(x => x.OvertimeTypeName)
              .HasColumnName("OvertimeTypeName")
              .HasMaxLength(100);

            ot.Property(x => x.Status)
              .HasColumnName("Status")
              .HasMaxLength(50);

            // Chỉ mục phụ trợ truy vấn lọc nhanh theo ngày + bộ phận
            ot.HasIndex(x => new { x.WorkDateRoot, x.BoPhan });
        }
    }
}
