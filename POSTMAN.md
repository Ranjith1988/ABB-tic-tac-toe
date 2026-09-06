# Manual API smoke test

1. `POST http://localhost:5000/api/games` with `{ "mode": "TwoPlayer" }`.
2. Copy `gameId`.
3. `POST /api/games/{gameId}/moves` with `{ "player": "X", "row": 0, "column": 0 }`.
4. Alternate X/O and verify `currentPlayer`, `moveHistory`, `status` and `winningCells`.
5. Call `POST /api/games/{gameId}/undo` and verify the latest move is removed.
6. Call `POST /api/games/{gameId}/reset` and verify the board/history reset while scoreboard remains unchanged.
7. Create a `Computer` game and verify one X request returns both X and O moves when the game remains in progress.
8. Call `GET /api/scoreboard` and `POST /api/scoreboard/reset`.
