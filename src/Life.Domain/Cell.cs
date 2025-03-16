namespace Life.Domain;

/// <summary>
/// Represents a cell in the Conway's Game of Life.
/// </summary>
internal sealed class Cell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Cell"/> class.
    /// </summary>
    /// <param name="isAlive">Set the <see cref="Cell"/> state.</param>
    public Cell(bool isAlive) => IsAlive = isAlive;

    /// <summary>
    /// Gets a value indicating whether the <see cref="Cell"/> is alive.
    /// </summary>
    public bool IsAlive { get; private set; }

    /// <summary>
    /// Defines the state of the <see cref="Cell"/>.
    /// If <paramref name="state"/> is <see langword="true"/>, the <see cref="Cell"/> is alive; otherwise, it is dead.
    /// </summary>
    /// <param name="state">The state to set.</param>
    public void SetState(bool state) => IsAlive = state;
}
