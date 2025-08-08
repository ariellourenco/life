namespace Life.IntegrationTests;

public sealed class GameApiTests : IClassFixture<SharedFixture>
{
    private readonly HttpClient _client;

    private readonly SharedFixture _fixture;

    public GameApiTests(SharedFixture fixture)
    {
        _client = fixture.CreateClient();
        _client.BaseAddress = new Uri("/api/game/");
        _fixture = fixture;
    }

    [Fact]
    public async Task CanStartGameOfLife()
    {
        // Arrange
        var rows = 5;
        var columns = 5;
        var board = new bool[rows, columns];
        await using var context = _fixture.CreateDbContext();

        // Act
        var response = await _client.PostAsJsonAsync("start", board,
            SharedFixture.JsonOptions,
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var id = await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenBoardGreaterThan100Rows()
    {
        // Arrange
        var rows = 101;
        var columns = 99;
        var board = new bool[rows, columns];

        // Act
        var response = await _client.PostAsJsonAsync("start", board,
            SharedFixture.JsonOptions,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenBoardGreaterThan100Columns()
    {
        // Arrange
        var rows = 100;
        var columns = 101;
        var board = new bool[rows, columns];

        // Act
        var response = await _client.PostAsJsonAsync("start", board,
            SharedFixture.JsonOptions,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenBoardIsEmpty()
    {
        // Arrange
        var rows = 0;
        var columns = 0;
        var board = new bool[rows, columns];

        // Act
        var response = await _client.PostAsJsonAsync("start", board,
            SharedFixture.JsonOptions,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnNextGeneration()
    {
        // Arrange
        var id = Guid.NewGuid();

        var initialState = new[,]
        {
            { false, true, false },
            { false, true, false },
            { false, true, false }
        };

        // The next state of the board remains the same because each live cell has exactly
        // two live neighbors, and no dead cell has exactly three live neighbors.
        var expectedState = new[,]
        {
            { false, true, false },
            { false, true, false },
            { false, true, false }
        };

        await using var context = _fixture.CreateDbContext();
        context.Games.Add(new Game(id, initialState));

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var response = await _client.GetAsync($"{id}/next", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<bool[,]>(SharedFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(expectedState, result);
    }

    [Fact]
    public async Task Next_ReturnsNotFound_WhenGameDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        await using var context = _fixture.CreateDbContext();

        // Act
        var response = await _client.GetAsync($"{id}/next", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnStateAfterSpecifiedNumberOfGenerations()
    {
        // Arrange
        var id = Guid.NewGuid();
        var generation = 5;

        var initialState = new[,]
        {
            { true, false, false },
            { false, true, false },
            { false, false, true }
        };

        var expectedState = new[,]
        {
            { false, false, false },
            { false, false, false },
            { false, false, false }
        };

        await using var context = _fixture.CreateDbContext();
        context.Games.Add(new Game(id, initialState));

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var response = await _client.GetAsync($"{id}/next/{generation}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<bool[,]>(SharedFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(expectedState, result);
    }

    [Fact]
    public async Task ShouldReturnTheBoardFinalState()
    {
        // Arrange
        var id = Guid.NewGuid();

        var initialState = new[,]
        {
            { true, false, false },
            { false, true, false },
            { false, false, true }
        };

        var expectedState = new[,]
        {
            { false, false, false },
            { false, false, false },
            { false, false, false }
        };

        await using var context = _fixture.CreateDbContext();
        context.Games.Add(new Game(id, initialState));

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var response = await _client.GetAsync($"{id}/final", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<bool[,]>(SharedFixture.JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(expectedState, result);
    }

    [Fact]
    public async Task ShouldReturn422UnprocessableEntity_WhenBoardCannotReachStability()
    {
        // Arrange
        var id = Guid.NewGuid();

        var initialState = new[,]
        {
            {true, false, true, false, true},
            {false, true, false, true, false},
            {true, false, true, false, true},
            {false, true, false, true, false},
            {true, false, true, false, true}
        };

        await using var context = _fixture.CreateDbContext();
        context.Games.Add(new Game(id, initialState));

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var response = await _client.GetAsync($"{id}/final", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
