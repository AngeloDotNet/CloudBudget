using System.Text;
using System.Text.Json.Serialization;
using CloudBudget.API.BackgroundServices;
using CloudBudget.API.Data;
using CloudBudget.API.Data.Seed;
using CloudBudget.API.Entities;
using CloudBudget.API.Mapping;
using CloudBudget.API.Middleware;
using CloudBudget.API.Repositories;
using CloudBudget.API.Repositories.Interfaces;
using CloudBudget.API.Services;
using CloudBudget.API.Services.EmailSender;
using CloudBudget.API.Services.EmailSender.Interfaces;
using CloudBudget.API.Services.Interfaces;
using CloudBudget.API.Services.ReportGenerators;
using CloudBudget.API.Services.ReportGenerators.Interfaces;
using CloudBudget.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CloudBudget.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "BudgetApp API", Version = "v1" });

            var jwtScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            };
            c.AddSecurityDefinition("Bearer", jwtScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtScheme, Array.Empty<string>() }
            });
        });

        builder.Services.AddAutoMapper(typeof(MappingProfile));
        builder.Services.AddDbContext<CloudBudgetDbContext>(options =>
        {
            var sqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(sqlConnection, sqlOptions =>
            {
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqlOptions.UseCompatibilityLevel(170); // Set compatibility level to SQL Server 2025 (17.x)
            });

            options.LogTo(Console.WriteLine, LogLevel.Information);
        });

        builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

        builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();

        builder.Services.AddTransient<IReportGenerator, CsvReportGenerator>();
        builder.Services.AddTransient<IReportGenerator, ExcelReportGenerator>();

        builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();
        builder.Services.AddTransient<IRefreshTokenService, RefreshTokenService>();

        builder.Services.AddHttpClient<IGeoIpService, HttpGeoIpService>();
        builder.Services.AddTransient<IGeoIpService, NoOpGeoIpService>();

        builder.Services.AddHostedService<CleanupService>();
        builder.Services.AddHostedService<MonthlyReportService>();

        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
                else
                {
                    // In development or if not configured, allow any (consider tightening for production)
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
            .AddEntityFrameworkStores<CloudBudgetDbContext>()
            .AddDefaultTokenProviders();

        var jwtSection = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"];
        var jwtIssuer = jwtSection["Issuer"];
        var jwtAudience = jwtSection["Audience"];

        if (!string.IsNullOrEmpty(jwtKey))
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
        }
        else
        {
            builder.Services.AddAuthentication(); // fallback minimo
        }

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("CanSendReports", policy => policy.RequireClaim("reports:send", "true"));
        });

        builder.Services.Configure<GeoIpSettings>(builder.Configuration.GetSection("GeoIp"));
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.Configure<ReportSettings>(builder.Configuration.GetSection("Report"));
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

        var app = builder.Build();

        // Run seeder at startup (ensure DB updated)
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                // apply pending migrations (opzionale)
                var db = services.GetRequiredService<CloudBudgetDbContext>();
                db.Database.Migrate();

                var seeder = services.GetRequiredService<IdentitySeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Errore durante lo startup seeding");
                throw;
            }
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<JwtRevocationMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BudgetApp API v1"));
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("DefaultCors");
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.Run();
    }
}