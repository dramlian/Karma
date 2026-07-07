using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server;

public class DatabaseInitializer
{
    private readonly ChatDbContext _db;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(ChatDbContext db, ILogger<DatabaseInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    // MySQL in Docker needs a few seconds to accept connections; retry until it's ready.
    public async Task InitializeAsync()
    {
        for (var attempt = 1; attempt <= 15; attempt++)
        {
            try
            {
                await _db.Database.MigrateAsync();
                _logger.LogInformation("Database migrated and ready.");
                return;
            }
            catch (Exception ex) when (attempt < 15)
            {
                _logger.LogWarning("Waiting for database (attempt {Attempt}/15): {Message}", attempt, ex.GetBaseException().Message);
                await Task.Delay(3000);
            }
        }
    }
}
