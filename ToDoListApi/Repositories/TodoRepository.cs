using Microsoft.EntityFrameworkCore;
using ToDoListApi.Models;

namespace ToDoListApi.Repositories;

public class TodoRepository(AppDbContext db) : ITodoRepository
{
  public async Task<List<Todo>> GetAllAsync(CancellationToken cancellationToken)
  {
    // Return all todos ordered by Created date
    return await db.Todos
      .AsNoTracking()
      .OrderByDescending(t => t.Appointment)
      .ToListAsync(cancellationToken);
  }

  public async Task<Todo> GetAsync(Guid id, CancellationToken cancellationToken)
  {
    // Returns null! when not found
    return await db.Todos
      .AsNoTracking()
      .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)!;
  }

  public async Task<Guid> UpsertAsync(Todo todo, CancellationToken cancellationToken)
  {
    var existing = await db.Todos
      .FirstOrDefaultAsync(t => t.Id == todo.Id, cancellationToken);

    if (existing is null)
    {
      // New entity
      if (todo.Id == Guid.Empty)
      {
        todo.Id = Guid.NewGuid();
      }

      await db.Todos.AddAsync(todo, cancellationToken);
    }
    else
    {
      // Update existing entity fields
      existing.Title = todo.Title;
      existing.Appointment = todo.Appointment;
      existing.Created = todo.Created;
      existing.Updated = todo.Updated;
      db.Todos.Update(existing);
    }

    await db.SaveChangesAsync(cancellationToken);
    return todo.Id;
  }

  public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
  {
    var entity = await db.Todos
      .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    if (entity is not null)
    {
      db.Todos.Remove(entity);
      await db.SaveChangesAsync(cancellationToken);
    }
  }
}
