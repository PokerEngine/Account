using Application.Command;
using Application.Query;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Controller;

[ApiController]
[Route("api/account")]
[Produces("application/json")]
public class AccountController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher
) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RegisterAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterAccount([FromBody] RegisterAccountCommand command)
    {
        var response = await commandDispatcher.DispatchAsync<RegisterAccountCommand, RegisterAccountResponse>(command);
        return CreatedAtAction(nameof(GetAccountDetail), new { uid = response.Uid }, response);
    }

    [HttpGet("{uid:guid}")]
    [ProducesResponseType(typeof(GetAccountDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccountDetail(Guid uid)
    {
        var query = new GetAccountDetailQuery { Uid = uid };
        var response = await queryDispatcher.DispatchAsync<GetAccountDetailQuery, GetAccountDetailResponse>(query);
        return Ok(response);
    }
}
