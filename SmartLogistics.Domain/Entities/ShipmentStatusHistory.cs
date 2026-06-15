using SmartLogistics.Domain.Common;
using SmartLogistics.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Entities
{

    public class ShipmentStatusHistory : BaseEntity
    {
        public Guid ShipmentId { get; set; }
        public ShipmentStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Shipment Shipment { get; set; } = null!;
    }
}

