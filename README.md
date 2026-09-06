# Tic Tac Toe — Angular + .NET

A production-quality local implementation of the supplied Tic Tac Toe exercise. The Angular frontend is a thin UI client; the .NET Web API is the source of truth for game state, validation, move history, game status, computer moves, and the session scoreboard.

## Requirements covered

- 3×3 board, X/O turns and invalid-move validation
- Row, column and diagonal win detection
- Winning-cell highlighting and completed-game lock
- Draw detection
- Move history with move number, player and row/column
- Two-player and computer modes
- Deterministic computer priority: win → block → center → corner → first free cell
- Mode-specific undo
- Session scoreboard and independent scoreboard reset
- REST API between Angular and .NET
- Backend unit tests for game rules/state transitions
- Frontend API/component tests
- Swagger/OpenAPI and health endpoint
- Centralized API error handling and strict TypeScript/C# compiler settings

These capabilities map directly to the supplied problem statement. fileciteturn0file0L17-L45

## Architecture

```text
Browser
  |
  | REST/JSON
  v
Angular 20 + TypeScript
  |
  | HTTP
  v
.NET 8 Web API
  |
  +-- GameService -------- Game rules / state transitions
  +-- BasicComputerMoveStrategy
  +-- InMemoryGameStore --- game sessions + scoreboard
```

The backend owns the state, as required by the clarification. fileciteturn0file0L186-L189

## Technology stack

- Frontend: Angular 20, TypeScript, RxJS, standalone components, SCSS
- Backend: .NET 8 Web API, C#
- API: REST/JSON
- Storage: thread-safe in-memory store, explicitly permitted by the exercise
- API documentation: Swagger/OpenAPI
- Testing: xUnit + Angular/Jasmine/Karma

## Run locally

### Backend

Prerequisite: .NET 8 SDK.

```bash
cd backend
dotnet restore
dotnet run --project src/TicTacToe.Api/TicTacToe.Api.csproj --launch-profile http
```

API: `http://localhost:5000`
Swagger: `http://localhost:5000/swagger`
Health: `http://localhost:5000/health`

### Frontend

Prerequisite: Node.js 20+ and npm.

```bash
cd frontend
npm install
npm start
```

Open `http://localhost:4200`.

The development API URL is configured in `frontend/src/environments/environment.ts`; production uses `/api`.

## API contract

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/games` | Create game session |
| GET | `/api/games/{id}` | Get current game |
| POST | `/api/games/{id}/moves` | Submit player move; in computer mode this also performs the computer response atomically |
| POST | `/api/games/{id}/undo` | Undo according to selected mode |
| POST | `/api/games/{id}/reset` | Reset current game, preserving scoreboard |
| GET | `/api/scoreboard` | Get session scoreboard |
| POST | `/api/scoreboard/reset` | Reset scoreboard |

### Create game

```json
POST /api/games
{
  "mode": "TwoPlayer"
}
```

Valid modes: `TwoPlayer`, `Computer`.

### Move

```json
POST /api/games/{id}/moves
{
  "player": "X",
  "row": 0,
  "column": 2
}
```

Rows/columns are zero-based in the API and displayed as one-based positions in the UI.

### Game state response

The response contains game ID, board, current player, mode, status, winner, winning cells, move history and scoreboard, matching the requested response contract. fileciteturn0file0L142-L153

## Undo design decision

This implementation chooses **Option A: disable Undo after completion**. This keeps the final scoreboard immutable for a completed game and avoids retroactive score changes. The UI disables Undo for completed games and the API rejects it as `UNDO_NOT_ALLOWED`. This is explicitly permitted by the exercise. fileciteturn0file0L190-L198

- Two-player: removes exactly one move.
- Computer mode: removes the human X move and the immediately generated O move as one pair.
- A fresh game starts with X.

## Computer strategy

The computer is O and the human is X. The strategy is deterministic and follows the required priority exactly: win if possible, otherwise block X, then center, then a corner, then the first available cell. fileciteturn0file0L111-L127

The computer response is generated inside the same backend request as the human move, so the frontend never has to implement game rules or trust client-side computer logic.

## Testing

### Backend

```bash
cd backend
dotnet test
```

Tests cover valid/invalid moves, turn switching, row/column/diagonal wins, draw, reset, both undo modes, scoreboard updates, computer move selection, and moves after completion. These correspond to the testing expectations in the exercise. fileciteturn0file0L199-L217

### Frontend

```bash
cd frontend
npm test
```

Frontend tests cover REST request construction, API error mapping and basic board rendering.

## Engineering standards

- Strict TypeScript and Angular template checking
- Nullable reference types and warnings-as-errors in C#
- Dependency injection and interface-driven backend services
- Centralized exception-to-HTTP error mapping
- No game-rule logic duplicated in the frontend
- Per-game locking to make state transitions atomic under concurrent requests
- Immutable response projections so clients cannot mutate server state
- CORS restricted to the local Angular origin by default
- Swagger/OpenAPI for API review
- Health endpoint for operational smoke testing
- Responsive, keyboard-accessible UI with visible focus states and ARIA labels

## AI-assisted development notes

The exercise explicitly permits AI-assisted development and asks the candidate to explain prompts, generated code, manual changes, reviews, assumptions and trade-offs. fileciteturn0file0L218-L228

Suggested review narrative:

1. Convert each functional requirement into a backend state transition and acceptance test.
2. Keep the backend authoritative; the Angular app only renders API state and sends user intent.
3. Isolate the computer strategy behind `IComputerMoveStrategy` so it can be unit tested independently.
4. Choose Option A for post-completion undo to keep scoreboard semantics simple and auditable.
5. Review concurrency, invalid inputs, completed-game behavior and score idempotency manually.

## Assumptions and limitations

- In-memory storage is used because the problem statement explicitly allows it. State is lost when the API process restarts. A production deployment with persistence would replace `InMemoryGameStore` with SQLite/EF Core or another durable store.
- There is no authentication because the exercise is a local browser application and does not require user accounts.
- Scoreboard is process/session scoped, not a global multi-instance leaderboard.
- The requested basic computer strategy is deterministic rather than minimax/AI-optimal.
- Undo after completion is disabled by design, per Option A.

## Future improvements

- SQLite/EF Core persistence
- Distributed state/leaderboard for multi-instance deployments
- Authentication and per-user game history
- SignalR for multiplayer synchronization
- Contract/integration tests using `WebApplicationFactory`
- Structured logging/metrics and OpenTelemetry
- CI quality gates for build, test, coverage and dependency scanning
- Stronger computer opponent using minimax

## Submission checklist

The repository contains the Angular source, .NET source, tests, README, API contract and setup instructions requested by the exercise. fileciteturn0file0L243-L252
