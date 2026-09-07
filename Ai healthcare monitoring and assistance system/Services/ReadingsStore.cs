using Ai_healthcare_monitoring_and_assistance_system.Data;

namespace Ai_healthcare_monitoring_and_assistance_system.Services;

public class ReadingsStore
{
    private readonly IServiceProvider _serviceProvider;
    private const int MaxReadings = 100;

    public ReadingsStore(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public void Add(PatientReadingEntity reading)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
        db.PatientReadings.Add(reading);
        db.SaveChanges();
        var oldReadings = db.PatientReadings.Where(r => r.DeviceId == reading.DeviceId).OrderByDescending(r => r.ReceivedAt).Skip(MaxReadings).ToList();
        if (oldReadings.Count > 0)
        {
            db.PatientReadings.RemoveRange(oldReadings);
            db.SaveChanges();
        }
    }

    public IEnumerable<PatientReadingEntity> GetAll()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
        return db.PatientReadings.OrderByDescending(r => r.ReceivedAt).Take(100).ToList();
    }

    public PatientReadingEntity? GetLatest()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
        return db.PatientReadings.OrderByDescending(r => r.ReceivedAt).FirstOrDefault();
    }

    public int Count
    {
        get
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
            return db.PatientReadings.Count();
        }
    }

    public IEnumerable<PatientReadingEntity> GetByDevice(string deviceId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
        return db.PatientReadings.Where(r => r.DeviceId!.ToLower() == deviceId.ToLower()).OrderByDescending(r => r.ReceivedAt).Take(100).ToList();
    }
}
