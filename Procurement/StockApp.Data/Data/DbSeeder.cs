using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockApp.Data;
using StockApp.Data.Entities;

namespace StockApp.Api.Data
{
    public static class DbSeeder
    {
        public static readonly string[] Roles =
            { "Administrator", "Admin Gudang", "Staf Gudang", "Pembelian", "Owner" };

        public static async Task SeedAsync(IServiceProvider sp, IConfiguration config)
        {
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in Roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            var users = sp.GetRequiredService<UserManager<AppUser>>();
            if (await users.FindByNameAsync("admin") is null)
            {
                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@stockapp.local",
                    EmailConfirmed = true,
                    FullName = "Administrator",
                };

                var result = await users.CreateAsync(admin, config["Seed:AdminPassword"] ?? "Admin123!");
                if (result.Succeeded)
                    await users.AddToRoleAsync(admin, "Administrator");
            }

            await SeedEmployeesAsync(sp.GetRequiredService<AppDbContext>());
        }

        private static async Task SeedEmployeesAsync(AppDbContext db)
        {
            if (await db.Employees.AnyAsync()) return;   // hanya saat tabel masih kosong

            db.Employees.AddRange(
                new Employee { Nik = "EMP-001", Name = "Ian Keefe", Email = "ian@perusahaan.co.id", Position = "IT Manager", Warehouse = "Gudang Utama", Role = "Administrator" },
                new Employee { Nik = "EMP-002", Name = "Budi Santoso", Email = "budi@perusahaan.co.id", Position = "Staf Gudang", Warehouse = "Gudang Utama", Role = "Staf Gudang" },
                new Employee { Nik = "EMP-003", Name = "Sari Wulandari", Email = "sari@perusahaan.co.id", Position = "Staf Pembelian", Warehouse = "Gudang B", Role = "Pembelian" },
                new Employee { Nik = "EMP-004", Name = "Andi Pratama", Email = "andi@perusahaan.co.id", Position = "Kepala Gudang", Warehouse = "Gudang B", Role = "Admin Gudang" });

            await db.SaveChangesAsync();
        }
    }
}