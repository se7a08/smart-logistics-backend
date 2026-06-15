using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Interfaces
{
    
    public interface INotificationService
    {
        
        Task SendToDeviceAsync(string token, string title, string message, Dictionary<string, string>? extraData = null);

        Task SendToTopicAsync(string topicName, string title, string message, Dictionary<string, string>? extraData = null);

        Task SendToMultipleDevicesAsync(IEnumerable<string> tokens, string title, string message, Dictionary<string, string>? extraData = null);
    }
}