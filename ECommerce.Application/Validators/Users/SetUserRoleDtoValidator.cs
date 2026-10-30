using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.DTOs.Users;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Users
{
    public class SetUserRoleDtoValidator : AbstractValidator<SetUserRoleDto>
    {
        public SetUserRoleDtoValidator()
        {

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(r => r is Roles.Admin or Roles.Customer)
                .WithMessage($"Role must be '{Roles.Admin}' or '{Roles.Customer}'.");

        }
    }
}
