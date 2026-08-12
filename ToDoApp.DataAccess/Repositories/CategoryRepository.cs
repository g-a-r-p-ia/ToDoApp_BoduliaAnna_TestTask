using Microsoft.EntityFrameworkCore;
using ToDoApp.DataAccess.Context;
using ToDoApp.Interfaces.Entities;
using ToDoApp.Interfaces.Repositories;

namespace ToDoApp.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ToDoDbContext _context;

    public CategoryRepository(ToDoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetForUserAsync(Guid userId)
    {
        return await _context.Categories
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Category entity)
    {
        _context.Categories.Add(entity);
        await _context.SaveChangesAsync();
    }
}
