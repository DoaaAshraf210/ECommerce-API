
using ECommerce.API.Filters;
using ECommerce.API.Handlers;
using ECommerce.Application;
 using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;
using Serilog;

namespace ECommerce.API
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());
            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            }).AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
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
            if (!app.Environment.IsDevelopment()
              && string.IsNullOrWhiteSpace(app.Configuration["Email:Host"]))
            {
                throw new InvalidOperationException(
                    "Email:Host must be configured outside Development (LogEmailService leaks reset tokens).");
            }
            var redisConfigured = !string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("Redis"));
            app.Logger.LogInformation("Cache mode: {Mode}",
                redisConfigured ? "Redis (L2) + memory (L1)" : "memory only (L1)");
            await IdentitySeeder.SeedRolesAsync(app.Services);
            await IdentitySeeder.SeedAdminAsync(app.Services, app.Configuration);
            app.UseSerilogRequestLogging();
            app.UseExceptionHandler();            // Configure the HTTP request pipeline.
        
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseStaticFiles();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();     
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
