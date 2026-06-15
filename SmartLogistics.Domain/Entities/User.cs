using SmartLogistics.Domain.Common;
using SmartLogistics.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Entities
{
    
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;

        public string? FcmToken { get; set; }

        public string? LicenseNumber { get; set; }
        public string? VehiclePlate { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Shipment> AssignedShipments { get; set; } = new List<Shipment>();
        public ICollection<DriverLocation> Locations { get; set; } = new List<DriverLocation>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

