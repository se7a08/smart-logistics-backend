using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Infrastructure.Services.Notifications
{
    
    public class FcmNotificationService : INotificationService
    {
        private readonly ILogger<FcmNotificationService> _logger;
        private readonly FirebaseMessaging? _messaging;

        public FcmNotificationService(IConfiguration config, ILogger<FcmNotificationService> logger)
        {
            _logger = logger;

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    var path = config["Firebase:CredentialFilePath"];

                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        _logger.LogWarning("Firebase credentials not found (path: {Path}). Push notifications are disabled.", path);
                        return;
                    }

                    FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromFile(path) });
                }

                _messaging = FirebaseMessaging.DefaultInstance;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase init failed. Push notifications are disabled.");
            }
        }

        public async Task SendToDeviceAsync(string token, string title, string messageBody, Dictionary<string, string>? extraData = null)
        {
            try
            {
                var fcmMessage = new Message
                {
                    Token = token,
                    Notification = new Notification { Title = title, Body = messageBody },
                    Data = extraData ?? new Dictionary<string, string>(),
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High 
                    }
                };

                var result = await _messaging.SendAsync(fcmMessage);
                _logger.LogInformation($"Notification sent! ID: {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification to device: {ex.Message}");
            }
        }

        public async Task SendToTopicAsync(string topicName, string title, string messageBody, Dictionary<string, string>? extraData = null)
        {
            try
            {
                var fcmMessage = new Message
                {
                    Topic = topicName,
                    Notification = new Notification { Title = title, Body = messageBody },
                    Data = extraData ?? new Dictionary<string, string>()
                };

                await _messaging.SendAsync(fcmMessage);
                _logger.LogInformation($"Topic notification sent to: {topicName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending topic notification: {ex.Message}");
            }
        }

        public async Task SendToMultipleDevicesAsync(IEnumerable<string> tokens, string title, string messageBody, Dictionary<string, string>? extraData = null)
        {
            if (_messaging is null) return;
            var tokensList = tokens.ToList();
            if (!tokensList.Any()) return;

            try
            {
                var multicastMsg = new MulticastMessage
                {
                    Tokens = tokensList,
                    Notification = new Notification { Title = title, Body = messageBody },
                    Data = extraData ?? new Dictionary<string, string>()
                };

                var response = await _messaging.SendEachForMulticastAsync(multicastMsg);
                _logger.LogInformation($"Multicast sent: {response.SuccessCount} success, {response.FailureCount} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Multicast error: {ex.Message}");
            }
        }
    }
}