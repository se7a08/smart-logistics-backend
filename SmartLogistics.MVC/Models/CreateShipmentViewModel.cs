using System.ComponentModel.DataAnnotations;

namespace SmartLogistics.MVC.Models
{
    public class CreateShipmentViewModel
    {
        [Required, StringLength(100), Display(Name = "Recipient name")]
        public string RecipientName { get; set; } = string.Empty;

        [Required, StringLength(20), Display(Name = "Phone")]
        public string RecipientPhone { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256), Display(Name = "Email")]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required, StringLength(500), Display(Name = "Delivery address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Range(-90, 90)] public double DeliveryLatitude { get; set; }
        [Range(-180, 180)] public double DeliveryLongitude { get; set; }

        [Range(0.001, 100000), Display(Name = "Weight (kg)")]
        public decimal Weight { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 100000000), Display(Name = "Declared value")]
        public decimal DeclaredValue { get; set; }

        [Display(Name = "Fragile")]
        public bool IsFragile { get; set; }

        [Required, Display(Name = "Origin warehouse")]
        public Guid? OriginWarehouseId { get; set; }

        [Required, Display(Name = "Destination warehouse")]
        public Guid? DestinationWarehouseId { get; set; }

        [DataType(DataType.Date), Display(Name = "Estimated delivery")]
        public DateTime? EstimatedDelivery { get; set; }

        public List<Application.Features.Warehouses.Queries.WarehouseOptionDto> Warehouses { get; set; } = new();
    }
}
