using AutoMapper;
using ToDoApp.Interfaces.Entities;
using ToDoApp.Interfaces.Repositories;
using ToDoApp.Services.DTOs;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.Services.Implementations;

public class TodoTaskService : ITodoTaskService
{
    private readonly ITodoTaskRepository _todoTaskRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public TodoTaskService(ITodoTaskRepository todoTaskRepository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _todoTaskRepository = todoTaskRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto dto, Guid userId)
    {
        await ValidateCategoryOwnershipAsync(dto.CategoryId, userId);

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

        await ValidateCategoryOwnershipAsync(dto.CategoryId, userId);

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.IsCompleted = dto.IsCompleted;
        entity.CategoryId = dto.CategoryId;
        entity.Deadline = dto.Deadline;
        entity.UpdatedAt = DateTime.UtcNow;

        await _todoTaskRepository.UpdateAsync(entity);

        return _mapper.Map<TodoTaskDto>(entity);
    }

    public async Task<PagedResultDto<TodoTaskDto>> GetAllForUserAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm, Guid? categoryId, bool? isCompleted)
    {
        var totalCount = await _todoTaskRepository.CountForUserAsync(userId, searchTerm, categoryId, isCompleted);

        var tasks = await _todoTaskRepository.GetForUserAsync(userId, pageNumber, pageSize, searchTerm, categoryId, isCompleted);

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

    private async Task ValidateCategoryOwnershipAsync(Guid categoryId, Guid userId)
    {
        var userCategories = await _categoryRepository.GetForUserAsync(userId);

        if (userCategories.All(c => c.Id != categoryId))
        {
            throw new UnauthorizedAccessException("Invalid category.");
        }
    }
}
