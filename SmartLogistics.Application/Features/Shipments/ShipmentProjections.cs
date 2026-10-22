using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Domain.Entities;
using System.Linq.Expressions;


namespace SmartLogistics.Application.Features.Shipments
{
    public static class ShipmentProjections
    {
        public static readonly Expression<Func<Shipment, ShipmentDto>> ToDto = s => new ShipmentDto(
            s.Id, s.TrackingNumber, s.QrCode, s.RecipientName, s.RecipientPhone, s.RecipientEmail,
            s.DeliveryAddress, s.DeliveryLatitude, s.DeliveryLongitude, s.Weight, s.Description,
            s.DeclaredValue, s.IsFragile, s.Status.ToString(), s.PickedUpAt, s.DeliveredAt,
            s.EstimatedDelivery, s.QrVerified, s.DeliveryNotes, s.DriverId,
            s.Driver != null ? s.Driver.FullName : "Not Assigned",
            s.OriginWarehouseId, s.OriginWarehouse.Name,
            s.DestinationWarehouseId, s.DestinationWarehouse.Name,
            s.CreatedAt);
    }
}
