using AutoMapper;
using ToDoApp.Interfaces.Entities;
using ToDoApp.Interfaces.Repositories;
using ToDoApp.Services.DTOs;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.Services.Implementations;

public class TodoTaskService : ITodoTaskService
{
    private readonly ITodoTaskRepository _todoTaskRepository;
    private readonly IMapper _mapper;

    public TodoTaskService(ITodoTaskRepository todoTaskRepository, IMapper mapper)
    {
        _todoTaskRepository = todoTaskRepository;
        _mapper = mapper;
    }

    public async Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto dto, Guid userId)
    {
        var taskEntity = _mapper.Map<TodoTask>(dto);
        taskEntity.UserId = userId;
        taskEntity.CreatedAt = DateTime.UtcNow;
        await _todoTaskRepository.AddAsync(taskEntity);

        return _mapper.Map<TodoTaskDto>(taskEntity);
    }

    public async Task<TodoTaskDto?> UpdateAsync(Guid taskId, UpdateTodoTaskDto dto, Guid userId)
    {
        var entity = await _todoTaskRepository.GetByIdAsync(taskId, userId);

        if (entity == null)
        {
            return null;
        }

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.IsCompleted = dto.IsCompleted;
        entity.CategoryId = dto.CategoryId;
        entity.Deadline = dto.Deadline;
        entity.UpdatedAt = DateTime.UtcNow;

        await _todoTaskRepository.UpdateAsync(entity);

        return _mapper.Map<TodoTaskDto>(entity);
    }

    public async Task<PagedResultDto<TodoTaskDto>> GetAllForUserAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm, Guid? categoryId)
    {
        var totalCount = await _todoTaskRepository.CountForUserAsync(userId, searchTerm, categoryId);

        var tasks = await _todoTaskRepository.GetForUserAsync(userId, pageNumber, pageSize, searchTerm, categoryId);

        var dtoList = _mapper.Map<IEnumerable<TodoTaskDto>>(tasks);

        return new PagedResultDto<TodoTaskDto>
        {
            Items = dtoList,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> SoftDeleteAsync(Guid taskId, Guid userId)
    {
        var task = await _todoTaskRepository.GetByIdAsync(taskId, userId);

        if (task == null)
        {
            return false;
        }

        await _todoTaskRepository.SoftDeleteAsync(taskId, userId);

        return true;
    }
}
