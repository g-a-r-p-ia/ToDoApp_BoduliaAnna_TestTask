using Microsoft.EntityFrameworkCore;
using ToDoApp.DataAccess.Context;
using ToDoApp.Interfaces;

namespace ToDoApp.DataAccess.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ToDoDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ToDoDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        // TODO: [Your Task] Implement adding an entity to the DbSet and saving changes
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
