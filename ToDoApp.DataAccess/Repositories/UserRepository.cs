using Microsoft.EntityFrameworkCore;
using ToDoApp.DataAccess.Context;
using ToDoApp.Interfaces.Entities;
using ToDoApp.Interfaces.Repositories;

namespace ToDoApp.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ToDoDbContext _context;

    public UserRepository(ToDoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User entity)
    {
        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
    }
}
