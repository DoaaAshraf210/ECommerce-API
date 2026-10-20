using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.DTOs.Categories;
using ECommerce.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Carts
{
    public class AddCartItemDtoValidator : AbstractValidator<AddCartItemDto>
    {


        public AddCartItemDtoValidator()
        {


            RuleFor(c => c.ProductId)
                .NotEmpty().WithMessage("ProductId is required.")
                .GreaterThan(0).WithMessage("ProductId must be greater than 0.");


            RuleFor(c => c.Quantity)
                .NotEmpty().WithMessage("Quantity is required.")
                .InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
        }
    }
}
