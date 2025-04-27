using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Life.Api.Endpoints;

internal static class GameEnpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/game").WithTags("Game");

        group.MapPost("/start", StartGameAsync)
            .WithName("Start")
            .WithSummary("Starts a new Conway's Game of Life")
            .WithDescription("Starts a new Conway's Game of Life with the provided initial board state.");

        return group;
    }

    [ProducesResponseType<Guid>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    private static async Task<Results<Ok<Guid>, BadRequest<ProblemDetails>>> StartGameAsync(
        [FromServices] GameDbContext context,
        [FromServices] IOptions<GameSettings> options,
        [Description("The initial state of Conway's Game of Life board.")] bool[,] board,
        CancellationToken cancellationToken)
    {
        int rows = board.GetLength(0);
        int columns = board.GetLength(1);

        if (rows == 0 || rows > options.Value.MaxBoardSize || columns == 0 || columns > options.Value.MaxBoardSize)
        {
            return TypedResults.BadRequest<ProblemDetails>(new()
            {
                Detail = $@"Invalid board state. The board must have at least one cell and at most {100}x{100} cells."
            });
        }

        var game = new Game(Guid.NewGuid(), board);

        context.Games.Add(game);
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(game.Id);
    }
}