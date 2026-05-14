using FluentValidation;
using SmartLogistics.Application.DTOs.Shipments;

namespace SmartLogistics.Application.Common.Validators
{
    // Validator class for validating new shipment creation requests
    public class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
    {
        public CreateShipmentRequestValidator()
        {
            // Recipient Information Validation
            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage("Recipient name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.RecipientPhone)
                .NotEmpty().WithMessage("Recipient phone number is required");

            RuleFor(x => x.RecipientEmail)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Invalid email address format");

            // Address and Geo-location Validation
            RuleFor(x => x.DeliveryAddress)
                .NotEmpty().WithMessage("Delivery address is required")
                .MaximumLength(500).WithMessage("Address is too long");

            RuleFor(x => x.DeliveryLatitude)
                .InclusiveBetween(-90, 90).WithMessage("Invalid latitude coordinates");

            RuleFor(x => x.DeliveryLongitude)
                .InclusiveBetween(-180, 180).WithMessage("Invalid longitude coordinates");

            // Shipment Specifications Validation
            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Shipment weight must be greater than zero");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Shipment description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.DeclaredValue)
                .GreaterThanOrEqualTo(0).WithMessage("Declared value cannot be negative");

            // Route Validation
            RuleFor(x => x.OriginWarehouseId)
                .NotEmpty().WithMessage("Origin warehouse must be specified");

            RuleFor(x => x.DestinationWarehouseId)
                .NotEmpty().WithMessage("Destination warehouse must be specified")
                .NotEqual(x => x.OriginWarehouseId).WithMessage("Destination warehouse must be different from the origin warehouse");
        }
    }
}