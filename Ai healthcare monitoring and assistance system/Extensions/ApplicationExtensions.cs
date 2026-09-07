using Ai_healthcare_monitoring_and_assistance_system.Data;
using Microsoft.EntityFrameworkCore;

namespace Ai_healthcare_monitoring_and_assistance_system.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.InitializeDatabase();
        return app;
    }

    private static void InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
        db.Database.Migrate();

        if (!db.PatientReadings.Any())
        {
            DatabaseSeeder.SeedMockData(db);
        }
    }
}
