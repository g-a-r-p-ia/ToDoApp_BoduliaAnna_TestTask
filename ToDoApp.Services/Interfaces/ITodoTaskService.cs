using ToDoApp.Services.DTOs;

namespace ToDoApp.Services.Interfaces;

public interface ITodoTaskService
{
    Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto dto, Guid userId);
    Task<TodoTaskDto?> UpdateAsync(Guid taskId, UpdateTodoTaskDto dto, Guid userId);
    Task<PagedResultDto<TodoTaskDto>> GetAllForUserAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm, Guid? categoryId, bool? isCompleted);
    Task<bool> SoftDeleteAsync(Guid taskId, Guid userId);
}
