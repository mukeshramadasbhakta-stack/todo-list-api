using AutoMapper;
using ToDoListApi.Dtos;
using ToDoListApi.Models;
using ToDoListApi.Repositories;

namespace ToDoListApi.Services;

public class TodoService(ILogger<TodoService> logger,IMapper mapper, ITodoRepository todoRepository) : ITodoService
{
  /// <summary>
  /// Get all records from db
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns>the records</returns>
  public async Task<List<TodoDto>> GetAllAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Getting all Todos");
    var models = await todoRepository.GetAllAsync(cancellationToken);
    return models.Select(mapper.Map<TodoDto>).ToList();
  }

  /// <summary>
  /// Get a record by id
  /// </summary>
  /// <param name="id"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>the record</returns>
  /// <exception cref="ArgumentException">argument errors</exception>
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
  
  /// <summary>
  /// Upserts a record
  /// </summary>
  /// <param name="todoDto">if guid is not found a new item is created</param>
  /// <param name="cancellationToken"></param>
  /// <returns>guid of updated/created record</returns>
  /// <exception cref="ArgumentException">argument errors</exception>
  public async Task<Guid> UpsertAsync(TodoDto todoDto, CancellationToken cancellationToken)
  {
    if (todoDto == null)
    {
      throw new ArgumentException("todoDto is null");
    }

    if (todoDto.Appointment <= DateTime.Now)
    {
      throw new ArgumentException("Appointment is in the past");
    }

    logger.LogInformation("Upserting Todo record with id {Id}", todoDto.Id);
    var todo = mapper.Map<Todo>(todoDto);

    var record = await GetAsyncInternal(todo.Id, cancellationToken);
    todo.Created = record?.Created ?? DateTime.Now;
    todo.Updated = DateTime.Now;

    return await todoRepository.UpsertAsync(todo, cancellationToken);
  }

  /// <summary>
  /// Delete a record
  /// </summary>
  /// <param name="id">if record is not found we error out</param>
  /// <param name="cancellationToken"></param>
  /// <exception cref="ArgumentException">argument errors</exception>
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

  /// <summary>
  /// Acts for fetching and validations
  /// </summary>
  /// <param name="id"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  private async Task<Todo> GetAsyncInternal(Guid id, CancellationToken cancellationToken)
  {
    return await todoRepository.GetAsync(id, cancellationToken);
  }
}
