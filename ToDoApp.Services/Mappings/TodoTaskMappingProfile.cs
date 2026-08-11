using AutoMapper;
using ToDoApp.DataAccess.Entities;
using ToDoApp.Services.DTOs;

namespace ToDoApp.Services.Mappings;

public class TodoTaskMappingProfile : Profile
{
    public TodoTaskMappingProfile()
    {
        CreateMap<TodoTask, TodoTaskDto>();
        
        CreateMap<CreateTodoTaskDto, TodoTask>();

        CreateMap<Category, CategoryDto>();

        CreateMap<CreateCategoryDto, Category>();
    }
}
