using ToDoApp.DataAccess.Context;
using ToDoApp.DataAccess.Entities;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.Services.Implementations;

public class TodoTaskService : ITodoTaskService
{
    private readonly ToDoDbContext _context;

    public TodoTaskService(ToDoDbContext context)
    {
        _context = context;
    }

    public Task<TodoTask> CreateAsync(TodoTask task)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TodoTask>> GetAllForUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(Guid taskId)
    {
        throw new NotImplementedException();
    }
}
