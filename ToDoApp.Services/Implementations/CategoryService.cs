using AutoMapper;
using ToDoApp.Interfaces.Entities;
using ToDoApp.Interfaces.Repositories;
using ToDoApp.Services.DTOs;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, Guid userId)
    {
        var categoryEntity = _mapper.Map<Category>(dto);
        categoryEntity.UserId = userId;
        await _categoryRepository.AddAsync(categoryEntity);

        return _mapper.Map<CategoryDto>(categoryEntity);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllForUserAsync(Guid userId)
    {
        var categories = await _categoryRepository.GetForUserAsync(userId);

        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }
}
