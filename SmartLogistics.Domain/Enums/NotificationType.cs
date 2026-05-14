namespace SmartLogistics.Domain.Enums
{
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

