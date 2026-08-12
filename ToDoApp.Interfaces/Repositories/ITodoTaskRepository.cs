using ToDoApp.Interfaces.Entities;

namespace ToDoApp.Interfaces.Repositories;

public interface ITodoTaskRepository
{
    Task<TodoTask?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<TodoTask>> GetForUserAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm, Guid? categoryId);
    Task<int> CountForUserAsync(Guid userId, string? searchTerm, Guid? categoryId);
    Task AddAsync(TodoTask entity);
    Task UpdateAsync(TodoTask entity);
    Task SoftDeleteAsync(Guid id, Guid userId);
}
