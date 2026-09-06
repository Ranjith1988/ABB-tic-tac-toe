namespace TicTacToe.Api.Domain;

public enum Player
{
    X,
    O
}

public enum GameMode
{
    TwoPlayer,
    Computer
}

public enum GameStatus
{
    InProgress,
    Won,
    Draw
}

public sealed record Move(Player Player, int Row, int Column, int MoveNumber);

public sealed class GameSession
{
    public required Guid Id { get; init; }
    public GameMode Mode { get; set; }
    public Player CurrentPlayer { get; set; } = Player.X;
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player? Winner { get; set; }
    public List<int> WinningCells { get; } = [];
    public Player?[] Board { get; } = new Player?[9];
    public List<Move> Moves { get; } = [];
    public bool ScoreApplied { get; set; }
}

public sealed class Scoreboard
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}
