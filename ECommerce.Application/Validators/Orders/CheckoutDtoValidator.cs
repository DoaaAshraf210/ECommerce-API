using ECommerce.Application.DTOs.Orders;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Orders
{
    public class CheckoutDtoValidator : AbstractValidator<CheckoutDto>
    {


        public CheckoutDtoValidator()
        {

            RuleFor(x => x.ShippingAddress).NotNull()
                .SetValidator(new ShippingAddressDtoValidator());
        }
    }
}
       