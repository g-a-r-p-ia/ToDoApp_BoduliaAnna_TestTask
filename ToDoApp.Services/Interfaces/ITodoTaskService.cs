using ToDoApp.Services.DTOs;

namespace ToDoApp.Services.Interfaces;

public interface ITodoTaskService
{
    Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto dto, Guid userId);
    Task<PagedResultDto<TodoTaskDto>> GetAllForUserAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm, Guid? categoryId);
    Task<bool> SoftDeleteAsync(Guid taskId);
}
