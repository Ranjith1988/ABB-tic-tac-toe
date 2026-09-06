using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Services;

public interface IComputerMoveStrategy
{
    /// <summary>Returns a valid board index (0-8) for the computer's O move.</summary>
    int ChooseMove(IReadOnlyList<Player?> board);
}
