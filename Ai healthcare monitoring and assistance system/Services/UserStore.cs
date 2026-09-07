using BCrypt.Net;
using Ai_healthcare_monitoring_and_assistance_system.Data;

namespace Ai_healthcare_monitoring_and_assistance_system.Services;

public class UserStore
{
    private readonly IServiceProvider _serviceProvider;

    public UserStore(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public bool UserExists(string username)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
        return db.Users.Any(u => u.Username.ToLower() == username.ToLower());
    }

    public void AddUser(string username, string password)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
        db.Users.Add(new UserEntity { Username = username.ToLower(), PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
    }

    public bool ValidateUser(string username, string password)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
        var user = db.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
}
