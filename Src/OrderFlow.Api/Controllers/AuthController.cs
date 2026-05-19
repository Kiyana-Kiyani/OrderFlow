using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Contracts.Auth;
using OrderFlow.Application.Features.Auth.Login;
using OrderFlow.Application.Features.Auth.RefreshToken;
using OrderFlow.Application.Features.Auth.Register;
using System.Net;
namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);
            if (!result.Succeeded)
                return Problem(
                    statusCode: (int)HttpStatusCode.BadRequest,
                    title: "Registration Failed",
                    detail: result.Error,
                    instance: HttpContext.Request.Path
                    );
            return Ok(new RegisterUserResponse(result.UserId!.Value, result.Token!));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);
            if (!result.Succeeded)
                return Problem(
                    statusCode: (int)HttpStatusCode.Unauthorized,
                    title: "Login Failed",
                    detail: result.Error,
                    instance: HttpContext.Request.Path
                    );


            return Ok(new LoginUserResponse(result.UserId!.Value, result.Token!, result.RefreshToken!));
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            // Assuming your MediatR RefreshTokenCommand routes directly to AuthService.RefreshTokenAsync
            var result = await _sender.Send(new RefreshTokenCommand(request.ExpiredToken, request.RefreshToken), cancellationToken);

            if (!result.Succeeded)
                return Unauthorized(result.Error);

            return Ok(new LoginUserResponse(result.UserId!.Value, result.Token!, result.RefreshToken!));
        }
    }
}
