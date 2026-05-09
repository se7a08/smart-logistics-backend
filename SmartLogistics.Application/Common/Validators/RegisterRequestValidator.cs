using FluentValidation;
using SmartLogistics.Application.DTOs.Auth;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Application.DTOs.Warehouses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Common.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.");
            RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[1-9]\d{6,14}$")
                .WithMessage("Invalid phone number format.");
        }
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
    {
        public CreateShipmentRequestValidator()
        {
            RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.RecipientPhone).NotEmpty();
            RuleFor(x => x.RecipientEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.DeliveryAddress).NotEmpty().MaximumLength(500);
            RuleFor(x => x.DeliveryLatitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.DeliveryLongitude).InclusiveBetween(-180, 180);
            RuleFor(x => x.Weight).GreaterThan(0);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            RuleFor(x => x.DeclaredValue).GreaterThanOrEqualTo(0);
            RuleFor(x => x.OriginWarehouseId).NotEmpty();
            RuleFor(x => x.DestinationWarehouseId).NotEmpty()
                .NotEqual(x => x.OriginWarehouseId)
                .WithMessage("Destination warehouse must differ from origin warehouse.");
        }
    }

    public class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
    {
        public CreateWarehouseRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(20).Matches("^[A-Z0-9-]+$")
                .WithMessage("Code must be uppercase letters, numbers, or hyphens only.");
            RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
            RuleFor(x => x.Capacity).GreaterThan(0);
            RuleFor(x => x.ManagerName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ManagerPhone).NotEmpty();
        }
    }
}

