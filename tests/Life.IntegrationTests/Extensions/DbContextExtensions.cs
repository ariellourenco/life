using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Life.IntegrationTests.Extensions;

/// <summary>
/// Provides extension methods for configuring <see cref="DbContextOptions{TContext}"/>
/// in the service collection.
/// </summary>
internal static class DbContextExtensions
{
    /// <summary>
    /// Adds the specified <see cref="DbContextOptions{TContext}"/> to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The target <see cref="DbContext"/> type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add service to.</param>
    /// <param name="configure">The action used to configure the <see cref="DbContextOptionsBuilder{TContext}"/>.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> to chain configuration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddDbContextOptions<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder<TContext>> configure)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        // Remove any existing DbContextOptions<TContext> registrated.
        // We want to override the settings and calling the AddDbContext<TContext> method will noop.
        services.RemoveAll(typeof(DbContextOptions<TContext>));

        // Add the DbContextOptions<TContext>
        var builder = new DbContextOptionsBuilder<TContext>();
        configure(builder);

        // The untyped version just calls the typed one
        services.AddSingleton(builder.Options);
        services.AddSingleton<DbContextOptions>(service => service.GetRequiredService<DbContextOptions<TContext>>());

        return services;
    }
}