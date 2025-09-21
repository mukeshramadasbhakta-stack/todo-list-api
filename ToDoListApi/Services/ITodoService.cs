using ToDoListApi.Dtos;

namespace ToDoListApi.Services;

public interface ITodoService
{
  public Task<List<TodoDto>> GetAllAsync(CancellationToken cancellationToken);
  public Task<TodoDto> GetAsync(Guid id, CancellationToken cancellationToken);
  public Task<Guid> UpsertAsync(TodoDto todoDto, CancellationToken cancellationToken);
  public Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
