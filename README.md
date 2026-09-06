# Tic Tac Toe

Tic Tac Toe is a browser game with two ways to play:

- **Two Player**: take turns with another person on the same screen.
- **Play Against Computer**: play as X while the computer plays as O.

The game keeps the board, rules, move history and scoreboard on the .NET API. The Angular app is the game interface.

<img width="907" height="662" alt="image" src="https://github.com/user-attachments/assets/9b806fb1-778a-4d00-b691-b3fb9ac1e34f" />
<img width="907" height="662" alt="image" src="https://github.com/user-attachments/assets/69ac7175-7c4a-47af-bca7-8ec1de434c0f" />


## Play the Game

### Start a local game

You need Node.js 20.19 or newer and the .NET 8 SDK.

Start the API in one terminal:

```powershell
cd backend
dotnet restore
dotnet run --project src/TicTacToe.Api/TicTacToe.Api.csproj --launch-profile http
```

Start the game in a second terminal:

```powershell
cd frontend
npm install
npm start
```

Open [http://localhost:4200](http://localhost:4200) in your browser.

If PowerShell blocks the `npm.ps1` script, use `npm.cmd start` or allow local scripts for your Windows user:

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

### Choose a game mode

Use the **Game mode** menu at the top of the board:

- **Two Player** starts a local two-person game.
- **Play Against Computer** starts a game where you are X and the computer is O.

Changing the mode starts a fresh game. The session scoreboard remains available.

### Make a move

Select any empty square on the 3x3 board. The active player is shown above the board.

- X and O use different colors so the board is easy to scan.
- In computer mode, the computer responds automatically after your move.
- Completed games lock the board so no extra moves can be added.

### Read the game status

The status bar tells you whose turn it is, when the computer is thinking, and when the game has ended.

- A player wins by completing a row, column or diagonal.
- Winning squares are highlighted.
- A full board with no winner is a draw.

### Use the controls

- **Reset Game** clears the current board and starts again with X. The scoreboard is preserved.
- **Undo Last Move** removes the most recent move while the game is in progress.
  - In Two Player mode, it removes one move.
  - In Computer mode, it removes your move and the computer response together.
- **Reset** beside Scoreboard clears X wins, O wins and draws without changing the current board.

### Follow the move history

The move log shows each move in order:

| Move | Mark | Board position |
|---|---|---|
| #1 | X | Row 1, Column 1 |
| #2 | O | Row 2, Column 2 |

Rows and columns in the game display start at 1. The API uses zero-based row and column values.

## Game Rules

- X always starts.
- Players alternate turns.
- A move must target an empty square and the current player.
- A completed game cannot accept more moves.
- Undo is disabled after a win or draw so completed scoreboard results stay final.
- The computer uses a predictable strategy: win when possible, block X, choose the center, choose a corner, then choose the first free square.

## Troubleshooting

### The game controls are disabled

Make sure the API is running at [http://localhost:5000](http://localhost:5000). Check its health endpoint:

```text
http://localhost:5000/health
```

It should return:

```json
{"status":"Healthy"}
```

Then refresh the game page.

### Port 4200 is already in use

An Angular server may already be running. Open [http://localhost:4200](http://localhost:4200), or stop the existing process before starting another one.

### Port 5000 is already in use

Stop the existing API process or change the API port and update `frontend/src/environments/environment.ts` to match it.

## Developer Guide

### Project structure

```text
backend/
  src/TicTacToe.Api/       .NET REST API and game rules
  tests/                   Backend unit tests
frontend/
  src/app/                 Angular game UI and API client
.github/workflows/         Continuous integration workflow
```

### Local service URLs

| Service | URL |
|---|---|
| Game | [http://localhost:4200](http://localhost:4200) |
| API | [http://localhost:5000](http://localhost:5000) |
| Swagger | [http://localhost:5000/swagger](http://localhost:5000/swagger) |
| Health | [http://localhost:5000/health](http://localhost:5000/health) |

### API endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/games` | Create a game session |
| GET | `/api/games/{id}` | Read current game state |
| POST | `/api/games/{id}/moves` | Submit a move |
| POST | `/api/games/{id}/undo` | Undo the current game |
| POST | `/api/games/{id}/reset` | Reset the board and preserve the scoreboard |
| GET | `/api/scoreboard` | Read the session scoreboard |
| POST | `/api/scoreboard/reset` | Reset the session scoreboard |

### Run tests

Backend tests:

```powershell
cd backend
dotnet test
```

Frontend tests:

```powershell
cd frontend
npm test
```

Production frontend build:

```powershell
cd frontend
npm run build
```

The frontend CI test runs with coverage enforcement at **100% statements, branches, functions and lines**. Backend CI publishes a Coverlet coverage report alongside the 45 xUnit tests.

### Architecture notes

- Angular renders server responses and sends player intent; it does not duplicate game rules.
- The API owns validation, turn changes, win detection, computer moves and scoreboard updates.
- In-memory storage is used for this local exercise, so games and scores reset when the API process stops.
- The computer strategy is deterministic and intentionally simple rather than minimax-based.
- The UI uses responsive layout, keyboard focus states and accessible board labels.

## Production Considerations

Before deploying beyond a local demo, replace the in-memory store with durable storage, add authentication, configure a production API origin, add structured logging and metrics, and run integration tests against the deployed API.
