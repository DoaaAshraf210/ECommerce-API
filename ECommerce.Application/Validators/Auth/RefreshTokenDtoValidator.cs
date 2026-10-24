using ECommerce.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Auth
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(r => r.RefreshToken)
             .NotEmpty().WithMessage("Refresh Token is required.")
             .MaximumLength(256); ;

          
        }



    }
}
