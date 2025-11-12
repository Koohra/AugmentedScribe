using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Features.Auth.Commands;
using AugmentedScribe.Application.Features.Auth.Dtos;
using AugmentedScribe.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AugmentedScribe.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand(request);

        var authResult = await _mediator.Send(command);

        return Ok(authResult);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var query = new LoginUserQuery(request);
        var authResult = await _mediator.Send(query);
        return Ok(authResult);
    }
}