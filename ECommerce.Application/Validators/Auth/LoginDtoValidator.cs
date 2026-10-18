using ECommerce.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Validators.Auth
{
    public class LoginDtoValidator:AbstractValidator<LoginDto>    
    {
        public LoginDtoValidator()
        {

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address.").MaximumLength(256); ;

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");

        }
    }
}
