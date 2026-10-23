using ECommerce.Application.DTOs.Orders;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace ECommerce.Application.Validators.Orders
{
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusDtoValidator()
        {

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status must be a valid enum value.");


        }
    }
}
