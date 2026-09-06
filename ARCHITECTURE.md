# Architecture and design review

## Request flow

### Two Player
1. Angular creates a game and receives the initial server state.
2. User selects a cell.
3. Angular sends `POST /api/games/{id}/moves` with player, row and column.
4. Backend validates turn, bounds, occupancy and game status.
5. Backend applies move, evaluates win/draw, updates scoreboard once if complete, and switches turn if needed.
6. Backend returns the complete authoritative state.
7. Angular replaces its local view with the returned state.

### Computer
1. X submits a valid move.
2. Backend applies and evaluates X's move.
3. If the game is still in progress, the injected computer strategy chooses O's move using the required priority.
4. Backend applies/evaluates O's move.
5. A single response returns the resulting state.

## Concurrency

Each game has a dedicated lock. State-changing operations for one game are serialized, preventing lost updates from simultaneous move/undo/reset requests. The scoreboard has a separate lock.

## Score idempotency

`ScoreApplied` belongs to the game session. Once a result is recorded, repeated reads cannot increment it again. Reset Game explicitly starts a new game state and therefore permits a future completion to create one new score.

## Error model

Domain validation throws a typed `DomainException`. Middleware converts expected domain failures to HTTP 400 with `{ code, message }`. Unexpected exceptions are logged server-side and return a generic HTTP 500 response without leaking implementation details.

## Security baseline

- CORS is limited to the Angular development origin by default.
- No stack traces are exposed by the API error contract.
- Server state is never accepted wholesale from the client.
- Client-supplied player/coordinates are always validated.

## Deliberate trade-offs

The exercise allows in-memory storage and is intended to run locally, so a database is intentionally not introduced. The service/store interfaces make persistence replaceable without changing the API or UI contract.
