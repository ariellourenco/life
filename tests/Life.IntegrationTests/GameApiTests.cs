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
    public async Task CanUploadGameOfLifeBoard()
    {
        // Arrange
        var rows = 5;
        var columns = 5;
        var board = new bool[rows, columns];
        using var context = _fixture.CreateDbContext();

        // Act
        var response = await _client.PostAsJsonAsync("upload", board, SharedFixture.JsonOptions);

        response.EnsureSuccessStatusCode();

        var id = await response.Content.ReadFromJsonAsync<Guid>();

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
        var response = await _client.PostAsJsonAsync("upload", board, SharedFixture.JsonOptions);

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
        var response = await _client.PostAsJsonAsync("upload", board, SharedFixture.JsonOptions);

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
        var response = await _client.PostAsJsonAsync("upload", board, SharedFixture.JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnNextGeneration()
    {
        // Arrange
        var id = Guid.NewGuid();

        var initialState = new bool[,]
        {
            { false, true, false },
            { false, true, false },
            { false, true, false }
        };

        // The next state of the board remains the same because each live cell has exactly
        // two live neighbors, and no dead cell has exactly three live neighbors.
        var expectedState = new bool[,]
        {
            { false, true, false },
            { false, true, false },
            { false, true, false }
        };

        using var context = _fixture.CreateDbContext();
        context.Games.Add(new Game(id, initialState));

        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"{id}/next");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<bool[,]>(SharedFixture.JsonOptions);
        Assert.Equal(expectedState, result);
    }

    [Fact]
    public async Task Next_ReturnsNotFound_WhenGameDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        using var context = _fixture.CreateDbContext();

        // Act
        var response = await _client.GetAsync($"{id}/next");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnStateAfterSpecifiedNumberOfGenerations()
    {
        // Arrange
        var id = Guid.NewGuid();
        var generation = 5;

        var initialState = new bool[,]
        {
            { true, false, false },
            { false, true, false },
            { false, false, true }
        };

        var expectedState = new bool[,]
        {
            { false, false, false },
            { false, false, false },
            { false, false, false }
        };

        using var context = _fixture.CreateDbContext();
        context.Games.Add(new Game(id, initialState));

        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"{id}/next/{generation}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<bool[,]>(SharedFixture.JsonOptions);
        Assert.Equal(expectedState, result);
    }

    [Fact]
    public async Task ShouldReturnTheBoardFinalState()
    {
        // Arrange
        var id = Guid.NewGuid();

        var initialState = new bool[,]
        {
            { true, false, false },
            { false, true, false },
            { false, false, true }
        };

        var expectedState = new bool[,]
        {
            { false, false, false },
            { false, false, false },
            { false, false, false }
        };

        using var context = _fixture.CreateDbContext();
        context.Games.Add(new Game(id, initialState));

        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"{id}/final");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<bool[,]>(SharedFixture.JsonOptions);
        Assert.Equal(expectedState, result);
    }

        [Fact]
    public async Task ShouldReturn422UnprocessableEntity_WhenBoardCannotReachStability()
    {
        // Arrange
        var id = Guid.NewGuid();

        var initialState = new bool[,]
        {
            {true, false, true, false, true},
            {false, true, false, true, false},
            {true, false, true, false, true},
            {false, true, false, true, false},
            {true, false, true, false, true}
        };

        using var context = _fixture.CreateDbContext();
        context.Games.Add(new Game(id, initialState));

        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"{id}/final");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}