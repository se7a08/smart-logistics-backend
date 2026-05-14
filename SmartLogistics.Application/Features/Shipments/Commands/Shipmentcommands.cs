using AutoMapper;
using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Shipments.Commands
{
    // --- Create Shipment ---
    // Initiates a new shipment record and generates its unique tracking and QR code
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

            // Validate that both origin and destination warehouses exist and are operational
            var originExists = await _uow.Repository<Warehouse>().AnyAsync(w => w.Id == req.OriginWarehouseId && w.IsActive, ct);
            var destExists = await _uow.Repository<Warehouse>().AnyAsync(w => w.Id == req.DestinationWarehouseId && w.IsActive, ct);

            if (!originExists) throw new NotFoundException("Origin Warehouse", req.OriginWarehouseId);
            if (!destExists) throw new NotFoundException("Destination Warehouse", req.DestinationWarehouseId);

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
                EstimatedDelivery = req.EstimatedDelivery,
                Status = ShipmentStatus.Pending
            };

            // Link a unique QR code to the shipment for secure delivery verification
            shipment.QrCode = _qrService.GenerateQrCode(shipment.Id);

            await _uow.Repository<Shipment>().AddAsync(shipment, ct);

            // Initialize shipment history trail
            await _uow.Repository<ShipmentStatusHistory>().AddAsync(new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = ShipmentStatus.Pending,
                Notes = "Shipment record successfully created in the system."
            }, ct);

            await _uow.SaveChangesAsync(ct);

            // Fetch the created shipment with its navigation properties for a complete DTO response
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

    // --- Update Shipment Status ---
    // Handles manual status updates and maintains a geospatial history log
    public record UpdateShipmentStatusCommand(Guid ShipmentId, Guid UserId, UpdateShipmentStatusRequest Request) : IRequest<ShipmentDto>;

    public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand, ShipmentDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ITrackingService _tracking;

        public UpdateShipmentStatusCommandHandler(IUnitOfWork uow, IMapper mapper, ITrackingService tracking)
        {
            _uow = uow;
            _mapper = mapper;
            _tracking = tracking;
        }

        public async Task<ShipmentDto> Handle(UpdateShipmentStatusCommand command, CancellationToken ct)
        {
            var shipment = await _uow.Repository<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == command.ShipmentId && !s.IsDeleted, ct)
                ?? throw new NotFoundException("Shipment", command.ShipmentId);

            // Logic Check: Ensure the status move follows the logistics workflow
            ValidateStatusTransition(shipment.Status, command.Request.Status);

            shipment.Status = command.Request.Status;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedBy = command.UserId.ToString();

            if (command.Request.Status == ShipmentStatus.PickedUp) shipment.PickedUpAt = DateTime.UtcNow;
            if (command.Request.Status == ShipmentStatus.Delivered) shipment.DeliveredAt = DateTime.UtcNow;

            _uow.Repository<Shipment>().Update(shipment);

            // Append to history with current coordinates if provided
            await _uow.Repository<ShipmentStatusHistory>().AddAsync(new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = command.Request.Status,
                Notes = command.Request.Notes ?? $"Status changed to {command.Request.Status}",
                Latitude = command.Request.Latitude,
                Longitude = command.Request.Longitude
            }, ct);

            await _uow.SaveChangesAsync(ct);

            // Broadcast status update via SignalR
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
            {
                throw new BusinessRuleException($"Invalid workflow: Cannot change status from '{current}' to '{next}'.");
            }
        }
    }

    // --- Assign Driver ---
    // Links a driver to a pending shipment and triggers a push notification
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
            {
                throw new BusinessRuleException("Drivers can only be assigned to shipments in 'Pending' status.");
            }

            var driver = await _uow.Repository<User>()
                .FirstOrDefaultAsync(u => u.Id == command.DriverId && u.Role == UserRole.Driver && u.IsActive, ct)
                ?? throw new NotFoundException("Driver", command.DriverId);

            shipment.DriverId = driver.Id;
            shipment.UpdatedAt = DateTime.UtcNow;

            _uow.Repository<Shipment>().Update(shipment);
            await _uow.SaveChangesAsync(ct);

            // Notify the driver via FCM
            if (!string.IsNullOrEmpty(driver.FcmToken))
            {
                await _notifications.SendToDeviceAsync(
                    driver.FcmToken,
                    "New Assignment",
                    $"You have been assigned shipment #{shipment.TrackingNumber}.",
                    new Dictionary<string, string> { { "shipmentId", shipment.Id.ToString() } }
                );
            }

            return _mapper.Map<ShipmentDto>(shipment);
        }
    }
}