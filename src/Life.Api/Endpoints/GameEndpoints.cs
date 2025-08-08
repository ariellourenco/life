using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Life.Api.Endpoints;

internal static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/game").WithTags("Game");

        group.MapPost("/start", StartGameAsync)
            .WithName("Start")
            .WithSummary("Starts a new Conway's Game of Life")
            .WithDescription("Starts a new Conway's Game of Life with the provided initial board state.");

        group.MapGet("{id:guid}/next", GetNextGenerationAsync)
            .WithName("GetNextState")
            .WithSummary("Gets the next generation of the board")
            .WithDescription("Gets the next generation of the board for the specified ID.");

        group.MapGet("{id:guid}/next/{generation:int}", GetGenerationAsync)
            .WithName("GetStateAfterGenerations")
            .WithSummary("Gets the state of the board after a specified number of generations")
            .WithDescription("Gets the state of the board after a specified number of generations for the specified ID.");

        group.MapGet("{id:guid}/final", GetFinalStateAsync)
            .WithName("GetFinalState")
            .WithSummary("Gets the final state of the board")
            .WithDescription("""
                Gets the final state of the board for the specified ID.
                The final state is reached when there is no change over time, such as a "block" or a "beehive". If
                The maximum number of attempts is reached without reaching a final state an error message is returned.
            """);

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
            return TypedResults.BadRequest(new ProblemDetails
            {
                Detail = $"Invalid board state. The board must have at least one cell and at most {100}x{100} cells."
            });
        }

        var game = new Game(Guid.NewGuid(), board);

        context.Games.Add(game);
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(game.Id);
    }

    [ProducesResponseType<bool[,]>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async ValueTask<Results<Ok<bool[,]>, NotFound>> GetNextGenerationAsync(
        [FromServices] GameDbContext context,
        [Description("The game identifier")] Guid id,
        CancellationToken cancellationToken) =>
        await GetGenerationAsync(context, id, 1, cancellationToken);


    [ProducesResponseType<bool[,]>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async ValueTask<Results<Ok<bool[,]>, NotFound>> GetGenerationAsync(
        [FromServices] GameDbContext context,
        [Description("The game identifier")] Guid id,
        [Description("The generation to retrieves information from")] int generation,
        CancellationToken cancellationToken)
    {
        var game = await context.Games.FindAsync([id], cancellationToken);

        if (game is null)
        {
            return TypedResults.NotFound();
        }

        for (var i = 0; i < generation; i++)
        {
            game.NextGeneration();
        }

        context.Games.Update(game);
        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(game.RawBoard);
    }

    [ProducesResponseType<bool[,]>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    private static async ValueTask<Results<Ok<bool[,]>, NotFound, UnprocessableEntity<ProblemDetails>>> GetFinalStateAsync(
        [FromServices] GameDbContext context,
        [FromServices] IOptions<GameSettings> options,
        [Description("The game identifier")] Guid id,
        CancellationToken cancellationToken)
    {
        var game = await context.Games.FindAsync([id], cancellationToken);

        if (game is null)
            return TypedResults.NotFound();

        for (var i = 0; i < options.Value.MaxNumberOfAttempts; i++)
        {
            game.NextGeneration();

            if (game.IsFinalState())
            {
                context.Games.Update(game);
                await context.SaveChangesAsync(cancellationToken);

                return TypedResults.Ok(game.RawBoard);
            }
        }

        return TypedResults.UnprocessableEntity(new ProblemDetails
        {
            Detail = $"The maximum number of attempts ({options.Value.MaxNumberOfAttempts}) was reached without reaching a final state."
        });
    }
}
