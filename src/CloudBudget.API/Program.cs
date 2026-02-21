
using System.Text.Json.Serialization;
using CloudBudget.API.BackgroundServices;
using CloudBudget.API.Data;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        builder.Services.AddSwaggerGen();

        //builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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

        builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();

        builder.Services.AddTransient<IReportGenerator, CsvReportGenerator>();
        builder.Services.AddTransient<IReportGenerator, ExcelReportGenerator>();

        builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();
        builder.Services.AddTransient<IRefreshTokenService, RefreshTokenService>();

        //builder.Services.AddTransient<IGeoIpService, HttpGeoIpService>();
        builder.Services.AddHttpClient<IGeoIpService, HttpGeoIpService>();
        builder.Services.AddTransient<IGeoIpService, NoOpGeoIpService>();

        builder.Services.AddHostedService<CleanupService>();
        builder.Services.AddHostedService<MonthlyReportService>();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<CloudBudgetDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<GeoIpSettings>(builder.Configuration.GetSection("GeoIpSettings"));
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services.Configure<ReportSettings>(builder.Configuration.GetSection("ReportSettings"));
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseMiddleware<JwtRevocationMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
        app.MapControllers();

        app.Run();
    }
}