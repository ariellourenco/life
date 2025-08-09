namespace Life.Api.Domain;

/// <summary>
/// Represents the Game of Life, managing the board and its state transitions.
/// </summary>
public sealed class Game
{
    private readonly Board _board;

    private bool[,]? _previousState;

    /// <summary>
    /// Gets the unique identifier of the game.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the owner of the game.
    /// </summary>
    public int PlayerId { get; private set; }

    /// <summary>
    /// Gets the current state of the game board.
    /// </summary>
    public bool[,] RawBoard { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the game.</param>
    /// <param name="playerId">The identifier of the owner of the game.</param>
    /// <param name="rawBoard">The initial state of the board.</param>
    public Game(Guid id, int playerId, bool[,] rawBoard)
    {
        Id = id;
        PlayerId = playerId;
        RawBoard = rawBoard;
        _board = new Board(rawBoard);
    }

    /// <summary>
    /// Advances the game to the next generation.
    /// </summary>
    public void NextGeneration()
    {
        _previousState = RawBoard;
        _board.Update();

        RawBoard = _board.GetBoardState();
    }

    /// <summary>
    /// Determines whether the game has reached a final state.
    /// </summary>
    /// <returns><see langword="true" /> if the game has reached a final state; otherwise, <see langword="false" />.</returns>
    public bool IsFinalState()
    {
        if (_previousState is null)
        {
            return false;
        }

        int rows = RawBoard.GetLength(0);
        int columns = RawBoard.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (RawBoard[y, x] != _previousState[y, x])
                {
                    return false;
                }
            }
        }

        return true;
    }
}
