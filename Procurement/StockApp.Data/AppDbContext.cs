using Microsoft.EntityFrameworkCore;
using StockApp.Data.Entities;

namespace StockApp.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<UserAuth> UserAuths => Set<UserAuth>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserAuth>(e =>
            {
                e.ToTable("UserAuth");
                e.Property(x => x.UserName).HasMaxLength(100);
                e.Property(x => x.PasswordHash).HasMaxLength(500);
                e.HasIndex(x => x.UserName).IsUnique();
                e.HasIndex(x => x.EmployeeId).IsUnique();   // 1 karyawan = 1 akun
                e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<LoginLog>(e =>
            {
                e.Property(x => x.UserName).HasMaxLength(256);
                e.Property(x => x.FailReason).HasMaxLength(100);
                e.Property(x => x.IpAddress).HasMaxLength(64);
                e.Property(x => x.UserAgent).HasMaxLength(512);
                e.HasIndex(x => x.CreatedAt);
            });

            builder.Entity<Employee>(e =>
            {
                e.Property(x => x.Nik).HasMaxLength(30);
                e.Property(x => x.Name).HasMaxLength(150);
                e.Property(x => x.Email).HasMaxLength(150);
                e.Property(x => x.Position).HasMaxLength(100);
                e.Property(x => x.Warehouse).HasMaxLength(100);
                e.Property(x => x.Role).HasMaxLength(50);
                e.HasIndex(x => x.Nik).IsUnique();
            });
        }

        // Mengisi CreatedAt / UpdatedAt otomatis untuk semua data master.
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
                if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = now;
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}