using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoListApi.Dtos;
using ToDoListApi.Mappings;
using ToDoListApi.Models;
using ToDoListApi.Repositories;
using ToDoListApi.Services;

namespace ToDoListApi.IntegrationTests;

public class EndToEndTests
{
  private readonly Mock<ILogger<TodoService>> _mockLogger;
  private readonly MapperConfiguration _config;
  private readonly ITodoRepository _todoRepository;

  public EndToEndTests()
  {
    _mockLogger = new Mock<ILogger<TodoService>>();
    var loggerFactoryMock = new Mock<ILoggerFactory>();
    loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
      .Returns(new Mock<ILogger<EndToEndTests>>().Object);

    _config = new MapperConfiguration(cfg =>
    {
      cfg.AddProfile<TodoMapper>();
    }, loggerFactoryMock.Object);

    var dbName = $"IntegrationTests-{Guid.NewGuid()}";
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(dbName)
      .EnableSensitiveDataLogging()
      .Options;

    _todoRepository = new TodoRepository(new AppDbContext(options));
  }


  [Fact]
  public async Task CreateAFewItems_Then_Retrieve_Them_Delete_Them()
  {
    // Arrange
    var mapper = new Mapper(_config);
    var fixture = new Fixture();
    fixture.Customize<TodoDto>(a => a.With(t => t.Appointment, DateTime.Now.AddDays(3)));
    var items = fixture.CreateMany<TodoDto>(5).ToList();
    ITodoService service = new TodoService(_mockLogger.Object, mapper, _todoRepository);

    // Add items
    foreach (var item in items)
    {
      await service.UpsertAsync(item, CancellationToken.None);
    }

    // Verify what we just added
    var getAll = await service.GetAllAsync(CancellationToken.None);
    getAll.Should().NotBeNull();
    getAll.Count.Should().Be(items.Count);
    getAll.All(item => items.Any(a => a.Id == item.Id)).Should().BeTrue();

    // Get one item at a time
    foreach (var item in items)
    {
      var todo = await service.GetAsync(item.Id, CancellationToken.None);
      todo.Should().NotBeNull();
    }

    // Delete the items
    foreach (var item in items)
    {
      await service.DeleteAsync(item.Id, CancellationToken.None);
    }

    // Verify the collection is kosher
    getAll = await service.GetAllAsync(CancellationToken.None);
    getAll.Should().NotBeNull();
    getAll.Count.Should().Be(0);
  }
}
