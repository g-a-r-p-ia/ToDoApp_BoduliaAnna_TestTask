using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ToDoApp.DataAccess.Context;
using ToDoApp.DataAccess.Entities;
using ToDoApp.Services.DTOs;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ToDoDbContext _context;
    private readonly IMapper _mapper;

    public CategoryService(ToDoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, Guid userId)
    {
        var categoryEntity = _mapper.Map<Category>(dto);
        categoryEntity.UserId = userId;
        _context.Categories.Add(categoryEntity);
        await _context.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(categoryEntity);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllForUserAsync(Guid userId)
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }
}
