
using ECommerce.API.Filters;
using ECommerce.API.Handlers;
using ECommerce.Application;
 using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
namespace ECommerce.API
{
    public class Program
    {
        public async static Task Main(string[] args)
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

            var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("Jwt settings are missing.");

            if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
                throw new InvalidOperationException("Jwt:Key must be at least 32 characters. Set it in User Secrets.");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = "role",
                        NameClaimType = "name"
                    };
                });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();


            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste your JWT token only (without the word Bearer)."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });          

            var app = builder.Build(); 
            await IdentitySeeder.SeedRolesAsync(app.Services);
            await IdentitySeeder.SeedAdminAsync(app.Services, app.Configuration);
            app.UseExceptionHandler();            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();     
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
