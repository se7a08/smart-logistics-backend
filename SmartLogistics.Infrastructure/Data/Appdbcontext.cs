using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Common;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;

namespace SmartLogistics.Infrastructure.Data
{
    
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        
        public DbSet<User> Users { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Shipment> Shipments {  get; set; }
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistories {  get; set; }
        public DbSet<DriverLocation> DriverLocations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<User>(builder => {
                builder.HasKey(u => u.Id);
                builder.Property(u => u.FullName).HasMaxLength(150).IsRequired();
                builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
                builder.HasIndex(u => u.Email).IsUnique(); 
                builder.HasMany(u => u.RefreshTokens).WithOne(t => t.User).HasForeignKey(t => t.UserId);
            });

            modelBuilder.Entity<Warehouse>(w => {
                w.HasKey(x => x.Id);
                w.Property(x => x.Name).HasMaxLength(180).IsRequired();
                w.Property(x => x.Code).HasMaxLength(15).IsRequired();
                w.HasIndex(x => x.Code).IsUnique();
                w.Property(x => x.Address).HasMaxLength(450);
            });
            modelBuilder.Entity<Shipment>(s =>
            {
                s.HasKey(s => s.Id);
                s.Property(s => s.TrackingNumber).HasMaxLength(50).IsRequired();
                s.HasIndex(s => s.TrackingNumber).IsUnique();
                s.Property(s => s.QrCode).HasMaxLength(500).IsRequired();
                s.Property(s => s.RecipientName).HasMaxLength(100).IsRequired();
                s.Property(s => s.RecipientPhone).HasMaxLength(20).IsRequired();
                s.Property(s => s.RecipientEmail).HasMaxLength(256).IsRequired();
                s.Property(s => s.DeliveryAddress).HasMaxLength(500).IsRequired();
                s.Property(s => s.Description).HasMaxLength(500).IsRequired();
                s.Property(s => s.Weight).HasColumnType("decimal(10,3)");
                s.Property(s => s.DeclaredValue).HasColumnType("decimal(18,2)");
                s.Property(s => s.DeliveryNotes).HasMaxLength(1000);
                s.Property(s => s.DeliveryPhotoUrl).HasMaxLength(500);
                s.HasOne(s => s.Driver).WithMany(u => u.AssignedShipments)
                .HasForeignKey(s => s.DriverId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);

                s.HasOne(s => s.OriginWarehouse).WithMany(w => w.OriginShipments)
                    .HasForeignKey(s => s.OriginWarehouseId).OnDelete(DeleteBehavior.Restrict);

                s.HasOne(s => s.DestinationWarehouse).WithMany(w => w.DestinationShipments)
                    .HasForeignKey(s => s.DestinationWarehouseId).OnDelete(DeleteBehavior.Restrict);

                s.HasMany(s => s.StatusHistory).WithOne(h => h.Shipment)
                    .HasForeignKey(h => h.ShipmentId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Notification>(n =>
            {
                n.HasKey(n => n.Id);
                n.Property(n => n.Title).HasMaxLength(200).IsRequired();
                n.Property(n => n.Body).HasMaxLength(1000).IsRequired();
                n.Property(n => n.ReferenceType).HasMaxLength(100);
                n.HasIndex(n => new { n.UserId, n.IsRead });
            });
            modelBuilder.Entity<ShipmentStatusHistory>(sh =>
            {
                sh.HasKey(h => h.Id);
                sh.Property(h => h.Notes).HasMaxLength(500);
                sh.HasIndex(h => h.ShipmentId);
                sh.HasIndex(h => h.CreatedAt);
            });
            modelBuilder.Entity<DriverLocation>(l => 
            {
                l.HasKey(l => l.Id);
                l.HasIndex(l => new { l.DriverId, l.RecordedAt });
            });
            modelBuilder.Entity<RefreshToken>(r =>
            {
                r.HasKey(r => r.Id);
                r.Property(t => t.Token).HasMaxLength(200).IsRequired();
                r.HasIndex(t => t.Token).IsUnique();
                r.Property(t => t.ReplacedByToken).HasMaxLength(200);
                r.Property(t => t.RevokedReason).HasMaxLength(200);
                r.Property(t => t.CreatedByIp).HasMaxLength(50);
            });
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Shipment>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Warehouse>().HasQueryFilter(w => !w.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    // تحويل المسح الحقيقي لمسح منطقي (Soft Delete)
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
