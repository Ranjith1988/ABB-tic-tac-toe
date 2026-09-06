using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Infrastructure;

public interface IGameStore
{
    GameSession CreateGame(GameMode mode);
    GameSession? GetGame(Guid id);
    Scoreboard GetScoreboard();
    void ResetScoreboard();
    object GetLock(Guid id);
    void RemoveGame(Guid id);
    void IncrementScore(Player? winner);
}
