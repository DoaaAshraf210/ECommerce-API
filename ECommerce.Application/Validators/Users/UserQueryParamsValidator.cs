using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.DTOs.Users;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Users
{
    public class UserQueryParamsValidator : AbstractValidator<UserQueryParams>
    {
        public UserQueryParamsValidator()
        {
            RuleFor(x => x.Search)
          .MaximumLength(100)
          .WithMessage("Search must not exceed 100 characters.");

            RuleFor(x => x.PageNumber)
                .InclusiveBetween(1, 10000);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50);

            RuleFor(x => x.Role)
                .Must(r => r is Roles.Admin or Roles.Customer)
                .When(x => !string.IsNullOrWhiteSpace(x.Role))
                .WithMessage($"Role must be '{Roles.Admin}' or '{Roles.Customer}'.");
        }
    }
}
