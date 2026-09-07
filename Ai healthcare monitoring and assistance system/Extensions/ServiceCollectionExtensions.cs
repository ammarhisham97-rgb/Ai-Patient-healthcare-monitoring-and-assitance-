using System.Text;
using System.Text.Json.Serialization;
using Ai_healthcare_monitoring_and_assistance_system.AI.Services;
using Ai_healthcare_monitoring_and_assistance_system.Data;
using Ai_healthcare_monitoring_and_assistance_system.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ML;

namespace Ai_healthcare_monitoring_and_assistance_system.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddAuthentication(services, configuration);
        AddCors(services);
        AddDatabase(services, configuration);
        AddDomainServices(services);
        AddJsonOptions(services);
        return services;
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = configuration["JwtSecret"] ?? "YourSuperSecretKeyForJWTAuthenticationMinimum32Characters!!!";
        var jwtIssuer = configuration["JwtIssuer"] ?? "PatientMonitorAPI";
        var jwtAudience = configuration["JwtAudience"] ?? "PatientMonitorClient";
        var key = Encoding.ASCII.GetBytes(jwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true
            };
        });

        services.AddAuthorization();
    }

    private static void AddCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<HealthMonitorDbContext>(options => options.UseSqlServer(connectionString));
    }

    private static void AddDomainServices(IServiceCollection services)
    {
        services.AddSingleton<ReadingsStore>(sp => new ReadingsStore(sp));
        services.AddSingleton<UserStore>(sp => new UserStore(sp));
        services.AddSingleton<AnomalyDetectionService>(sp => new AnomalyDetectionService(sp.GetRequiredService<MLContext>(), sp));
        services.AddSingleton(sp => new MLContext());
        services.AddScoped<ReportService>();
    }

    private static void AddJsonOptions(IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    }
}
