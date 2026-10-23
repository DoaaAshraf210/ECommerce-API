using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Validators.Products;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Tests.Validators
{
    public class CreateProductDtoValidatorTests
    {
        private readonly CreateProductDtoValidator _validator = new();

        [Fact]
        public void Validate_InvalidProduct_HasErrors()
        {
            var dto = new CreateProductDto("", null, "A1", -5, 0, true, 1, 1);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
            result.ShouldHaveValidationErrorFor(x => x.Price);
            result.ShouldNotHaveValidationErrorFor(x => x.Sku);
        }

        [Fact]
        public void Validate_ValidProduct_HasNoErrors()
        {
            var dto = new CreateProductDto("Phone", null, "PH-1", 99.99m, 5, true, 1, 1);

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}