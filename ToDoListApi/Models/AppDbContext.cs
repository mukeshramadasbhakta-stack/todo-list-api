using Microsoft.EntityFrameworkCore;

namespace ToDoListApi.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Todo> Todos => Set<Todo>();
}

