using Microsoft.EntityFrameworkCore;
using ToDoApp.DataAccess.Entities;

namespace ToDoApp.DataAccess.Context;

public class ToDoDbContext : DbContext
{
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
    {
    }

public DbSet<User> Users { get; set; }
public DbSet<Category> Categories { get; set; }
public DbSet<TodoTask> TodoTasks { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ToDoDbContext).Assembly);
    }
}
