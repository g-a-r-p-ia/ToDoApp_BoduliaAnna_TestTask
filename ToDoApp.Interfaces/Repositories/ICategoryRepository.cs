using ToDoApp.Interfaces.Entities;

namespace ToDoApp.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetForUserAsync(Guid userId);
    Task AddAsync(Category entity);
}
