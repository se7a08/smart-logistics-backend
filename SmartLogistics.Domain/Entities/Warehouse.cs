using SmartLogistics.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Entities
{
   
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;

        // Manager contact info
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerPhone { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Shipment> OriginShipments { get; set; } = new List<Shipment>();
        public ICollection<Shipment> DestinationShipments { get; set; } = new List<Shipment>();
    }
}

