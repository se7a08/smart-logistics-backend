using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Enums
{
    /// <summary>
    /// Shipment lifecycle statuses.
    /// </summary>
    public enum ShipmentStatus
    {
        Pending = 0,
        PickedUp = 1,
        InTransit = 2,
        Delivered = 3,
        Cancelled = 4
    }

    /// <summary>
    /// System roles for role-based authorization.
    /// </summary>
    public enum UserRole
    {
        Admin = 0,
        Driver = 1
    }

    /// <summary>
    /// Notification types for FCM push messages.
    /// </summary>
    public enum NotificationType
    {
        ShipmentAssigned = 0,
        ShipmentDelivered = 1,
        ShipmentDelayed = 2,
        StatusUpdated = 3,
        General = 4
    }
}

