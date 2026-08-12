using ToDoApp.Services.DTOs;

namespace ToDoApp.Services.Interfaces;

public interface IAuthService
{
    Task<string> LoginWithGoogleAsync(string googleToken);
    Task<string> RegisterAsync(RegisterDto dto);
    Task<string> LoginAsync(LoginDto dto);
}
