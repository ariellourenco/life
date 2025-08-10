namespace Life.UnitTests;

public sealed class GameTests
{
    private const int PlayerId = 27;

    [Fact]
    public void Constructor_InitializesGameWithCorrectIdAndBoard()
    {
        // Arrange
        var id = Guid.NewGuid();
        var boardState = new bool[5, 5];

        // Act
        var game = new Game(id, PlayerId, boardState);

        // Assert
        Assert.Equal(id, game.Id);
        Assert.Equal(boardState, game.RawBoard);
    }

    [Fact]
    public void NextGeneration_AdvancesToNextGeneration()
    {
        // Arrange
        var initialState = new[,]
        {
            { false, true, false },
            { false, true, false },
            { false, true, false }
        };

        var game = new Game(Guid.NewGuid(), PlayerId, initialState);

        // Act
        game.NextGeneration();
        var newState = game.RawBoard;

        // Assert
        var expectedState = new[,]
        {
            { false, true, false },
            { false, true, false },
            { false, true, false }
        };

        Assert.Equal(expectedState, newState);
    }

    [Fact]
    public void IsFinalState_ReturnsTrueWhenStateIsStable()
    {
        // Arrange
        var initialState = new bool[,]
        {
            { false, false, false },
            { true, true, true },
            { false, false, false }
        };
        var game = new Game(Guid.NewGuid(), PlayerId, initialState);

        // Act
        game.NextGeneration();
        var isFinalState = game.IsFinalState();

        // Assert
        Assert.True(isFinalState);
    }

    [Fact]
    public void IsFinalState_ReturnsFalseWhenStateIsNotStable()
    {
        // Arrange
        var initialState = new[,]
        {
            { true, true, false },
            { false, true, false },
            { false, true, false }
        };
        var game = new Game(Guid.NewGuid(), PlayerId, initialState);

        // Act
        game.NextGeneration();
        var isFinalState = game.IsFinalState();

        // Assert
        Assert.False(isFinalState);
    }
}
