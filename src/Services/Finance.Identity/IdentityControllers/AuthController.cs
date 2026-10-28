using Finance.Identity.DTOs;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Identity;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public Task<TokenResponse> Register(RegisterRequest request)
        => auth.RegisterAsync(request.Email, request.Password);

    [HttpPost("login")]
    public Task<TokenResponse> Login(LoginRequest request)
        => auth.LoginAsync(request.Email, request.Password);

    [HttpPost("refresh")]
    public Task<TokenResponse> Refresh(RefreshRequest request)
        => auth.RefreshAsync(request.RefreshToken);

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request)
    {
        await auth.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}