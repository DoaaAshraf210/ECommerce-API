using ECommerce.Application.DTOs.Brands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Brands
{
    public class CreateBrandDtoValidator: AbstractValidator<CreateBrandDto>
    {
        public CreateBrandDtoValidator()
        {
            RuleFor(b => b.Name)
    .NotEmpty().WithMessage("Name is required.")
    .MaximumLength(100);


            RuleFor(b => b.LogoUrl)
                .MaximumLength(500);
        }
    }
}
