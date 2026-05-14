using FluentValidation;
using SmartLogistics.Application.DTOs.Warehouses;

namespace SmartLogistics.Application.Common.Validators
{
    // Validator for adding a new warehouse/store to the system
    public class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
    {
        public CreateWarehouseRequestValidator()
        {
            // Warehouse Identity Validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Warehouse name is required")
                .MaximumLength(200).WithMessage("Name is too long (maximum 200 characters)");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Warehouse code is required")
                .MaximumLength(20).WithMessage("Code must not exceed 20 characters")
                .Matches("^[A-Z0-9-]+$").WithMessage("Code must contain only uppercase letters, numbers, and hyphens (-)");

            // Geographic Location
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Detailed warehouse address is required");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City/Governorate is required");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Invalid latitude coordinates");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Invalid longitude coordinates");

            // Operational Capacity
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Warehouse capacity must be greater than zero");

            // Warehouse Manager Information
            RuleFor(x => x.ManagerName)
                .NotEmpty().WithMessage("Warehouse manager name is required");

            RuleFor(x => x.ManagerPhone)
                .NotEmpty().WithMessage("Manager phone number is required");
        }
    }
}