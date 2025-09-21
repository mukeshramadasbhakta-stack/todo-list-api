using AutoMapper;
using ToDoListApi.Dtos;
using ToDoListApi.Models;
using ToDoListApi.Repositories;

namespace ToDoListApi.Services;

public class TodoService(ILogger<TodoService> logger,IMapper mapper, ITodoRepository todoRepository) : ITodoService
{
  public async Task<List<TodoDto>> GetAllAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Getting all Todos");
    var models = await todoRepository.GetAllAsync(cancellationToken);
    return models.Select(mapper.Map<TodoDto>).ToList();
  }

  public async Task<TodoDto> GetAsync(Guid id, CancellationToken cancellationToken)
  {
    if (id == Guid.Empty)
    {
       throw new ArgumentException("Id cannot be empty");
    }

    logger.LogInformation("Getting Todo with id {Id}", id);

    var model = await GetAsyncInternal(id, cancellationToken);
    return mapper.Map<TodoDto>(model);
  }

  public async Task<Guid> UpsertAsync(TodoDto todoDto, CancellationToken cancellationToken)
  {
    if (todoDto == null)
    {
      throw new ArgumentException("todoDto is null");
    }

    if (todoDto.Appointment <= DateTime.UtcNow)
    {
      throw new ArgumentException("Appointment is in the past");
    }

    logger.LogInformation("Upserting Todo record with id {Id}", todoDto.Id);
    var todo = mapper.Map<Todo>(todoDto);

    var record = await GetAsyncInternal(todo.Id, cancellationToken);
    todo.Created = record?.Created ?? DateTime.UtcNow;
    todo.Updated = DateTime.UtcNow;

    return await todoRepository.UpsertAsync(todo, cancellationToken);
  }

  public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Id cannot be empty");
    }

    var record = await GetAsyncInternal(id, cancellationToken);
    if (record == null)
    {
      throw new ArgumentException("Record not found");
    }

    logger.LogInformation("Deleting Todo record with id {Id}", id);
    await todoRepository.DeleteAsync(id, cancellationToken);
  }

  private async Task<Todo> GetAsyncInternal(Guid id, CancellationToken cancellationToken)
  {
    return await todoRepository.GetAsync(id, cancellationToken);
  }
}
