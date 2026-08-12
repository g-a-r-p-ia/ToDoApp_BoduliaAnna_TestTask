namespace ToDoApp.Interfaces.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
}
