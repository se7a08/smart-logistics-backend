using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Interfaces
{
    // واجهة خدمة الإشعارات عشان نبعت رسائل للموبايل (Firebase)
    public interface INotificationService
    {
        // إرسال إشعار لجهاز محدد باستخدام التوكن بتاعه
        Task SendToDeviceAsync(string token, string title, string message, Dictionary<string, string>? extraData = null);

        // إرسال إشعار لمجموعة مشتركة في موضوع معين (زي كل السواقين)
        Task SendToTopicAsync(string topicName, string title, string message, Dictionary<string, string>? extraData = null);

        // إرسال إشعار لمجموعة أجهزة في نفس الوقت
        Task SendToMultipleDevicesAsync(IEnumerable<string> tokens, string title, string message, Dictionary<string, string>? extraData = null);
    }
}