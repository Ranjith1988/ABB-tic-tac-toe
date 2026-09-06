using System.Collections.Concurrent;
using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Infrastructure;

public sealed class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<Guid, GameSession> _games = new();
    private readonly ConcurrentDictionary<Guid, object> _locks = new();
    private readonly object _scoreboardLock = new();
    private readonly Scoreboard _scoreboard = new();

    public GameSession CreateGame(GameMode mode)
    {
        var game = new GameSession { Id = Guid.NewGuid(), Mode = mode };
        _games[game.Id] = game;
        _locks[game.Id] = new object();
        return game;
    }

    public GameSession? GetGame(Guid id) => _games.GetValueOrDefault(id);

    public Scoreboard GetScoreboard()
    {
        lock (_scoreboardLock)
        {
            return new Scoreboard
            {
                XWins = _scoreboard.XWins,
                OWins = _scoreboard.OWins,
                Draws = _scoreboard.Draws
            };
        }
    }

    public void ResetScoreboard()
    {
        lock (_scoreboardLock)
        {
            _scoreboard.XWins = 0;
            _scoreboard.OWins = 0;
            _scoreboard.Draws = 0;
        }
    }

    public object GetLock(Guid id) => _locks.GetOrAdd(id, _ => new object());

    public void RemoveGame(Guid id)
    {
        _games.TryRemove(id, out _);
        _locks.TryRemove(id, out _);
    }

    public void IncrementScore(Player? winner)
    {
        lock (_scoreboardLock)
        {
            if (winner == Player.X) _scoreboard.XWins++;
            else if (winner == Player.O) _scoreboard.OWins++;
            else _scoreboard.Draws++;
        }
    }

}
