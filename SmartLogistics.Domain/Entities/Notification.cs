using SmartLogistics.Domain.Common;
using SmartLogistics.Domain.Enums;

namespace SmartLogistics.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }

        public User User { get; set; } = null!;
    }
}

