using AutoMapper;
using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Commands
{
    // --- Update Driver Location ---
    // Triggered periodically by the mobile app to track the driver's movement
    public record UpdateDriverLocationCommand(Guid DriverId, UpdateLocationRequest Request) : IRequest<DriverLocationDto>;

    public class UpdateDriverLocationCommandHandler : IRequestHandler<UpdateDriverLocationCommand, DriverLocationDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITrackingService _tracking;

        public UpdateDriverLocationCommandHandler(IUnitOfWork uow, ITrackingService tracking)
        {
            _uow = uow;
            _tracking = tracking;
        }

        public async Task<DriverLocationDto> Handle(UpdateDriverLocationCommand command, CancellationToken ct)
        {
            var driver = await _uow.Repository<User>()
                .FirstOrDefaultAsync(u => u.Id == command.DriverId && u.Role == UserRole.Driver, ct)
                ?? throw new NotFoundException("Driver", command.DriverId);

            var location = new DriverLocation
            {
                DriverId = command.DriverId,
                Latitude = command.Request.Latitude,
                Longitude = command.Request.Longitude,
                Speed = command.Request.Speed,
                Heading = command.Request.Heading,
                Accuracy = command.Request.Accuracy,
                RecordedAt = DateTime.UtcNow
            };

            await _uow.Repository<DriverLocation>().AddAsync(location, ct);
            await _uow.SaveChangesAsync(ct);

            // Broadcast location to Admin Dashboard and Client App via SignalR
            await _tracking.BroadcastDriverLocationAsync(
                command.DriverId,
                command.Request.Latitude,
                command.Request.Longitude
            );

            return new DriverLocationDto(
                driver.Id,
                driver.FullName,
                location.Latitude,
                location.Longitude,
                location.Speed,
                location.Heading,
                location.RecordedAt
            );
        }
    }

    // --- Complete Delivery ---
    // Finalizes the shipment lifecycle after successful QR verification
    public record CompleteDeliveryCommand(Guid DriverId, Guid ShipmentId, string? Notes, string? PhotoUrl) : IRequest<bool>;

    public class CompleteDeliveryCommandHandler : IRequestHandler<CompleteDeliveryCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITrackingService _tracking;

        public CompleteDeliveryCommandHandler(IUnitOfWork uow, ITrackingService tracking)
        {
            _uow = uow;
            _tracking = tracking;
        }

        public async Task<bool> Handle(CompleteDeliveryCommand command, CancellationToken ct)
        {
            var shipment = await _uow.Repository<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.DriverId == command.DriverId && !s.IsDeleted, ct)
                ?? throw new NotFoundException("Shipment", command.ShipmentId);

            // Business Rule: Delivery can only be completed if the shipment is currently in transit
            if (shipment.Status != ShipmentStatus.InTransit)
            {
                throw new BusinessRuleException("Only shipments with 'InTransit' status can be marked as delivered.");
            }

            // Security Check: QR code must be verified at the delivery point
            if (!shipment.QrVerified)
            {
                throw new BusinessRuleException("QR code verification is mandatory before completing the delivery.");
            }

            shipment.Status = ShipmentStatus.Delivered;
            shipment.DeliveredAt = DateTime.UtcNow;
            shipment.DeliveryNotes = command.Notes;
            shipment.DeliveryPhotoUrl = command.PhotoUrl;
            shipment.UpdatedAt = DateTime.UtcNow;

            _uow.Repository<Shipment>().Update(shipment);

            // Record this status change in the history trail
            await _uow.Repository<ShipmentStatusHistory>().AddAsync(new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = ShipmentStatus.Delivered,
                Notes = command.Notes ?? "Shipment successfully delivered to recipient."
            }, ct);

            await _uow.SaveChangesAsync(ct);

            // Notify all interested parties about the status update
            await _tracking.NotifyShipmentStatusChangeAsync(shipment.Id, "Delivered");

            return true;
        }
    }

    // --- Scan QR Code ---
    // Validates the physical presence of the driver at the delivery location
    public record ScanQrCommand(Guid DriverId, Guid ShipmentId, string QrCode) : IRequest<bool>;

    public class ScanQrCommandHandler : IRequestHandler<ScanQrCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IQrCodeService _qr;

        public ScanQrCommandHandler(IUnitOfWork uow, IQrCodeService qr)
        {
            _uow = uow;
            _qr = qr;
        }

        public async Task<bool> Handle(ScanQrCommand command, CancellationToken ct)
        {
            var shipment = await _uow.Repository<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.DriverId == command.DriverId, ct)
                ?? throw new NotFoundException("Shipment", command.ShipmentId);

            // Validate the scanned code against the stored shipment data
            if (!_qr.ValidateQrCode(command.QrCode, command.ShipmentId))
            {
                throw new BusinessRuleException("The scanned QR code is invalid for this specific shipment.");
            }

            shipment.QrVerified = true;
            shipment.UpdatedAt = DateTime.UtcNow;

            _uow.Repository<Shipment>().Update(shipment);
            await _uow.SaveChangesAsync(ct);

            return true;
        }
    }
}