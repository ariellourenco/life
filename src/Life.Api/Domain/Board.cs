namespace Life.Api.Domain;

/// <summary>
/// Represents a board of <see cref="Cell"/>s in the Conway's Game of Life.
/// </summary>
internal sealed class Board
{
    private readonly Cell[,] _board;
    private readonly int _columns;
    private readonly int _rows;

    /// <summary>
    /// Initializes a new instance of the <see cref="Board"/> class.
    /// </summary>
    /// <param name="rows">The number of rows of the <see cref="Board"/>.</param>
    /// <param name="columns">The number of colunmns of the <see cref="Board"/>.</param>
    public Board(int rows, int columns)
    {
        _rows = rows;
        _columns = columns;
        _board = new Cell[rows, columns];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Board"/> class.
    /// </summary>
    /// <param name="states">The initial state of the board.</param>
    public Board(bool[,] states)
    {
        _rows = states.GetLength(0);
        _columns = states.GetLength(1);
        _board = new Cell[_rows, _columns];

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                SetCellState(x, y, states[y, x]);
            }
        }
    }

    /// <summary>
    /// Sets the state of a specific <see cref="Cell"/> on the game board.
    /// </summary>
    /// <param name="x">The x-coordinate of the <see cref="Cell"/>.</param>
    /// <param name="y">The y-coordinate of the <see cref="Cell"/>.</param>
    /// <param name="isAlive">The state to set for the <see cref="Cell"/>. (<see langword="true"/>for alive, <see langword="false"/> for dead).</param>
    public void SetCellState(int x, int y, bool isAlive)
    {
        if (IsValidPosition(y, x))
        {
            if (_board[y, x] == null)
            {
                _board[y, x] = new Cell(isAlive);
            }
            else
            {
                _board[y, x].SetState(isAlive);
            }
        }
    }

    /// <summary>
    /// Updates the game board to the next generation based on its current state
    /// by applying the rules of Conway's Game of Life.
    /// </summary>
    public void Update()
    {
        var board = new Cell[_rows, _columns];

        for (var y = 0; y < _rows; y++)
        {
            for (var x = 0; x < _columns; x++)
            {
                int aliveNeighbors = CountAliveNeighbors(x, y);
                bool isCurrentlyAlive = GetCellState(x, y);

                // Apply Conway's Game of Life rules
                bool shouldBeAlive =
                    (isCurrentlyAlive && (aliveNeighbors == 2 || aliveNeighbors == 3)) ||
                    (!isCurrentlyAlive && aliveNeighbors == 3);

                board[y, x] = new Cell(shouldBeAlive);
            }
        }

        UpdateInternalBoard(board);
    }

    /// <summary>
    /// Gets the current state of the game board.
    /// </summary>
    /// <returns>A 2D array of boolean values representing the state of each <see cref="Cell"/>.</returns>
    public bool[,] GetBoardState()
    {
        var state = new bool[_rows, _columns];

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                state[y, x] = _board[y, x].IsAlive;
            }
        }

        return state;
    }

    private int CountAliveNeighbors(int row, int column)
    {
        int count = 0;

        for (var y = -1; y <= 1; y++)
        {
            for (var x = -1; x <= 1; x++)
            {
                // We don't want to count the current cell itself as a neighbor.
                if (x == 0 && y == 0) continue;

                if (GetCellState(row + x, column + y))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private bool IsValidPosition(int row, int column) =>
        row >= 0 && row < _rows && column >= 0 && column < _columns;

    private bool GetCellState(int row, int column) =>
        IsValidPosition(row, column) && _board[row, column].IsAlive;

    private void UpdateInternalBoard(Cell[,] board)
    {
        // Replace old grid with the new grid
        for (var y = 0; y < _rows; y++)
        {
            for (var x = 0; x < _columns; x++)
            {
                _board[y, x] = board[y, x];
            }
        }
    }
}