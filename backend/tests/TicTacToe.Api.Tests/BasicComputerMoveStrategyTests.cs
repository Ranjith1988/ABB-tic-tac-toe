using TicTacToe.Api.Domain;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Tests;

public sealed class BasicComputerMoveStrategyTests
{
    private readonly BasicComputerMoveStrategy strategy = new();

    [Fact]
    public void InvalidBoardSize_IsRejected()
    {
        var exception = Assert.Throws<ArgumentException>(() => strategy.ChooseMove(new Player?[8]));

        Assert.Contains("exactly 9 cells", exception.Message);
    }

    [Fact]
    public void FullBoard_IsRejected()
    {
        var board = Enumerable.Repeat<Player?>(Player.X, 9).ToArray();

        Assert.Throws<InvalidOperationException>(() => strategy.ChooseMove(board));
    }

    [Fact]
    public void CenterIsPreferredWhenNoTacticalMoveExists()
    {
        var board = new Player?[] { Player.X, null, null, null, null, null, null, null, null };

        Assert.Equal(4, strategy.ChooseMove(board));
    }

    [Fact]
    public void FirstCornerIsPreferredAfterCenter()
    {
        var board = new Player?[] { null, Player.X, null, null, Player.O, null, null, null, null };

        Assert.Equal(0, strategy.ChooseMove(board));
    }

    [Fact]
    public void FirstFreeCellIsUsedWhenCenterAndCornersAreTaken()
    {
        var board = new Player?[] { Player.X, null, Player.O, null, Player.X, null, Player.O, null, Player.X };

        Assert.Equal(1, strategy.ChooseMove(board));
    }

    [Fact]
    public void AvailableCornerIsPreferredBeforeAnEdge()
    {
        var board = new Player?[] { null, Player.X, Player.O, Player.O, Player.X, null, Player.X, Player.O, null };

        Assert.Equal(0, strategy.ChooseMove(board));
    }
}