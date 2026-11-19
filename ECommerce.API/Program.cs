
using ECommerce.API.Filters;
using ECommerce.API.Handlers;
using ECommerce.API.Hubs;
using ECommerce.API.Hubs;
using ECommerce.Application;
using ECommerce.Application.Interfaces;
 using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Identity;
using ECommerce.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ECommerce.API
{
    public class Program
    {
    private static string ClientKey(HttpContext ctx)
    => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
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
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation failed",
                        Instance = context.HttpContext.Request.Path
                    });
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



                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            if (!string.IsNullOrEmpty(accessToken)
                                && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
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
            builder.Services.AddSignalR(o => o.MaximumReceiveMessageSize = 4 * 1024);
            builder.Services.AddSingleton<IAdminNotifier, SignalRAdminNotifier>();

            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                });
            });

         

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, ct) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too Many Requests",
                        Detail = "Too many requests. Please try again later.",
                        Instance = context.HttpContext.Request.Path
                    }, ct);
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

                options.AddPolicy("auth", ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

                options.AddPolicy("email", ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));
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

            var storage = app.Configuration.GetSection("Storage").Get<StorageSettings>() ?? new StorageSettings();
            var uploadsRoot = storage.GetRootPath(app.Environment.ContentRootPath);
            Directory.CreateDirectory(uploadsRoot);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsRoot),
                RequestPath = "/uploads",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                    ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
                }
            });
            app.UseCors("Frontend");
            app.UseRateLimiter();
    
            app.UseAuthentication();     
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<AdminHub>("/hubs/admin", options =>
            {
                options.CloseOnAuthenticationExpiration = true;
            });



            app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
               .DisableRateLimiting();

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            }).DisableRateLimiting();
            app.Run();
        }
    }
}
