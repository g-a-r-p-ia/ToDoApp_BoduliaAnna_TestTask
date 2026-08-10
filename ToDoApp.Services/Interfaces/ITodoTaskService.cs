using ToDoApp.DataAccess.Entities;

namespace ToDoApp.Services.Interfaces;

public interface ITodoTaskService
{
    Task<TodoTask> CreateAsync(TodoTask task);
    Task<IEnumerable<TodoTask>> GetAllForUserAsync(Guid userId);
    Task<bool> SoftDeleteAsync(Guid taskId);
}
