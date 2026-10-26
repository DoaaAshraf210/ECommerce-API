using ECommerce.Application.DTOs.Products;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Products
{
    public class ProductQueryParamsValidator : AbstractValidator<ProductQueryParams>
    {

        public ProductQueryParamsValidator()
        {
            RuleFor(x => x.Search)
             .MaximumLength(100)
             .WithMessage("Search term cannot exceed 100 characters.");

            RuleFor(x => x.PageNumber)
                .InclusiveBetween(1, 10000)
                .WithMessage("Page number must be between 1 and 10000.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50)
                .WithMessage("Page size must be between 1 and 50.");
        }


    }
}