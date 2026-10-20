using ECommerce.Application.DTOs.Carts;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Carts
{
    public class UpdateCartItemDtoValidator : AbstractValidator<UpdateCartItemDto>
    {
        public UpdateCartItemDtoValidator()
        {
            RuleFor(c => c.Quantity)
              .NotEmpty().WithMessage("Quantity is required.")
              .InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");

        }
    }
}
