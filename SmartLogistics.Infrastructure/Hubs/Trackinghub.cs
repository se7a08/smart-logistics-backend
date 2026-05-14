using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic; 

namespace SmartLogistics.Infrastructure.Hubs
{
    [Authorize]
    public class TrackingHub : Hub
    {
        private readonly ILogger<TrackingHub> _logger;

        
        private static readonly Dictionary<string, Guid> _driverConnections = new Dictionary<string, Guid>();
        private static readonly Dictionary<Guid, HashSet<string>> _shipmentGroups = new Dictionary<Guid, HashSet<string>>();

        public TrackingHub(ILogger<TrackingHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            
            var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            
            if (_driverConnections.ContainsKey(Context.ConnectionId))
            {
                _driverConnections.Remove(Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

       
        public async Task RegisterAsDriver(Guid driverId)
        {
            _driverConnections[Context.ConnectionId] = driverId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Driver_{driverId}");
            _logger.LogInformation($"Driver {driverId} is now online");
        }

        
        public async Task SubscribeToShipment(Guid shipmentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Shipment_{shipmentId}");

           
            if (!_shipmentGroups.ContainsKey(shipmentId))
            {
                _shipmentGroups[shipmentId] = new HashSet<string>();
            }
            _shipmentGroups[shipmentId].Add(Context.ConnectionId);
        }

        public async Task UnsubscribeFromShipment(Guid shipmentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Shipment_{shipmentId}");
        }
    }
}