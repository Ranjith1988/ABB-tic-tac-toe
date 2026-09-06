using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Infrastructure;

namespace TicTacToe.Api.Services;

public sealed class GameService(IGameStore store, IComputerMoveStrategy computerMoveStrategy) : IGameService
{
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    public MoveResponse CreateGame(GameMode mode) => ToResponse(store.CreateGame(mode));

    public MoveResponse GetGame(Guid id)
    {
        var game = GetRequiredGame(id);
        lock (store.GetLock(id)) return ToResponse(game);
    }

    public MoveResponse MakeMove(Guid id, MoveRequest request)
    {
        var game = GetRequiredGame(id);
        lock (store.GetLock(id))
        {
            ValidateHumanMove(game, request);
            ApplyMove(game, request.Player, request.Row, request.Column);
            EvaluateAndAdvance(game);

            // In computer mode, X's move is atomic with the computer's response.
            // This prevents a client from observing a transient O-turn and guarantees
            // the computer cannot move after a completed game.
            if (game.Mode == GameMode.Computer && game.Status == GameStatus.InProgress)
            {
                var computerMove = computerMoveStrategy.ChooseMove(game.Board);
                ApplyMove(game, Player.O, computerMove / 3, computerMove % 3);
                EvaluateAndAdvance(game);
            }

            return ToResponse(game);
        }
    }

    public MoveResponse Undo(Guid id)
    {
        var game = GetRequiredGame(id);
        lock (store.GetLock(id))
        {
            // Clarification choice: Option A — undo is disabled after completion.
            if (game.Status != GameStatus.InProgress)
                throw new DomainException("UNDO_NOT_ALLOWED", "Undo is not available after a game is completed.");
            if (game.Moves.Count == 0)
                throw new DomainException("NO_MOVES", "There are no moves to undo.");

            var count = game.Mode == GameMode.Computer ? Math.Min(2, game.Moves.Count) : 1;
            for (var i = 0; i < count; i++)
            {
                var move = game.Moves[^1];
                game.Board[ToIndex(move.Row, move.Column)] = null;
                game.Moves.RemoveAt(game.Moves.Count - 1);
            }

            game.CurrentPlayer = Player.X;
            if (game.Mode == GameMode.TwoPlayer && game.Moves.Count % 2 == 1)
                game.CurrentPlayer = Player.O;
            else if (game.Mode == GameMode.Computer && game.Moves.Count % 2 == 1)
                game.CurrentPlayer = Player.O;

            game.Status = GameStatus.InProgress;
            game.Winner = null;
            game.WinningCells.Clear();
            return ToResponse(game);
        }
    }

    public MoveResponse Reset(Guid id)
    {
        var game = GetRequiredGame(id);
        lock (store.GetLock(id))
        {
            // Scoreboard is deliberately untouched by Reset Game.
            Array.Fill(game.Board, null);
            game.Moves.Clear();
            game.CurrentPlayer = Player.X;
            game.Status = GameStatus.InProgress;
            game.Winner = null;
            game.WinningCells.Clear();
            game.ScoreApplied = false;
            return ToResponse(game);
        }
    }

    public ScoreboardResponse GetScoreboard()
    {
        var score = store.GetScoreboard();
        return new(score.XWins, score.OWins, score.Draws);
    }

    public void ResetScoreboard() => store.ResetScoreboard();

    private void ValidateHumanMove(GameSession game, MoveRequest request)
    {
        if (game.Status != GameStatus.InProgress)
            throw new DomainException("GAME_COMPLETED", "The game is already completed.");
        if (request.Player != game.CurrentPlayer)
            throw new DomainException("WRONG_PLAYER", $"It is {game.CurrentPlayer}'s turn.");
        if (game.Mode == GameMode.Computer && request.Player != Player.X)
            throw new DomainException("INVALID_PLAYER", "Only player X can make moves in computer mode.");
        if (request.Row is < 0 or > 2 || request.Column is < 0 or > 2)
            throw new DomainException("OUT_OF_RANGE", "Row and column must be between 0 and 2.");
        if (game.Board[ToIndex(request.Row, request.Column)] is not null)
            throw new DomainException("CELL_OCCUPIED", "The selected cell is already occupied.");
    }

    private void ApplyMove(GameSession game, Player player, int row, int column)
    {
        game.Board[ToIndex(row, column)] = player;
        game.Moves.Add(new Move(player, row, column, game.Moves.Count + 1));
    }

    private void EvaluateAndAdvance(GameSession game)
    {
        var winnerLine = WinningLines.FirstOrDefault(line =>
            game.Board[line[0]] is not null &&
            game.Board[line[0]] == game.Board[line[1]] &&
            game.Board[line[1]] == game.Board[line[2]]);

        if (winnerLine is not null)
        {
            game.Status = GameStatus.Won;
            game.Winner = game.Board[winnerLine[0]];
            game.WinningCells.Clear();
            game.WinningCells.AddRange(winnerLine);
            ApplyScoreOnce(game);
            return;
        }

        if (game.Board.All(cell => cell is not null))
        {
            game.Status = GameStatus.Draw;
            game.Winner = null;
            game.WinningCells.Clear();
            ApplyScoreOnce(game);
            return;
        }

        game.CurrentPlayer = game.CurrentPlayer == Player.X ? Player.O : Player.X;
    }

    private void ApplyScoreOnce(GameSession game)
    {
        if (game.ScoreApplied) return;
        store.IncrementScore(game.Winner);
        game.ScoreApplied = true;
    }

    private GameSession GetRequiredGame(Guid id) =>
        store.GetGame(id) ?? throw new DomainException("GAME_NOT_FOUND", $"Game '{id}' was not found.");

    private MoveResponse ToResponse(GameSession game) => new(
        game.Id,
        game.Board.ToArray(),
        game.CurrentPlayer,
        game.Mode,
        game.Status,
        game.Winner,
        game.WinningCells.ToArray(),
        game.Moves.ToArray(),
        ToScoreboard());

    private Scoreboard ToScoreboard() => store.GetScoreboard();
    private static int ToIndex(int row, int column) => row * 3 + column;
}

public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
