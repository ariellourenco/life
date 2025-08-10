using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Life.Api.Authorization;

/// <summary>
/// Extensions for the <see cref="CurrentUser"/> class.
/// </summary>
internal static class CurrentUserExtensions
{
    /// <summary>
    /// Registers the <see cref="CurrentUser"/> service in the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> where the services were registered.</returns>
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddScoped<CurrentUser>();
        services.AddScoped<IClaimsTransformation, ClaimsTransformation>();

        return services;
    }

    private sealed class ClaimsTransformation(CurrentUser user, UserManager<Gamer> manager) : IClaimsTransformation
    {
        /// <inheritdoc />
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // We're not going to transform anything. We're using this as a hook into authorization
            // to set the current user without adding custom middleware.
            user.Principal = principal;

            if (principal.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } id)
            {
                // Resolve the user manager and see if the current user is a valid user in the database
                // we do this once and store it on the current user.
                user.User = await manager.FindByIdAsync(id);
            }

            return principal;
        }
    }
}
