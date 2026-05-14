using SmartLogistics.Domain.Common;
using SmartLogistics.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Entities
{
    
    public class ShipmentStatusHistory : BaseEntity
    {
        public Guid ShipmentId { get; set; }
        public ShipmentStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Navigation Property
        public Shipment Shipment { get; set; } = null!;
    }

    
    public class DriverLocation : BaseEntity
    {
        public Guid DriverId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }       // km/h
        public double? Heading { get; set; }     // degrees from north
        public double? Accuracy { get; set; }    // meters

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public User Driver { get; set; } = null!;
    }

  
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Optional reference to related entity
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }

   
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? RevokedReason { get; set; }
        public string? CreatedByIp { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation Property
        public User User { get; set; } = null!;
    }
}

