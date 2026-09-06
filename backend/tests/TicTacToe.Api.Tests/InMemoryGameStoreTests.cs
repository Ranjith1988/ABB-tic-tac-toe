using TicTacToe.Api.Domain;
using TicTacToe.Api.Infrastructure;

namespace TicTacToe.Api.Tests;

public sealed class InMemoryGameStoreTests
{
    [Fact]
    public void CreateAndGetGame_ReturnsSameSession()
    {
        var store = new InMemoryGameStore();

        var game = store.CreateGame(GameMode.Computer);

        Assert.Same(game, store.GetGame(game.Id));
        Assert.Equal(GameMode.Computer, game.Mode);
    }

    [Fact]
    public void ScoreboardSnapshot_IsIndependent()
    {
        var store = new InMemoryGameStore();
        store.IncrementScore(Player.X);

        var snapshot = store.GetScoreboard();
        snapshot.XWins = 99;

        Assert.Equal(1, store.GetScoreboard().XWins);
    }

    [Fact]
    public void IncrementScore_TracksBothPlayersAndDraws()
    {
        var store = new InMemoryGameStore();

        store.IncrementScore(Player.X);
        store.IncrementScore(Player.O);
        store.IncrementScore(null);

        var score = store.GetScoreboard();
        Assert.Equal(1, score.XWins);
        Assert.Equal(1, score.OWins);
        Assert.Equal(1, score.Draws);
    }

    [Fact]
    public void RemoveGame_RemovesSessionAndLockCanBeRecreated()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.TwoPlayer);
        var firstLock = store.GetLock(game.Id);

        store.RemoveGame(game.Id);

        Assert.Null(store.GetGame(game.Id));
        Assert.NotSame(firstLock, store.GetLock(game.Id));
    }
}