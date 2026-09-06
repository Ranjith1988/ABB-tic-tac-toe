using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Controllers;
using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Tests;

public sealed class ControllerTests
{
    [Fact]
    public void GamesController_DelegatesAllActions()
    {
        var service = new StubGameService();
        var controller = new GamesController(service);

        var created = controller.Create(new CreateGameRequest(GameMode.TwoPlayer));
        var fetched = controller.Get(service.GameId);
        var moved = controller.Move(service.GameId, new MoveRequest(Player.X, 0, 0));
        var undone = controller.Undo(service.GameId);
        var reset = controller.Reset(service.GameId);

        Assert.IsType<CreatedAtActionResult>(created.Result);
        Assert.Equal(service.GameId, ((MoveResponse)((CreatedAtActionResult)created.Result!).Value!).GameId);
        Assert.IsType<OkObjectResult>(fetched.Result);
        Assert.IsType<OkObjectResult>(moved.Result);
        Assert.IsType<OkObjectResult>(undone.Result);
        Assert.IsType<OkObjectResult>(reset.Result);
    }

    [Fact]
    public void ScoreboardController_DelegatesGetAndReset()
    {
        var service = new StubGameService();
        var controller = new ScoreboardController(service);

        var result = controller.Get();
        var reset = controller.Reset();

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<NoContentResult>(reset);
        Assert.True(service.ScoreboardWasReset);
    }

    private sealed class StubGameService : IGameService
    {
        public Guid GameId { get; } = Guid.NewGuid();
        public bool ScoreboardWasReset { get; private set; }

        public MoveResponse CreateGame(GameMode mode) => Response(mode);
        public MoveResponse GetGame(Guid id) => Response(GameMode.TwoPlayer);
        public MoveResponse MakeMove(Guid id, MoveRequest request) => Response(GameMode.TwoPlayer);
        public MoveResponse Undo(Guid id) => Response(GameMode.TwoPlayer);
        public MoveResponse Reset(Guid id) => Response(GameMode.TwoPlayer);
        public ScoreboardResponse GetScoreboard() => new(1, 2, 3);
        public void ResetScoreboard() => ScoreboardWasReset = true;

        private MoveResponse Response(GameMode mode) => new(GameId, new Player?[9], Player.X, mode,
            GameStatus.InProgress, null, [], [], new Scoreboard());
    }
}