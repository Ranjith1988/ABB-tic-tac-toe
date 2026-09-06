using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Infrastructure;
using TicTacToe.Api.Services;
using System.Reflection;

namespace TicTacToe.Api.Tests;

public sealed class GameServiceEdgeCaseTests
{
    private static GameService CreateService() => new(new InMemoryGameStore(), new BasicComputerMoveStrategy());

    [Fact]
    public void MissingGame_IsRejected()
    {
        var service = CreateService();

        var exception = Assert.Throws<DomainException>(() => service.GetGame(Guid.NewGuid()));

        Assert.Equal("GAME_NOT_FOUND", exception.Code);
    }

    [Fact]
    public void UndoWithNoMoves_IsRejected()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var exception = Assert.Throws<DomainException>(() => service.Undo(game.GameId));

        Assert.Equal("NO_MOVES", exception.Code);
    }

    [Fact]
    public void ComputerMode_RejectsNonXRequest()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.Computer);

        var exception = Assert.Throws<DomainException>(() =>
            service.MakeMove(game.GameId, new MoveRequest(Player.O, 0, 0)));

        Assert.Equal("WRONG_PLAYER", exception.Code);
    }

    [Fact]
    public void ComputerMode_UsesInvalidPlayerGuardWhenTurnIsO()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.Computer);
        game.CurrentPlayer = Player.O;
        var service = new GameService(store, new BasicComputerMoveStrategy());

        var exception = Assert.Throws<DomainException>(() =>
            service.MakeMove(game.Id, new MoveRequest(Player.O, 0, 1)));

        Assert.Equal("INVALID_PLAYER", exception.Code);
    }

    [Fact]
    public void OWin_UpdatesOScore()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 2, 2));

        var result = service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 2));

        Assert.Equal(Player.O, result.Winner);
        Assert.Equal(1, result.Scoreboard.OWins);
    }

    [Fact]
    public void ResetScoreboard_ClearsExistingResults()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 2));

        service.ResetScoreboard();

        Assert.Equal(new ScoreboardResponse(0, 0, 0), service.GetScoreboard());
    }

    [Fact]
    public void ComputerUndoWithOneMove_RemovesThatMove()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.Computer);
        game.Board[0] = Player.X;
        game.Moves.Add(new Move(Player.X, 0, 0, 1));
        game.CurrentPlayer = Player.O;
        var service = new GameService(store, new BasicComputerMoveStrategy());

        var result = service.Undo(game.Id);

        Assert.Empty(result.MoveHistory);
        Assert.All(result.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, result.CurrentPlayer);
    }

    [Fact]
    public void ComputerUndoWithThreeMoves_RestoresOAfterPairRemoval()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.Computer);
        game.Board[0] = Player.X;
        game.Board[4] = Player.O;
        game.Board[1] = Player.X;
        game.Moves.Add(new Move(Player.X, 0, 0, 1));
        game.Moves.Add(new Move(Player.O, 1, 1, 2));
        game.Moves.Add(new Move(Player.X, 0, 1, 3));
        game.CurrentPlayer = Player.O;
        var service = new GameService(store, new BasicComputerMoveStrategy());

        var result = service.Undo(game.Id);

        Assert.Single(result.MoveHistory);
        Assert.Equal(Player.O, result.CurrentPlayer);
    }

    [Fact]
    public void ScoreIsAppliedOnlyOnce()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.TwoPlayer);
        game.Winner = Player.X;
        var service = new GameService(store, new BasicComputerMoveStrategy());
        var method = typeof(GameService).GetMethod("ApplyScoreOnce", BindingFlags.Instance | BindingFlags.NonPublic)!;

        method.Invoke(service, [game]);
        method.Invoke(service, [game]);

        Assert.Equal(1, store.GetScoreboard().XWins);
    }
}