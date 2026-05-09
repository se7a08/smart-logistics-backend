using SmartLogistics.Domain.Common;
using SmartLogistics.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Entities
{
    /// <summary>
    /// Core shipment entity representing a logistics delivery order.
    /// Contains QR code, status tracking, and full lifecycle management.
    /// </summary>
    public class Shipment : BaseEntity
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;

        // Recipient details
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public double DeliveryLatitude { get; set; }
        public double DeliveryLongitude { get; set; }

        // Package details
        public decimal Weight { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal DeclaredValue { get; set; }
        public bool IsFragile { get; set; }

        // Status and lifecycle
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? EstimatedDelivery { get; set; }

        // Delivery proof
        public string? DeliveryNotes { get; set; }
        public string? DeliveryPhotoUrl { get; set; }
        public bool QrVerified { get; set; } = false;

        // Foreign keys
        public Guid? DriverId { get; set; }
        public Guid OriginWarehouseId { get; set; }
        public Guid DestinationWarehouseId { get; set; }

        // Navigation Properties
        public User? Driver { get; set; }
        public Warehouse OriginWarehouse { get; set; } = null!;
        public Warehouse DestinationWarehouse { get; set; } = null!;
        public ICollection<ShipmentStatusHistory> StatusHistory { get; set; } = new List<ShipmentStatusHistory>();
    }
}

