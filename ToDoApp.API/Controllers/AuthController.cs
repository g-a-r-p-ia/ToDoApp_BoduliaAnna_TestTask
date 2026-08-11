using Microsoft.AspNetCore.Mvc;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] string googleToken)
    {
        var result = await _authService.LoginWithGoogleAsync(googleToken);
        return Ok(new { Token = result });
    }
}
