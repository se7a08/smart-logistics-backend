using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::SmartLogistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLogistics.Domain.Entities;

namespace SmartLogistics.Infrastructure.Data.Configurations
{
    
   
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.LicenseNumber).HasMaxLength(50);
            builder.Property(u => u.VehiclePlate).HasMaxLength(20);
            builder.Property(u => u.FcmToken).HasMaxLength(500);

            builder.HasMany(u => u.RefreshTokens).WithOne(t => t.User).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Locations).WithOne(l => l.Driver).HasForeignKey(l => l.DriverId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Notifications).WithOne(n => n.User).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.HasKey(w => w.Id);
            builder.Property(w => w.Name).HasMaxLength(200).IsRequired();
            builder.Property(w => w.Code).HasMaxLength(20).IsRequired();
            builder.HasIndex(w => w.Code).IsUnique();
            builder.Property(w => w.Address).HasMaxLength(500).IsRequired();
            builder.Property(w => w.City).HasMaxLength(100).IsRequired();
            builder.Property(w => w.Country).HasMaxLength(100).IsRequired();
            builder.Property(w => w.ManagerName).HasMaxLength(100).IsRequired();
            builder.Property(w => w.ManagerPhone).HasMaxLength(20).IsRequired();
        }
    }

    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.TrackingNumber).HasMaxLength(50).IsRequired();
            builder.HasIndex(s => s.TrackingNumber).IsUnique();
            builder.Property(s => s.QrCode).HasMaxLength(500).IsRequired();
            builder.Property(s => s.RecipientName).HasMaxLength(100).IsRequired();
            builder.Property(s => s.RecipientPhone).HasMaxLength(20).IsRequired();
            builder.Property(s => s.RecipientEmail).HasMaxLength(256).IsRequired();
            builder.Property(s => s.DeliveryAddress).HasMaxLength(500).IsRequired();
            builder.Property(s => s.Description).HasMaxLength(500).IsRequired();
            builder.Property(s => s.Weight).HasColumnType("decimal(10,3)");
            builder.Property(s => s.DeclaredValue).HasColumnType("decimal(18,2)");
            builder.Property(s => s.DeliveryNotes).HasMaxLength(1000);
            builder.Property(s => s.DeliveryPhotoUrl).HasMaxLength(500);

            builder.HasOne(s => s.Driver).WithMany(u => u.AssignedShipments)
                .HasForeignKey(s => s.DriverId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);

            builder.HasOne(s => s.OriginWarehouse).WithMany(w => w.OriginShipments)
                .HasForeignKey(s => s.OriginWarehouseId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.DestinationWarehouse).WithMany(w => w.DestinationShipments)
                .HasForeignKey(s => s.DestinationWarehouseId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.StatusHistory).WithOne(h => h.Shipment)
                .HasForeignKey(h => h.ShipmentId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ShipmentStatusHistoryConfiguration : IEntityTypeConfiguration<ShipmentStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ShipmentStatusHistory> builder)
        {
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Notes).HasMaxLength(500);
            builder.HasIndex(h => h.ShipmentId);
            builder.HasIndex(h => h.CreatedAt);
        }
    }

    public class DriverLocationConfiguration : IEntityTypeConfiguration<DriverLocation>
    {
        public void Configure(EntityTypeBuilder<DriverLocation> builder)
        {
            builder.HasKey(l => l.Id);
            builder.HasIndex(l => new { l.DriverId, l.RecordedAt });
            // Keep only last 7 days - enforced via background service
        }
    }

    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
            builder.Property(n => n.Body).HasMaxLength(1000).IsRequired();
            builder.Property(n => n.ReferenceType).HasMaxLength(100);
            builder.HasIndex(n => new { n.UserId, n.IsRead });
        }
    }

    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Token).HasMaxLength(200).IsRequired();
            builder.HasIndex(t => t.Token).IsUnique();
            builder.Property(t => t.ReplacedByToken).HasMaxLength(200);
            builder.Property(t => t.RevokedReason).HasMaxLength(200);
            builder.Property(t => t.CreatedByIp).HasMaxLength(50);
        }
    }
}
