using ECommerce.Application.Services;
using ECommerce.Application.Validators.Brands;
using ECommerce.Application.Validators.Categories;
using ECommerce.Application.Validators.Products;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddValidatorsFromAssemblyContaining<CreateBrandDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateBrandDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateCategoryDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateProductDtoValidator>();
            return services;
        }
    }
}
