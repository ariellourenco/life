using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Life.Api.Controllers;

[ApiController]
[Route("[api/controller]")]
public sealed class GameController : ControllerBase
{
    private readonly GameDbContext _context;

    private readonly GameSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameController"/> class.
    /// </summary>
    /// <param name="context">The <see cref="GameDbContext"/>.</param>
    /// <param name="options">The options to be used by <see cref="GameController"/>.</param></param>
    public GameController(GameDbContext context, IOptions<GameSettings> options)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Uploads a new board state and returns the ID of the board.
    /// </summary>
    /// <param name="board">The initial state of the board.</param>
    /// <response code="200">Game of Life created.</response>
    /// <response code="400">Invalid board state.</response>
    /// <returns>The ID of the created board.</returns>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<Guid>> UploadBoardState([FromBody] bool[,] board, CancellationToken cancellationToken)
    {
            int rows = board.GetLength(0);
            int columns = board.GetLength(1);

            if (rows == 0 || rows > _settings.MaxBoardSize || columns == 0 || columns > _settings.MaxBoardSize)
            {
                return BadRequest($@"Invalid board state. The board must have at least one cell and at most
                    {100}x{100} cells.");
            }

            var game = new Game(Guid.NewGuid(), board);

            _context.Games.Add(game);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(game.Id);
    }

    /// <summary>
    /// Gets the next state for the board.
    /// </summary>
    /// <param name="id">The ID of the board.</param>
    /// <response code="200">Game of Life updated successfully.</response>
    /// <response code="404">The specified board was not found.</response>
    /// <returns>The next state of the board.</returns>
    [HttpGet("{id}/next")]
    [ProducesResponseType(typeof(bool[,]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<bool[,]>> GetNextState(Guid id, CancellationToken cancellationToken)
    {
        var game = await _context.Games.FindAsync(new object?[] { id }, cancellationToken);

        if (game is null)
        {
            return NotFound();
        }

        game.NextGeneration();
        _context.Games.Update(game);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(game.RawBoard);
    }

    /// <summary>
    /// Gets the state of the board after a specified number of generations.
    /// </summary>
    /// <param name="id">The ID of the board.</param>
    /// <param name="generations">The number of generations to advance.</param>
    /// <response code="200">Game of Life updated successfully.</response>
    /// <response code="404">The specified board was not found.</response>
    /// <returns>The state of the board after the specified number of generations.</returns>
    [HttpGet("{id}/next/{generations:int}")]
    [ProducesResponseType(typeof(bool[,]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<bool[,]>> GetStateAfterGenerations(Guid id, int generations, CancellationToken cancellationToken)
    {
        var game = await _context.Games.FindAsync(new object?[] { id }, cancellationToken);

        if (game is null)
        {
            return NotFound();
        }

        for (var i = 0; i < generations; i++)
        {
            game.NextGeneration();
        }

        _context.Games.Update(game);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(game.RawBoard);
    }

    /// <summary>
    /// Gets the final stage of the board.
    /// </summary>
    /// <remarks>
    /// The final stage is reached when there is no change over time, such as a "block" or a "beehive."
    /// Once a pattern reaches a stable state, it remains unchanged indefinitely.
    /// </remarks>
    /// <param name="id">The ID of the board.</param>
    /// <response code="200">Game of Life reached the stability successfully.</response>
    /// <response code="404">The specified board was not found.</response>
    /// <response code="422">The maximum number of attempts was reached without reaching a final state.</response>
    /// <returns>The final state of the board.</returns>
    [HttpGet("{id}/final")]
    [ProducesResponseType(typeof(bool[,]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<bool[,]>> GetFinalState(Guid id, CancellationToken cancellationToken)
    {
        var game = await _context.Games.FindAsync(new object?[] { id }, cancellationToken);

        if (game is null)
        {
            return NotFound();
        }

        for (var i = 0; i < _settings.MaxNumberOfAttempts; i++)
        {
            game.NextGeneration();

            if (game.IsFinalState())
            {
                _context.Games.Update(game);
                await _context.SaveChangesAsync(cancellationToken);

                return Ok(game.RawBoard);
            }
        }

        return UnprocessableEntity(
            new { message = "The maximum number of attempts was reached without reaching a final state." });
    }
}
