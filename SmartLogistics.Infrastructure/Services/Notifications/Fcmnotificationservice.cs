using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using global::SmartLogistics.Domain.Interfaces;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Infrastructure.Services.Notifications
{
    
   
    /// <summary>
    /// Firebase Cloud Messaging service for push notifications.
    /// Supports single device, multiple devices, and topic-based messaging.
    /// </summary>
    public class FcmNotificationService : INotificationService
    {
        private readonly ILogger<FcmNotificationService> _logger;
        private readonly FirebaseMessaging _messaging;

        public FcmNotificationService(IConfiguration config, ILogger<FcmNotificationService> logger)
        {
            _logger = logger;

            // Initialize Firebase Admin SDK if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialPath = config["Firebase:CredentialFilePath"];
                FirebaseApp.Create(new AppOptions
                {
                    Credential = string.IsNullOrEmpty(credentialPath)
                        ? GoogleCredential.GetApplicationDefault()
                        : GoogleCredential.FromFile(credentialPath)
                });
            }

            _messaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task SendToDeviceAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message
                {
                    Token = fcmToken,
                    Notification = new Notification { Title = title, Body = body },
                    Data = data ?? new Dictionary<string, string>(),
                    Android = new AndroidConfig
                    {
                        Notification = new AndroidNotification
                        {
                            ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                            Priority = NotificationPriority.HIGH
                        }
                    }
                };

                var response = await _messaging.SendAsync(message);
                _logger.LogInformation("FCM message sent to device. MessageId: {MessageId}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send FCM notification to device token: {Token}", fcmToken[..Math.Min(20, fcmToken.Length)]);
            }
        }

        public async Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message
                {
                    Topic = topic,
                    Notification = new Notification { Title = title, Body = body },
                    Data = data ?? new Dictionary<string, string>()
                };

                var response = await _messaging.SendAsync(message);
                _logger.LogInformation("FCM message sent to topic '{Topic}'. MessageId: {MessageId}", topic, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send FCM notification to topic: {Topic}", topic);
            }
        }

        public async Task SendToMultipleDevicesAsync(IEnumerable<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null)
        {
            var tokenList = fcmTokens.ToList();
            if (!tokenList.Any()) return;

            try
            {
                var message = new MulticastMessage
                {
                    Tokens = tokenList,
                    Notification = new Notification { Title = title, Body = body },
                    Data = data ?? new Dictionary<string, string>()
                };

                var response = await _messaging.SendEachForMulticastAsync(message);
                _logger.LogInformation("FCM multicast: {SuccessCount} sent, {FailureCount} failed out of {Total}",
                    response.SuccessCount, response.FailureCount, tokenList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send FCM multicast notification");
            }
        }
    }
}
