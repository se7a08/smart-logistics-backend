using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;

namespace SmartLogistics.Infrastructure.Data.Seeding
{
    public static class ShipmentSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Shipments.AnyAsync()) return;

            var warehouses = await context.Warehouses.ToListAsync();
            var driver = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Driver);
            if (warehouses.Count < 2 || driver is null) return;

            var rnd = new Random(42);
            var now = DateTime.UtcNow;

            string[] names = { "Mohamed Ali", "Sara Ibrahim", "Omar Khaled", "Nour Hassan", "Youssef Adel",
                               "Mariam Samir", "Karim Fathy", "Hana Mostafa", "Ziad Tarek", "Laila Mahmoud" };
            string[] streets = { "Corniche El Nile St", "Taha Hussein St", "El Galaa St", "Port Said St", "El Hurriya St" };

            // 25 shipments: status + days ago + hours until/after ETA
            var plan = new (ShipmentStatus Status, int DaysAgo, int EtaOffsetHours)[]
            {
                (ShipmentStatus.Pending, 0, 48), (ShipmentStatus.Pending, 0, 36), (ShipmentStatus.Pending, 1, 24),
                (ShipmentStatus.Pending, 3, -20),                                   // delayed
                (ShipmentStatus.PickedUp, 0, 30), (ShipmentStatus.PickedUp, 1, 12), (ShipmentStatus.PickedUp, 2, -10), // delayed
                (ShipmentStatus.InTransit, 0, 20), (ShipmentStatus.InTransit, 1, 8), (ShipmentStatus.InTransit, 2, 5),
                (ShipmentStatus.InTransit, 4, -30),                                 // delayed
                (ShipmentStatus.Delivered, 1, 0), (ShipmentStatus.Delivered, 2, 0), (ShipmentStatus.Delivered, 3, 0),
                (ShipmentStatus.Delivered, 4, 0), (ShipmentStatus.Delivered, 5, 0), (ShipmentStatus.Delivered, 6, 0),
                (ShipmentStatus.Delivered, 8, 0), (ShipmentStatus.Delivered, 10, 0), (ShipmentStatus.Delivered, 12, 0),
                (ShipmentStatus.Delivered, 15, 0),
                (ShipmentStatus.Cancelled, 2, 0), (ShipmentStatus.Cancelled, 5, 0),
                (ShipmentStatus.Cancelled, 9, 0), (ShipmentStatus.Cancelled, 14, 0)
            };

            var shipments = new List<Shipment>();
            var histories = new List<ShipmentStatusHistory>();
            var createdAtFix = new List<(Guid Id, DateTime At)>();
            var historyFix = new List<(Guid Id, DateTime At)>();

            for (int i = 0; i < plan.Length; i++)
            {
                var (status, daysAgo, etaOffset) = plan[i];
                var created = now.AddDays(-daysAgo).AddHours(-rnd.Next(0, 5));
                var origin = warehouses[i % 2];
                var destination = warehouses[(i + 1) % 2];
                var assigned = status is ShipmentStatus.PickedUp or ShipmentStatus.InTransit or ShipmentStatus.Delivered;
                var tracking = $"SL-2026-{i + 1:0000}";

                var shipment = new Shipment
                {
                    TrackingNumber = tracking,
                    QrCode = $"QR-{tracking}-{Guid.NewGuid():N}"[..40],
                    RecipientName = names[i % names.Length],
                    RecipientPhone = $"0100{rnd.Next(1000000, 9999999)}",
                    RecipientEmail = $"customer{i + 1}@example.com",
                    DeliveryAddress = $"{rnd.Next(1, 90)} {streets[i % streets.Length]}, {destination.City}",
                    DeliveryLatitude = 28.10 + rnd.NextDouble() * 0.05,
                    DeliveryLongitude = 30.74 + rnd.NextDouble() * 0.05,
                    Weight = Math.Round((decimal)(rnd.NextDouble() * 20 + 0.5), 3),
                    Description = i % 3 == 0 ? "Electronics" : i % 3 == 1 ? "Clothing" : "Documents",
                    DeclaredValue = rnd.Next(100, 5000),
                    IsFragile = i % 4 == 0,
                    Status = status,
                    EstimatedDelivery = status == ShipmentStatus.Delivered
                        ? created.AddHours(30)
                        : now.AddHours(etaOffset),
                    PickedUpAt = status is ShipmentStatus.Pending or ShipmentStatus.Cancelled ? null : created.AddHours(4),
                    DeliveredAt = status == ShipmentStatus.Delivered ? created.AddHours(26) : null,
                    QrVerified = status == ShipmentStatus.Delivered,
                    DriverId = assigned ? driver.Id : null,
                    OriginWarehouseId = origin.Id,
                    DestinationWarehouseId = destination.Id
                };
                shipments.Add(shipment);
                createdAtFix.Add((shipment.Id, created));

                // Status history up to the current status
                var steps = status switch
                {
                    ShipmentStatus.Pending => new[] { ShipmentStatus.Pending },
                    ShipmentStatus.PickedUp => new[] { ShipmentStatus.Pending, ShipmentStatus.PickedUp },
                    ShipmentStatus.InTransit => new[] { ShipmentStatus.Pending, ShipmentStatus.PickedUp, ShipmentStatus.InTransit },
                    ShipmentStatus.Delivered => new[] { ShipmentStatus.Pending, ShipmentStatus.PickedUp, ShipmentStatus.InTransit, ShipmentStatus.Delivered },
                    _ => new[] { ShipmentStatus.Pending, ShipmentStatus.Cancelled }
                };

                for (int s = 0; s < steps.Length; s++)
                {
                    var h = new ShipmentStatusHistory
                    {
                        ShipmentId = shipment.Id,
                        Status = steps[s],
                        Notes = steps[s] switch
                        {
                            ShipmentStatus.Pending => "Shipment created",
                            ShipmentStatus.PickedUp => "Picked up by driver",
                            ShipmentStatus.InTransit => "On the way to destination",
                            ShipmentStatus.Delivered => "Delivered, QR verified",
                            _ => "Cancelled by admin"
                        }
                    };
                    histories.Add(h);
                    historyFix.Add((h.Id, created.AddHours(s * 8)));
                }
            }

            context.Shipments.AddRange(shipments);
            context.ShipmentStatusHistories.AddRange(histories);
            await context.SaveChangesAsync();

            // SaveChangesAsync overwrites CreatedAt on insert, so backdate directly in SQL.
            foreach (var (id, at) in createdAtFix)
                await context.Shipments.IgnoreQueryFilters().Where(x => x.Id == id)
                    .ExecuteUpdateAsync(x => x.SetProperty(p => p.CreatedAt, at));

            foreach (var (id, at) in historyFix)
                await context.ShipmentStatusHistories.IgnoreQueryFilters().Where(x => x.Id == id)
                    .ExecuteUpdateAsync(x => x.SetProperty(p => p.CreatedAt, at));
        }
    }
}
