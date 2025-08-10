using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
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

    public HttpClient CreateClient(int id, string username) =>
        CreateDefaultClient(new Uri("/api/game/"), new NoopAuthenticationHandler(Services, id, username));

    public GameDbContext CreateDbContext()
    {
        var context = Services
            .GetRequiredService<IDbContextFactory<GameDbContext>>()
            .CreateDbContext();

        context.Database.EnsureCreated();

        return context;
    }

    public async Task CreateUserAsync(int id, string email, string? password = null)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Gamer>>();
        var gamer = await userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));

        if (gamer is null)
        {
            var newUser = new Gamer { Id = id, UserName = email };
            var result = await userManager.CreateAsync(newUser, password ?? Guid.NewGuid().ToString());

            Assert.True(result.Succeeded);
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // This creates the SQLite in-memory database, which will persist until the connection
        // is closed at the end of the test.
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            services.AddScoped<TokenService>();

            // We are using DbContextFactory for testing, therefore, we need to replace the configuration
            // for the DbContext to use a different configured database.
            services.AddDbContextFactory<GameDbContext>();
            services.AddDbContextOptions<GameDbContext>(options => options
                .ConfigureWarnings(x => x.Log(CoreEventId.ManyServiceProvidersCreatedWarning))
                .UseSqlite(_connection));

            // Lower the requirements for the tests
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
            });
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

    private sealed class NoopAuthenticationHandler(IServiceProvider provider, int id, string username) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await using var scope = provider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TokenService>();
            var token = await service.GenerateTokenAsync(id, username, cancellationToken);

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
