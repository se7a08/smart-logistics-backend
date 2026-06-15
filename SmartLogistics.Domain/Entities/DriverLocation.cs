using SmartLogistics.Domain.Common;

namespace SmartLogistics.Domain.Entities
{
    public class DriverLocation : BaseEntity
    {
        public Guid DriverId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }       
        public double? Heading { get; set; }     
        public double? Accuracy { get; set; }    

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public User Driver { get; set; } = null!;
    }
}

