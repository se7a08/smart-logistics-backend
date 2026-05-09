using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.DTOs.Drivers
{
        public record UpdateLocationRequest(
        double Latitude,
        double Longitude,
        double? Speed,
        double? Heading,
        double? Accuracy
        );

        public record DriverLocationDto(
            Guid DriverId,
            string DriverName,
            double Latitude,
            double Longitude,
            double? Speed,
            double? Heading,
            DateTime RecordedAt
        );

        public record DriverTaskDto(
            Guid ShipmentId,
            string TrackingNumber,
            string RecipientName,
            string RecipientPhone,
            string DeliveryAddress,
            double DeliveryLatitude,
            double DeliveryLongitude,
            string Status,
            DateTime? EstimatedDelivery
        );
}

