using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Life.IntegrationTests;

public sealed class SharedFixture : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Filename=:memory:");

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null,
        Converters =
        {
            new BoolArray2DConverter()
        }
    };

    public GameDbContext CreateDbContext()
    {
        var context = Services
            .GetRequiredService<IDbContextFactory<GameDbContext>>()
            .CreateDbContext();

        context.Database.EnsureCreated();

        return context;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // This creates the SQLite in-memory database, which will persist until the connection
        // is closed at the end of the test.
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // We are using DbContextFactory for testing, therefore, we need to replace the configuration
            // for the DbContext to use a different configured database.
            services.AddDbContextFactory<GameDbContext>();
            services.AddDbContextOptions<GameDbContext>(options => options
                .ConfigureWarnings(x => x.Log(CoreEventId.ManyServiceProvidersCreatedWarning))
                .UseSqlite(_connection));
        });

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Close the connection to the in-memory database.
            _connection.Close();
        }

        base.Dispose(disposing);
    }
}
