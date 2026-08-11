using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ToDoApp.DataAccess.Context;
using ToDoApp.DataAccess.Entities;
using ToDoApp.Services.DTOs;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.Services.Implementations;

public class TodoTaskService : ITodoTaskService
{
    private readonly ToDoDbContext _context;
    private readonly IMapper _mapper;

    public TodoTaskService(ToDoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TodoTaskDto> CreateAsync(CreateTodoTaskDto dto, Guid userId)
    {
       var taskEntity  = _mapper.Map<TodoTask>(dto);
       taskEntity.UserId = userId;
       _context.TodoTasks.Add(taskEntity);
       await _context.SaveChangesAsync();

       var resultDto = _mapper.Map<TodoTaskDto>(taskEntity);
       return resultDto;
    }

    public async Task<PagedResultDto<TodoTaskDto>> GetAllForUserAsync(Guid userId, int pageNumber, int pageSize, string? searchTerm, Guid? categoryId)
    {
        var query = _context.TodoTasks
        .Include(t => t.Category)
        .Where(t => t.UserId == userId && !t.IsDeleted);

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Title.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = _mapper.Map<IEnumerable<TodoTaskDto>>(tasks);

        return new PagedResultDto<TodoTaskDto>
        {
            Items = dtoList,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> SoftDeleteAsync(Guid taskId)
    {
        var task = await _context.TodoTasks.FindAsync(taskId);
    
        if (task == null)
        {
            return false; 
        }

        task.IsDeleted = true;
        await _context.SaveChangesAsync();

        return true; 
    }
}
