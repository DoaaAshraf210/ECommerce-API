
using Microsoft.OpenApi;
 using ECommerce.Infrastructure;
using ECommerce.Application;
namespace ECommerce.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //builder.Services.AddSwaggerGen(swagger =>
            //{
            //    swagger.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            //    {
            //        Title = "ECommerce API",
            //        Version = "v1",
            //        Description= "This is a sample ECommerce API for demonstration purposes.",
            //    });

            //swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //{
            //    Description = "Enter 'Bearer' [space] followed by your valid token.",
            //    Name = "Authorization",
            //    In = ParameterLocation.Header,
            //    Type = SecuritySchemeType.ApiKey,
            //    Scheme = "Bearer",
            //    BearerFormat = "JWT"


            //});

            //    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //        {
            //            new OpenApiSecurityScheme
            //            {
            //                Reference = new Microsoft.OpenApi.OpenApiReference
            //                {
            //                    Type = ReferenceType.SecurityScheme,
            //                    Id = "Bearer"
            //                }
            //            },
            //            new string[] {}
            //        }
            //    });
            //});

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
