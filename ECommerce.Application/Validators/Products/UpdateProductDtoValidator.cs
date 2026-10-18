using ECommerce.Application.DTOs.Products;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Products
{
    public class UpdateProductDtoValidator: AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {

            RuleFor(x => x.Name)
                      .NotEmpty().WithMessage("Name is required.")
                      .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000);

            RuleFor(x => x.Sku)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .PrecisionScale(18, 2, false);

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.CategoryId).GreaterThan(0);
            RuleFor(x => x.BrandId).GreaterThan(0);
        }

















    
}
}
