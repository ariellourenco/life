# Conway's Game of Life

[![.NET](https://github.com/ariellourenco/life/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ariellourenco/life/actions/workflows/dotnet.yml)

This repository contains the implementation of [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) using ASP.NET Core. It exposes the game logic via HTTP endpoints which allows clients to interact with the simulation programmatically, such as retrieving the current grid state or advancing to the next generation.

## Assumptions

The implementation makes a few assumptions:

1. The max board size is 100x100.
2. Every cell outside the board is dead.

## Features

- Upload a new board state.
- Advance the board to the next generation.
- Retrieve the state of the board after a specified number of generations.
- Retrieve the final state of the board.

## Running the Project

To run the project locally, follow these steps:

1. Clone the repository:

```bash
git clone https://github.com/ariellourenco/life.git
cd life
```

1. Install the **dotnet-ef** tool: `dotnet tool install dotnet-ef -g`
1. Navigate to the `life/src/Life.Api/Data/` folder.
    1. Run `mkdir Database` to create the local database folder.
    1. Run `dotnet ef database update` to create the database.
1. Learn more about [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

## Running Tests

To run the unit tests, use the following command:

```bash
dotnet test
```

## Configuration

The application can be configured using the `appsettings.json` file. The following settings are available:

- `MaxBoardSize`: The maximum size of the board.
- `MaxAttempts`: The maximum number of attempts to reach a final state.