using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Services.DTOs;

public class CreateTodoTaskDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public Guid CategoryId { get; set; } 
}