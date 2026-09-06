using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    MoveResponse CreateGame(GameMode mode);
    MoveResponse GetGame(Guid id);
    MoveResponse MakeMove(Guid id, MoveRequest request);
    MoveResponse Undo(Guid id);
    MoveResponse Reset(Guid id);
    ScoreboardResponse GetScoreboard();
    void ResetScoreboard();
}
