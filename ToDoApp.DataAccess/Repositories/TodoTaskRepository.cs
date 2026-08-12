using Microsoft.EntityFrameworkCore;
using ToDoApp.DataAccess.Context;
using ToDoApp.Interfaces.Entities;
using ToDoApp.Interfaces.Repositories;

namespace ToDoApp.DataAccess.Repositories;

public class TodoTaskRepository : ITodoTaskRepository
{
    private readonly ToDoDbContext _context;

    public TodoTaskRepository(ToDoDbContext context)
    {
        _context = context;
    }

    public async Task<TodoTask?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.TodoTasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
    }

    public async Task<IEnumerable<TodoTask>> GetForUserAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm, Guid? categoryId, bool? isCompleted)
    {
        var query = _context.TodoTasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted);

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Title.Contains(searchTerm));
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountForUserAsync(Guid userId, string? searchTerm, Guid? categoryId, bool? isCompleted)
    {
        var query = _context.TodoTasks
            .Where(t => t.UserId == userId && !t.IsDeleted);

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Title.Contains(searchTerm));
        }

        return await query.CountAsync();
    }

    public async Task AddAsync(TodoTask entity)
    {
        _context.TodoTasks.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TodoTask entity)
    {
        _context.TodoTasks.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id, Guid userId)
    {
        var entity = await GetByIdAsync(id, userId);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
