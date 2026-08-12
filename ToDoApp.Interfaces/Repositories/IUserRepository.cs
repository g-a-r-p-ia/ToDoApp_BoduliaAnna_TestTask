using ToDoApp.Interfaces.Entities;

namespace ToDoApp.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User entity);
}
