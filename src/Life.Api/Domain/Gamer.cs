using Microsoft.AspNetCore.Identity;

namespace Life.Api.Domain;

/// <summary>
/// This is our user class for the application. It represents a player in the Game of Life.
/// It inherits from <see cref="IdentityUser{int}"/> to integrate with ASP.NET Core Identity.
/// </summary>
public sealed class Gamer : IdentityUser<int> { }

/// <summary>
/// The request used to exchange username and password details to
/// the create user and token endpoints
/// </summary>
public record GamerInfo
{
    /// <summary>
    /// The gamer's email address which acts as a user name.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The gamer's password.
    /// </summary>
    public required string Password { get; init; }
}
