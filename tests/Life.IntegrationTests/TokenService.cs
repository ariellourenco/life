using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Life.IntegrationTests;

/// <summary>
/// Provides a default implementation of bearer tokens generator for testing purposes.
/// </summary>
/// <param name="manager">The <see cref="SignInManager{Gamer}"/>.</param>
/// <param name="options">The <see cref="IOptionsMonitor{BearerTokenOptions}"/> containing the options to authenticate using bearer tokens.</param>
internal sealed class TokenService(SignInManager<Gamer> manager, IOptionsMonitor<BearerTokenOptions> options)
{
    private readonly BearerTokenOptions _options = options.Get(IdentityConstants.BearerScheme);

    /// <summary>
    /// Generates a bearer token for the specified user.
    /// </summary>
    /// <param name="id">The user unique identifier.</param>
    /// <param name="username">The username for this user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    /// <returns> A <see cref="Task{string}"/> containing a opaque bearer token used to authenticate.</returns>
    public async Task<string> GenerateTokenAsync(int id, string username, CancellationToken cancellationToken)
    {
        var principal = await manager.CreateUserPrincipalAsync(
            new Gamer
            {
                Id = id,
                UserName = username,
                SecurityStamp = Guid.NewGuid().ToString()
            });

        // This is copied from https://github.com/dotnet/aspnetcore/blob/238dabc8bf7a6d9485d420db01d7942044b218ee/src/Security/Authentication/BearerToken/src/BearerTokenHandler.cs#L66
        var timeProvider = _options.TimeProvider ?? TimeProvider.System;
        var utcNow = timeProvider.GetUtcNow();

        var ticket = new AuthenticationTicket(principal,
            new AuthenticationProperties { ExpiresUtc = utcNow + _options.BearerTokenExpiration },
            $"{IdentityConstants.BearerScheme}:AccessToken");

        return _options.BearerTokenProtector.Protect(ticket);
    }
}
