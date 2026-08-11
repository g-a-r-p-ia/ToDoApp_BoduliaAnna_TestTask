namespace ToDoApp.Services.Interfaces;

public interface IAuthService
{
    Task<string> LoginWithGoogleAsync(string googleToken);
}
