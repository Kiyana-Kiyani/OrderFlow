using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Admin.AssignRoleToUser;
using OrderFlow.Application.Features.Admin.GetAllRols;
using OrderFlow.Application.Features.Admin.GetAllUsers;
using OrderFlow.Application.Features.Admin.GetUserById;
using OrderFlow.Application.Features.Admin.RemoveRoleFromUser;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;

        public AdminController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("roles")]
        [ProducesResponseType(typeof(GetAllRolsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GetAllRolsResponse>> GetAllRoles(CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetAllRolsQuery(), cancellationToken);
            return Ok(response);
        }

        [HttpGet("users")]
        [ProducesResponseType(typeof(IReadOnlyList<GetAllUsersResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<GetAllUsersResponse>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetAllUsersQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("users/{userId:guid}")]
        [ProducesResponseType(typeof(GetUserByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserByIdResponse>> GetUserById([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetUserByIdQuery(userId), cancellationToken);
            return Ok(result);
        }

        [HttpPost("users/{userId:guid}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] string role, CancellationToken cancellationToken)
        {
            await _sender.Send(new AssignRoleToUserCommand(userId, role), cancellationToken);

            return NoContent();
        }

        [HttpDelete("users/{userId:guid}/roles/{role}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveRoleFromUser([FromRoute] Guid userId, [FromRoute] string role, CancellationToken cancellationToken)
        {
            await _sender.Send(new RemoveRoleFromUserCommand(userId, role), cancellationToken);

            return NoContent();
        }

    }
}
