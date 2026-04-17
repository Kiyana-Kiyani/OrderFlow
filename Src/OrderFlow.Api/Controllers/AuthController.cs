using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Auth.Login;
using OrderFlow.Application.Features.Auth.Register;


namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);
            if (!result.Succeeded)
                return BadRequest(new { Message = result.Error });

            return Ok(new RegisterUserResponse(result.UserId!.Value, result.Token!));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);
            if (!result.Succeeded) 
                return Unauthorized(new { Message  = result.Error});


            return Ok(new LoginUserResponse(result.UserId!.Value ,result.Token!));
        }
    }
}
