# life

This repository contains the implementation of [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) using ASP.NET Core. It exposes the game logic via HTTP endpoints which allows clients to interact with the simulation programmatically, such as retrieving the current grid state or advancing to the next generation.

## Assumptions

The implementation make a few assumptions:

1. The max board size is 100x100
2. Every cell outside the board is dead
