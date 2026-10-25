using ECommerce.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Auth
{
    public class GoogleLoginDtoValidator : AbstractValidator<GoogleLoginDto>
    {
        public GoogleLoginDtoValidator()
        {

            RuleFor(g => g.IdToken)
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(4096); ;

       
        }
    }
}
