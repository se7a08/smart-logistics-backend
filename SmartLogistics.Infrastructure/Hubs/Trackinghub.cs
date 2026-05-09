using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::SmartLogistics.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartLogistics.Domain.Interfaces;
using System.Collections.Concurrent;

namespace SmartLogistics.Infrastructure.Hubs
{
   
   
    /// <summary>
    /// SignalR hub for real-time GPS tracking and shipment status updates.
    /// Supports:
    /// - Admin subscribing to all driver locations
    /// - Per-shipment tracking rooms for customers
    /// - Live admin dashboard updates
    /// </summary>
    [Authorize]
    public class TrackingHub : Hub
    {
        private readonly ILogger<TrackingHub> _logger;

        // Track which connectionId belongs to which driverId (thread-safe)
        private static readonly ConcurrentDictionary<string, Guid> _driverConnections = new();
        private static readonly ConcurrentDictionary<Guid, HashSet<string>> _shipmentGroups = new();

        public TrackingHub(ILogger<TrackingHub> logger) => _logger = logger;

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            _logger.LogInformation("User {UserId} connected to TrackingHub. Role: {Role}", userId, role);

            // Admins automatically join the admin dashboard group
            if (role == "Admin")
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _driverConnections.TryRemove(Context.ConnectionId, out _);
            _logger.LogInformation("User {UserId} disconnected from TrackingHub", Context.UserIdentifier);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Driver calls this to register their connection for location tracking.
        /// </summary>
        public async Task RegisterAsDriver(Guid driverId)
        {
            _driverConnections[Context.ConnectionId] = driverId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Driver_{driverId}");
            _logger.LogInformation("Driver {DriverId} registered for live tracking", driverId);
        }

        /// <summary>
        /// Client subscribes to live updates for a specific shipment.
        /// </summary>
        public async Task SubscribeToShipment(Guid shipmentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Shipment_{shipmentId}");

            _shipmentGroups.AddOrUpdate(
                shipmentId,
                new HashSet<string> { Context.ConnectionId },
                (_, existing) => { existing.Add(Context.ConnectionId); return existing; });
        }

        /// <summary>
        /// Unsubscribe from shipment tracking.
        /// </summary>
        public async Task UnsubscribeFromShipment(Guid shipmentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Shipment_{shipmentId}");
        }
    }

    /// <summary>
    /// Service that broadcasts real-time events via SignalR hub context.
    /// Injected into application layer services to avoid hub context coupling.
    /// </summary>
    public class TrackingService : ITrackingService
    {
        private readonly IHubContext<TrackingHub> _hubContext;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(IHubContext<TrackingHub> hubContext, ILogger<TrackingService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Broadcasts driver GPS coordinates to all admins in real-time.
        /// </summary>
        public async Task BroadcastDriverLocationAsync(Guid driverId, double lat, double lng)
        {
            var payload = new
            {
                DriverId = driverId,
                Latitude = lat,
                Longitude = lng,
                Timestamp = DateTime.UtcNow
            };

            // Broadcast to admins group
            await _hubContext.Clients.Group("Admins")
                .SendAsync("DriverLocationUpdated", payload);

            _logger.LogDebug("Broadcast location for driver {DriverId}: {Lat}, {Lng}", driverId, lat, lng);
        }

        /// <summary>
        /// Notifies subscribers of a shipment status change.
        /// </summary>
        public async Task NotifyShipmentStatusChangeAsync(Guid shipmentId, string status)
        {
            var payload = new { ShipmentId = shipmentId, Status = status, Timestamp = DateTime.UtcNow };

            await _hubContext.Clients.Group($"Shipment_{shipmentId}")
                .SendAsync("ShipmentStatusChanged", payload);

            await _hubContext.Clients.Group("Admins")
                .SendAsync("ShipmentStatusChanged", payload);
        }

        /// <summary>
        /// Sends arbitrary events to the admin dashboard group.
        /// </summary>
        public async Task NotifyAdminDashboardAsync(string eventType, object data)
        {
            await _hubContext.Clients.Group("Admins")
                .SendAsync(eventType, data);
        }
    }
}
