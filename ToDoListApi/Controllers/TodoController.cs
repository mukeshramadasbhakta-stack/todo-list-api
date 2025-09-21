using Microsoft.AspNetCore.Mvc;
using ToDoListApi.Dtos;
using ToDoListApi.Services;
using Microsoft.AspNetCore.Authorization;
using ToDoListApi.Security;

namespace ToDoListApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
public class TodoController(ILogger<TodoController> logger, ITodoService todoService) : ControllerBase
{
  private const string LogErrorString = "{ClassName}::{MethodName} exception, correlationId {Id}";

  [HttpGet("GetAll")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetAll()
  {
    try
    {
      return Ok(await todoService.GetAllAsync(CancellationToken.None));
    }
    catch (Exception ex)
    {
      var correlationId = Guid.NewGuid();
      logger.LogError(ex, LogErrorString,
        nameof(TodoController), nameof(GetAll), correlationId);
      return StatusCode(StatusCodes.Status500InternalServerError,
        $"Error occurred , correlationId {correlationId}");
    }
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Get(Guid id)
  {
    try
    {
      var model = await todoService.GetAsync(id, CancellationToken.None);
      return (model == null) ? NotFound() : Ok(model);
    }
    catch (ArgumentException ex)
    {
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      var correlationId = Guid.NewGuid();
      logger.LogError(ex, LogErrorString,
        nameof(TodoController), nameof(Get), correlationId);
      return StatusCode(StatusCodes.Status500InternalServerError,
        $"Error occurred , correlationId {correlationId}");
    }
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Set([FromBody] TodoDto todoDto)
  {
    try
    {
      return Ok(await todoService.UpsertAsync(todoDto, CancellationToken.None));
    }
    catch (ArgumentException ex)
    {
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      var correlationId = Guid.NewGuid();
      logger.LogError(ex, LogErrorString,
        nameof(TodoController), nameof(Set), correlationId);
      return StatusCode(StatusCodes.Status500InternalServerError,
        $"Error occurred , correlationId {correlationId}");
    }
  }

  [HttpDelete]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Delete(Guid id)
  {
    try
    {
      await todoService.DeleteAsync(id, CancellationToken.None);
      return Ok();
    }
    catch (ArgumentException ex)
    {
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      var correlationId = Guid.NewGuid();
      logger.LogError(ex, LogErrorString,
        nameof(TodoController), nameof(Delete), correlationId);
      return StatusCode(StatusCodes.Status500InternalServerError,
        $"Error occurred , correlationId {correlationId}");
    }
  }
}
