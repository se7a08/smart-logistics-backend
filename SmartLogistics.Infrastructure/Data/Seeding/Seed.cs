using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;


namespace SmartLogistics.Infrastructure.Data.Seeding
{
    /// <summary>
    /// Seeds the database with initial Admin and Driver users,
    /// sample warehouses, and sample shipments for development/testing.
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Users.AnyAsync()) return; // Already seeded

            // ── Seed Users ──────────────────────────────────────────────────────

            var admin = new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                FullName = "System Administrator",
                Email = "admin@smartlogistics.com",
                PasswordHash = hasher.Hash("Admin@123"),
                PhoneNumber = "+201000000001",
                Role = UserRole.Admin,
                IsActive = true
            };

            var driver1 = new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                FullName = "Ahmed Hassan",
                Email = "ahmed.driver@smartlogistics.com",
                PasswordHash = hasher.Hash("Driver@123"),
                PhoneNumber = "+201000000002",
                Role = UserRole.Driver,
                LicenseNumber = "DL-123456",
                VehiclePlate = "ABC-1234",
                IsActive = true
            };

            var driver2 = new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                FullName = "Mohamed Ali",
                Email = "mohamed.driver@smartlogistics.com",
                PasswordHash = hasher.Hash("Driver@123"),
                PhoneNumber = "+201000000003",
                Role = UserRole.Driver,
                LicenseNumber = "DL-789012",
                VehiclePlate = "XYZ-5678",
                IsActive = true
            };

            context.Users.AddRange(admin, driver1, driver2);

            // ── Seed Warehouses ─────────────────────────────────────────────────

            var warehouseCairo = new Warehouse
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000001"),
                Name = "Cairo Central Warehouse",
                Code = "CAI-CENTRAL",
                Address = "10 Industrial Zone, Nasr City",
                City = "Cairo",
                Country = "Egypt",
                Latitude = 30.0626,
                Longitude = 31.2497,
                Capacity = 5000,
                ManagerName = "Omar Khalil",
                ManagerPhone = "+201111111111",
                IsActive = true
            };

            var warehouseAlex = new Warehouse
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000002"),
                Name = "Alexandria Port Warehouse",
                Code = "ALEX-PORT",
                Address = "5 Port Said Street, Bab Sharqi",
                City = "Alexandria",
                Country = "Egypt",
                Latitude = 31.2001,
                Longitude = 29.9187,
                Capacity = 3000,
                ManagerName = "Sara Nader",
                ManagerPhone = "+201222222222",
                IsActive = true
            };

            context.Warehouses.AddRange(warehouseCairo, warehouseAlex);

            await context.SaveChangesAsync();
        }
    }
}
