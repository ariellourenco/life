namespace Life.Api;

/// <summary>
/// Represents the settings for the game.
/// </summary>
public class GameSettings
{
    /// <summary>
    /// Gets or sets the maximum size of the Game of Life's board.
    /// </summary>
    public int MaxBoardSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum number of generations the game can run.
    /// </summary>
    public int MaxNumberOfAttempts { get; set; }
}
