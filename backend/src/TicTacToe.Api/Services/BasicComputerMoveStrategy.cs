using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Services;

/// <summary>
/// Deterministic strategy required by the exercise: win, block, center, corner, then any cell.
/// </summary>
public sealed class BasicComputerMoveStrategy : IComputerMoveStrategy
{
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    public int ChooseMove(IReadOnlyList<Player?> board)
    {
        if (board.Count != 9) throw new ArgumentException("A Tic Tac Toe board must contain exactly 9 cells.", nameof(board));
        if (board.All(cell => cell is not null)) throw new InvalidOperationException("No valid computer move is available.");

        var winningMove = FindTacticalMove(board, Player.O);
        if (winningMove >= 0) return winningMove;

        var blockingMove = FindTacticalMove(board, Player.X);
        if (blockingMove >= 0) return blockingMove;

        if (board[4] is null) return 4;

        foreach (var index in new[] { 0, 2, 6, 8 })
            if (board[index] is null) return index;

        return Enumerable.Range(0, 9).First(index => board[index] is null);
    }

    private static int FindTacticalMove(IReadOnlyList<Player?> board, Player player)
    {
        foreach (var index in Enumerable.Range(0, 9))
        {
            if (board[index] is not null) continue;
            var copy = board.ToArray();
            copy[index] = player;
            if (WinningLines.Any(line => copy[line[0]] == player && copy[line[1]] == player && copy[line[2]] == player))
                return index;
        }
        return -1;
    }
}
