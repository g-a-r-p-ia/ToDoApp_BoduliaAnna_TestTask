using ToDoApp.Services.DTOs;

namespace ToDoApp.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, Guid userId);
    Task<IEnumerable<CategoryDto>> GetAllForUserAsync(Guid userId);
}
