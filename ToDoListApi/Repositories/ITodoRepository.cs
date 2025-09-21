using ToDoListApi.Models;

namespace ToDoListApi.Repositories;

public interface ITodoRepository
{
  public Task<List<Todo>> GetAllAsync(CancellationToken cancellationToken);
  public Task<Todo> GetAsync(Guid id, CancellationToken cancellationToken);
  public Task<Guid> UpsertAsync(Todo todo, CancellationToken cancellationToken);
  public Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
