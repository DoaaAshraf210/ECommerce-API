using ECommerce.Application.DTOs.Categories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Categories
{
    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);
            RuleFor(c => c.Description)
                .MaximumLength(1000);

            RuleFor(c => c.ImageUrl)
                .MaximumLength(500);
        }

    }
}
