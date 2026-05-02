using Application.Command;
using Application.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controller;

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

    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        var response = await commandDispatcher.DispatchAsync<VerifyEmailCommand, VerifyEmailResponse>(command);
        return Ok(response);
    }

    [HttpGet("{uid:guid}")]
    [Authorize(Policy = "HasNickname")]
    [ProducesResponseType(typeof(GetAccountDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccountDetail(Guid uid)
    {
        var query = new GetAccountDetailQuery { Uid = uid };
        var response = await queryDispatcher.DispatchAsync<GetAccountDetailQuery, GetAccountDetailResponse>(query);
        return Ok(response);
    }
}
