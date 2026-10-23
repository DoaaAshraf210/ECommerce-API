using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.DTOs.Orders;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Orders
{
    public class ShippingAddressDtoValidator: AbstractValidator<ShippingAddressDto>
    {


        public ShippingAddressDtoValidator()
        {
            RuleFor(sa => sa.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100);
            RuleFor(sa => sa.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).Matches(@"^\+?[0-9]\d{10,15}$");
            RuleFor(sa => sa.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(200);
            RuleFor(sa => sa.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(100);
            RuleFor(sa => sa.PostalCode)
                .MaximumLength(20);
        }
    }
}

       