using System.Security.Claims;

namespace Life.Api.Authorization;

/// <summary>
/// Represents the current authenticated user.
/// </summary>
internal sealed class CurrentUser
{
    /// <summary>
    /// Gets the user ID for the current request.
    /// </summary>
    /// <remarks>
    /// The authorization server application is responsible for validating this value ensuring it identifies a registered user.
    /// If the user is not authenticated or <see cref="ClaimTypes.NameIdentifier"/> is not an integer,
    /// this property returns -1.
    /// </remarks>
    public int Id => int.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier)!, out int id)
        ? id : -1;

    /// <summary>
    /// Gets or sets the security principal.
    /// </summary>
    public ClaimsPrincipal Principal { get; set; } = default!;

    /// <summary>
    /// Gets or sets the current gamer.
    /// </summary>
    public Gamer? User { get; set; }
}
