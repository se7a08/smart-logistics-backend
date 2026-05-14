using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;


namespace SmartLogistics.Infrastructure.Data.Seeding
{
   
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher)
        {
            if (await context.Users.AnyAsync()) return;

            var admin = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Seha Admin",
                Email = "seha_admin@logistics.com",
                PasswordHash = hasher.Hash("123456"),
                PhoneNumber = "01023456789",
                Role = UserRole.Admin,
                IsActive = true
            };

            var driver1 = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Ahmed Hassan",
                Email = "ahmed.h@yahoo.com", 
                PasswordHash = hasher.Hash("pass123"),
                PhoneNumber = "01122334455",
                Role = UserRole.Driver,
                LicenseNumber = "DL-12345",
                VehiclePlate = "ن ص ج 123", 
                IsActive = true
            };

            context.Users.AddRange(admin, driver1);

            // إضافة مخازن في محافظة المنيا
            var warehouse1 = new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = "مخزن المنيا الرئيسي",
                Code = "MIN-MAIN-01",
                Address = "المنيا - شارع طه حسين",
                City = "Minya",
                Capacity = 1000,
                ManagerName = "Fady Zaki",
                IsActive = true
            };

            var warehouse2 = new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = "فرع ملوي",
                Code = "MAL-BRANCH-02", 
                Address = "ملوي - خلف المحطة",
                City = "Malwi",
                Capacity = 1500,
                ManagerName = "Mina Salah",
                IsActive = true
            };

            context.Warehouses.AddRange(warehouse1, warehouse2);

            
            await context.SaveChangesAsync();
        }
    }
}
