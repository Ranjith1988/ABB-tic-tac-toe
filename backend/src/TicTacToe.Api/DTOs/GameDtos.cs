using TicTacToe.Api.Domain;

namespace TicTacToe.Api.DTOs;

public sealed record CreateGameRequest(GameMode Mode);
public sealed record MoveRequest(Player Player, int Row, int Column);

public sealed record MoveResponse(
    Guid GameId,
    Player?[] Board,
    Player CurrentPlayer,
    GameMode Mode,
    GameStatus Status,
    Player? Winner,
    IReadOnlyList<int> WinningCells,
    IReadOnlyList<Move> MoveHistory,
    Scoreboard Scoreboard);

public sealed record ScoreboardResponse(int XWins, int OWins, int Draws);

public sealed record ApiError(string Code, string Message, IDictionary<string, string[]>? Errors = null);
