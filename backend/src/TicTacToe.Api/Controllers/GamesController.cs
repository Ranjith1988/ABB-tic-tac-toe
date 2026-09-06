using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Domain;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController(IGameService gameService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(MoveResponse), StatusCodes.Status201Created)]
    public ActionResult<MoveResponse> Create(CreateGameRequest request)
    {
        var response = gameService.CreateGame(request.Mode);
        return CreatedAtAction(nameof(Get), new { id = response.GameId }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MoveResponse), StatusCodes.Status200OK)]
    public ActionResult<MoveResponse> Get(Guid id) => Ok(gameService.GetGame(id));

    [HttpPost("{id:guid}/moves")]
    [ProducesResponseType(typeof(MoveResponse), StatusCodes.Status200OK)]
    public ActionResult<MoveResponse> Move(Guid id, MoveRequest request) => Ok(gameService.MakeMove(id, request));

    [HttpPost("{id:guid}/undo")]
    public ActionResult<MoveResponse> Undo(Guid id) => Ok(gameService.Undo(id));

    [HttpPost("{id:guid}/reset")]
    public ActionResult<MoveResponse> Reset(Guid id) => Ok(gameService.Reset(id));
}
