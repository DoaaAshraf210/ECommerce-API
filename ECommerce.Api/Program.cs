
using ECommerce.API.Filters;
using ECommerce.API.Handlers;
using ECommerce.Application;
 using ECommerce.Infrastructure;
namespace ECommerce.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                //It executes after model binding and before the action; the DTO is ready,
                //and the action will not execute if the data is invalid.
                options.Filters.Add<ValidationFilter>();
            });
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();


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
            app.UseExceptionHandler();            // Configure the HTTP request pipeline.
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
