using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public sealed class ScoreboardController(IGameService gameService) : ControllerBase
{
    [HttpGet]
    public ActionResult<ScoreboardResponse> Get() => Ok(gameService.GetScoreboard());

    [HttpPost("reset")]
    public IActionResult Reset()
    {
        gameService.ResetScoreboard();
        return NoContent();
    }
}
