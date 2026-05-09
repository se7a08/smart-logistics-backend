using AutoMapper;
using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Features.Shipments.Commands
{
    // ─── Create Shipment ────────────────────────────────────────────────────────

    public record CreateShipmentCommand(CreateShipmentRequest Request) : IRequest<ShipmentDto>;

    public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, ShipmentDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IQrCodeService _qrService;

        public CreateShipmentCommandHandler(IUnitOfWork uow, IMapper mapper, IQrCodeService qrService)
        {
            _uow = uow;
            _mapper = mapper;
            _qrService = qrService;
        }

        public async Task<ShipmentDto> Handle(CreateShipmentCommand command, CancellationToken ct)
        {
            var req = command.Request;

            var originExists = await _uow.Repository<Warehouse>().AnyAsync(w => w.Id == req.OriginWarehouseId && w.IsActive, ct);
            var destExists = await _uow.Repository<Warehouse>().AnyAsync(w => w.Id == req.DestinationWarehouseId && w.IsActive, ct);

            if (!originExists) throw new NotFoundException("Warehouse", req.OriginWarehouseId);
            if (!destExists) throw new NotFoundException("Warehouse", req.DestinationWarehouseId);

            var shipment = new Shipment
            {
                TrackingNumber = GenerateTrackingNumber(),
                RecipientName = req.RecipientName,
                RecipientPhone = req.RecipientPhone,
                RecipientEmail = req.RecipientEmail,
                DeliveryAddress = req.DeliveryAddress,
                DeliveryLatitude = req.DeliveryLatitude,
                DeliveryLongitude = req.DeliveryLongitude,
                Weight = req.Weight,
                Description = req.Description,
                DeclaredValue = req.DeclaredValue,
                IsFragile = req.IsFragile,
                OriginWarehouseId = req.OriginWarehouseId,
                DestinationWarehouseId = req.DestinationWarehouseId,
                EstimatedDelivery = req.EstimatedDelivery
            };

            // Generate QR code tied to shipment ID
            shipment.QrCode = _qrService.GenerateQrCode(shipment.Id);

            await _uow.Repository<Shipment>().AddAsync(shipment, ct);

            // Record initial status history
            await _uow.Repository<ShipmentStatusHistory>().AddAsync(new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = ShipmentStatus.Pending,
                Notes = "Shipment created"
            }, ct);

            await _uow.SaveChangesAsync(ct);

            // Reload with navigation properties for mapping
            var created = await _uow.Repository<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == shipment.Id, ct)
                ?? throw new NotFoundException("Shipment", shipment.Id);

            return _mapper.Map<ShipmentDto>(created);
        }

        private static string GenerateTrackingNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = Random.Shared.Next(100000, 999999);
            return $"SL-{timestamp}-{random}";
        }
    }

    // ─── Update Shipment Status ─────────────────────────────────────────────────

    public record UpdateShipmentStatusCommand(Guid ShipmentId, Guid UserId, UpdateShipmentStatusRequest Request) : IRequest<ShipmentDto>;

    public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand, ShipmentDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notifications;
        private readonly ITrackingService _tracking;

        public UpdateShipmentStatusCommandHandler(IUnitOfWork uow, IMapper mapper,
            INotificationService notifications, ITrackingService tracking)
        {
            _uow = uow;
            _mapper = mapper;
            _notifications = notifications;
            _tracking = tracking;
        }

        public async Task<ShipmentDto> Handle(UpdateShipmentStatusCommand command, CancellationToken ct)
        {
            var shipment = await _uow.Repository<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == command.ShipmentId && !s.IsDeleted, ct)
                ?? throw new NotFoundException("Shipment", command.ShipmentId);

            ValidateStatusTransition(shipment.Status, command.Request.Status);

            shipment.Status = command.Request.Status;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedBy = command.UserId.ToString();

            if (command.Request.Status == ShipmentStatus.PickedUp)
                shipment.PickedUpAt = DateTime.UtcNow;
            if (command.Request.Status == ShipmentStatus.Delivered)
                shipment.DeliveredAt = DateTime.UtcNow;

            _uow.Repository<Shipment>().Update(shipment);

            await _uow.Repository<ShipmentStatusHistory>().AddAsync(new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = command.Request.Status,
                Notes = command.Request.Notes ?? string.Empty,
                Latitude = command.Request.Latitude,
                Longitude = command.Request.Longitude
            }, ct);

            await _uow.SaveChangesAsync(ct);

            // Real-time notification via SignalR
            await _tracking.NotifyShipmentStatusChangeAsync(shipment.Id, shipment.Status.ToString());

            return _mapper.Map<ShipmentDto>(shipment);
        }

        private static void ValidateStatusTransition(ShipmentStatus current, ShipmentStatus next)
        {
            var validTransitions = new Dictionary<ShipmentStatus, HashSet<ShipmentStatus>>
        {
            { ShipmentStatus.Pending,   new() { ShipmentStatus.PickedUp, ShipmentStatus.Cancelled } },
            { ShipmentStatus.PickedUp,  new() { ShipmentStatus.InTransit, ShipmentStatus.Cancelled } },
            { ShipmentStatus.InTransit, new() { ShipmentStatus.Delivered, ShipmentStatus.Cancelled } },
            { ShipmentStatus.Delivered, new() },
            { ShipmentStatus.Cancelled, new() }
        };

            if (!validTransitions[current].Contains(next))
                throw new BusinessRuleException($"Cannot transition shipment from '{current}' to '{next}'.");
        }
    }

    // ─── Assign Driver ──────────────────────────────────────────────────────────

    public record AssignDriverCommand(Guid ShipmentId, Guid DriverId) : IRequest<ShipmentDto>;

    public class AssignDriverCommandHandler : IRequestHandler<AssignDriverCommand, ShipmentDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notifications;

        public AssignDriverCommandHandler(IUnitOfWork uow, IMapper mapper, INotificationService notifications)
        {
            _uow = uow;
            _mapper = mapper;
            _notifications = notifications;
        }

        public async Task<ShipmentDto> Handle(AssignDriverCommand command, CancellationToken ct)
        {
            var shipment = await _uow.Repository<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == command.ShipmentId && !s.IsDeleted, ct)
                ?? throw new NotFoundException("Shipment", command.ShipmentId);

            if (shipment.Status != ShipmentStatus.Pending)
                throw new BusinessRuleException("Only pending shipments can have a driver assigned.");

            var driver = await _uow.Repository<User>()
                .FirstOrDefaultAsync(u => u.Id == command.DriverId && u.Role == Domain.Enums.UserRole.Driver && u.IsActive, ct)
                ?? throw new NotFoundException("Driver", command.DriverId);

            shipment.DriverId = driver.Id;
            shipment.UpdatedAt = DateTime.UtcNow;
            _uow.Repository<Shipment>().Update(shipment);
            await _uow.SaveChangesAsync(ct);

            // Push notification to driver
            if (!string.IsNullOrEmpty(driver.FcmToken))
            {
                await _notifications.SendToDeviceAsync(driver.FcmToken,
                    "New Shipment Assigned",
                    $"Shipment {shipment.TrackingNumber} has been assigned to you.",
                    new Dictionary<string, string> { { "shipmentId", shipment.Id.ToString() } });
            }

            return _mapper.Map<ShipmentDto>(shipment);
        }
    }
}
