using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Infrastructure;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Tests;

public sealed class GameServiceTests
{
    private static GameService CreateService() => new(new InMemoryGameStore(), new BasicComputerMoveStrategy());

    [Fact]
    public void NewGame_StartsWithEmptyBoardAndXTurn()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void ValidMove_IsRecordedAndTurnSwitches()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var result = service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));

        Assert.Equal(Player.X, result.Board[0]);
        Assert.Equal(Player.O, result.CurrentPlayer);
        Assert.Single(result.MoveHistory);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 3)]
    public void InvalidMove_OutOfRange_IsRejected(int row, int column)
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var ex = Assert.Throws<DomainException>(() =>
            service.MakeMove(game.GameId, new MoveRequest(Player.X, row, column)));

        Assert.Equal("OUT_OF_RANGE", ex.Code);
    }

    [Fact]
    public void InvalidMove_OccupiedCell_IsRejectedAndTurnDoesNotChange()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));

        var ex = Assert.Throws<DomainException>(() =>
            service.MakeMove(game.GameId, new MoveRequest(Player.O, 0, 0)));

        Assert.Equal("CELL_OCCUPIED", ex.Code);
        Assert.Equal(Player.O, service.GetGame(game.GameId).CurrentPlayer);
    }

    [Fact]
    public void WrongPlayer_IsRejected()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var ex = Assert.Throws<DomainException>(() =>
            service.MakeMove(game.GameId, new MoveRequest(Player.O, 0, 0)));

        Assert.Equal("WRONG_PLAYER", ex.Code);
    }

    [Fact]
    public void RowWin_HighlightsWinningCellsAndUpdatesScoreOnce()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 1));
        var result = service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 2));

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, result.WinningCells);
        Assert.Equal(1, result.Scoreboard.XWins);
        Assert.Throws<DomainException>(() => service.MakeMove(game.GameId, new MoveRequest(Player.O, 2, 0)));
        Assert.Equal(1, service.GetScoreboard().XWins);
    }

    [Fact]
    public void ColumnWin_IsDetected()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 0, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 1, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 1));
        var result = service.MakeMove(game.GameId, new MoveRequest(Player.X, 2, 0));

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(new[] { 0, 3, 6 }, result.WinningCells);
    }

    [Fact]
    public void DiagonalWin_IsDetected()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 0, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 1, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 0, 2));
        var result = service.MakeMove(game.GameId, new MoveRequest(Player.X, 2, 2));

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(new[] { 0, 4, 8 }, result.WinningCells);
    }

    [Fact]
    public void Draw_IsDetectedAndScoreUpdated()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        var moves = new (Player p, int r, int c)[]
        {
            (Player.X,0,0),(Player.O,0,1),(Player.X,0,2),
            (Player.O,1,1),(Player.X,1,0),(Player.O,1,2),
            (Player.X,2,1),(Player.O,2,0),(Player.X,2,2)
        };
        MoveResponse result = service.GetGame(game.GameId);
        foreach (var move in moves)
            result = service.MakeMove(game.GameId, new MoveRequest(move.p, move.r, move.c));

        Assert.Equal(GameStatus.Draw, result.Status);
        Assert.Null(result.Winner);
        Assert.Equal(1, result.Scoreboard.Draws);
    }

    [Fact]
    public void ResetGame_ClearsBoardHistoryAndStatusButKeepsScoreboard()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 2));
        var before = service.GetScoreboard();

        var result = service.Reset(game.GameId);

        Assert.All(result.Board, cell => Assert.Null(cell));
        Assert.Empty(result.MoveHistory);
        Assert.Equal(Player.X, result.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, result.Status);
        Assert.Equal(before.XWins, result.Scoreboard.XWins);
    }

    [Fact]
    public void UndoTwoPlayer_RemovesOneMoveAndRestoresTurn()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 1));

        var result = service.Undo(game.GameId);

        Assert.Single(result.MoveHistory);
        Assert.Null(result.Board[4]);
        Assert.Equal(Player.O, result.CurrentPlayer);
    }

    [Fact]
    public void UndoComputer_RemovesHumanAndComputerPair()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.Computer);
        var result = service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));

        Assert.Equal(2, result.MoveHistory.Count);
        result = service.Undo(game.GameId);

        Assert.Empty(result.MoveHistory);
        Assert.All(result.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, result.CurrentPlayer);
    }

    [Fact]
    public void Computer_TakesCenterWhenAvailable()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.Computer);

        var result = service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));

        Assert.Equal(Player.O, result.Board[4]);
    }

    [Fact]
    public void Computer_BlocksImmediateXWin()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.Computer);
        // X(0,0), O(center), X(0,1) => O must block (0,2).
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 1));
        var result = service.GetGame(game.GameId);

        Assert.Equal(Player.O, result.Board[2]);
    }

    [Fact]
    public void Computer_TakesWinningMoveBeforeBlocking()
    {
        var strategy = new BasicComputerMoveStrategy();
        var board = new Player?[] { Player.O, Player.O, null, Player.X, Player.X, null, null, null, null };

        var move = strategy.ChooseMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void Computer_BlocksBeforeTakingCorner()
    {
        var strategy = new BasicComputerMoveStrategy();
        var board = new Player?[] { Player.X, Player.X, null, null, Player.O, null, null, null, null };

        var move = strategy.ChooseMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void MoveAfterCompletion_IsRejected()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 2));

        var ex = Assert.Throws<DomainException>(() =>
            service.MakeMove(game.GameId, new MoveRequest(Player.O, 2, 2)));

        Assert.Equal("GAME_COMPLETED", ex.Code);
    }

    [Fact]
    public void UndoAfterCompletion_IsRejectedByDesign()
    {
        var service = CreateService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 0));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.O, 1, 1));
        service.MakeMove(game.GameId, new MoveRequest(Player.X, 0, 2));

        var ex = Assert.Throws<DomainException>(() => service.Undo(game.GameId));
        Assert.Equal("UNDO_NOT_ALLOWED", ex.Code);
    }
}
