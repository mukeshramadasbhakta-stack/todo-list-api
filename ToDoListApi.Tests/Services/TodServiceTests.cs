using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoListApi.Dtos;
using ToDoListApi.Models;
using ToDoListApi.Repositories;
using ToDoListApi.Services;

namespace ToDoListApi.Tests.Services;

public class TodServiceTests
{
  private readonly Mock<ILogger<TodoService>> _mockLogger;
  private readonly Mock<IMapper> _mockMapper;
  private readonly Mock<ITodoRepository> _mockRepository;

  public TodServiceTests()
  {
    var fixture = new Fixture();
    fixture.Customize<Todo>(a => a.With(t => t.Appointment, DateTime.UtcNow.AddDays(-1)));
    _mockLogger = new Mock<ILogger<TodoService>>();
    _mockMapper = new Mock<IMapper>();
    _mockMapper.Setup(a => a.Map<TodoDto>(It.IsAny<Todo>()))
      .Returns(fixture.Build<TodoDto>().Create());
    _mockMapper.Setup(a => a.Map<Todo>(It.IsAny<TodoDto>()))
      .Returns(fixture.Build<Todo>().Create());
    _mockRepository = new Mock<ITodoRepository>();
    _mockRepository.Setup(a => a.GetAllAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(fixture.CreateMany<Todo>(5).ToList());
  }

  [Fact]
  public async Task GetAllAsync_Should_Return_All_Todos()
  {
      // Arrange
      ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);

      // Act
      var result = await service.GetAllAsync(CancellationToken.None);

      // Assert
      result.Should().NotBeNull();
      result.Should().HaveCount(5);
      result.Count(a => a == null).Should().Be(0);
      _mockLogger.Verify(
        x => x.Log(
          LogLevel.Information,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting all Todos")),
          It.IsAny<Exception>(),
          It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
        Times.Once);
      _mockMapper.Verify(
        x => x.Map<TodoDto>(It.IsAny<Todo>()), Times.AtLeastOnce);
  }

  [Fact]
  public async Task GetAsync_Should_Return_Data()
  {
    // Arrange
    var fixture = new Fixture();
    fixture.Customize<Todo>(a => a.With(t => t.Appointment, DateTime.UtcNow.AddDays(-1)));
    fixture.Customize<TodoDto>(a => a.With(t => t.Appointment, DateTime.UtcNow.AddDays(-1)));
    var model = fixture.Create<Todo>();
    _mockRepository.Setup(a => a.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(model);
    _mockMapper.Setup(a => a.Map<TodoDto>(model)).Returns(fixture.Create<TodoDto>());

    ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);

    // Act
    var result = await service.GetAsync(model.Id, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().NotBeEmpty();
    result.Title.Should().NotBeNullOrEmpty();
    result.Appointment.Should().NotBeAfter(DateTime.UtcNow);
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting Todo with id")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
      Times.Once);
    _mockMapper.Verify(
      x => x.Map<TodoDto>(It.IsAny<Todo>()), Times.AtLeastOnce);
    _mockRepository.Verify(
      x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetAsync_With_Empty_Id_Should_Throws_ArgumentException()
  {
    // Arrange
    ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);

    // Act
    async Task Act() => await service.GetAsync(Guid.Empty, CancellationToken.None);

    // Assert
    var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
    ex.Message.Should().Contain("Id cannot be empty");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting Todo with id")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
      Times.Never);
    _mockMapper.Verify(
      x => x.Map<TodoDto>(It.IsAny<Todo>()), Times.Never);
    _mockRepository.Verify(
      x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task UpsertAsync_Should_Return_Id()
  {
    // Arrange
    ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);
    var fixture = new Fixture();
    var id = Guid.NewGuid();
    fixture.Customize<TodoDto>(a =>
      a.With(t => t.Appointment, DateTime.UtcNow.AddDays(1))
        .With(t => t.Id, id)
      );
    var dto = fixture.Create<TodoDto>();
    _mockRepository.Setup(a => a.UpsertAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(id);

    // Act
    var result = await service.UpsertAsync(dto, CancellationToken.None);

    // Assert
    result.Should().Be(id);
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Upserting Todo record with id")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
      Times.Once);
    _mockMapper.Verify(
      x => x.Map<Todo>(It.IsAny<TodoDto>()), Times.AtLeastOnce);
    _mockRepository.Verify(
      x => x.UpsertAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UpsertAsync_WithNull_Payload_Should_Throws_ArgumentException()
  {
    // Arrange
    ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);

    // Act
    async Task Act() => await service.UpsertAsync(null, CancellationToken.None);

    // Assert
    var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
    ex.Message.Should().Contain("todoDto is null");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Upserting Todo record with id")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
      Times.Never);
    _mockMapper.Verify(
      x => x.Map<Todo>(It.IsAny<TodoDto>()), Times.Never);
    _mockRepository.Verify(
      x => x.UpsertAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task UpsertAsync_WhenAppointmentDate_In_Payload_IsLessThan_Today_Should_Throws_ArgumentException()
  {
    // Arrange
    ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);
    var fixture = new Fixture();
    var id = Guid.NewGuid();
    fixture.Customize<TodoDto>(a =>
      a.With(t => t.Appointment, DateTime.UtcNow.AddDays(-2))
        .With(t => t.Id, id)
    );
    var dto = fixture.Create<TodoDto>();
    _mockRepository.Setup(a => a.UpsertAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(id);

    // Act
    async Task Act() => await service.UpsertAsync(dto, CancellationToken.None);

    // Assert
    var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
    ex.Message.Should().Contain("Appointment is in the past");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Upserting Todo record with id")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
      Times.Never);
    _mockMapper.Verify(
      x => x.Map<Todo>(It.IsAny<TodoDto>()), Times.Never);
    _mockRepository.Verify(
      x => x.UpsertAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_Should_JustWork()
  {
    // Arrange
    ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);
    var fixture = new Fixture();
    var id = Guid.NewGuid();
    fixture.Customize<TodoDto>(a =>
      a.With(t => t.Appointment, DateTime.UtcNow.AddDays(-1))
        .With(t => t.Id, id)
    );
    fixture.Customize<Todo>(a =>
      a.With(t => t.Appointment, DateTime.UtcNow.AddDays(+2))
        .With(t => t.Id, id)
    );
    var model = fixture.Create<Todo>();
    _mockRepository.Setup(a => a.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(model);

    // Act
    await service.DeleteAsync(id, CancellationToken.None);

    // Assert
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting Todo record with id")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
      Times.Once);
    _mockRepository.Verify(
      x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task DeleteAsync_With_Empty_Id_Should_Throws_ArgumentException()
  {
    // Arrange
    ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);

    // Act
    async Task Act() => await service.DeleteAsync(Guid.Empty, CancellationToken.None);

    // Assert
    var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
    ex.Message.Should().Contain("Id cannot be empty");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting Todo record with id")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
      Times.Never);
    _mockMapper.Verify(
      x => x.Map<TodoDto>(It.IsAny<Todo>()), Times.Never);
    _mockRepository.Verify(
      x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_With_Invalid_Id_Should_Throws_ArgumentException()
  {
    // Arrange
    ITodoService service = new TodoService(_mockLogger.Object, _mockMapper.Object, _mockRepository.Object);
    Todo model = null;
    _mockRepository.Setup(a => a.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(model);

    // Act
    async Task Act() => await service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

    // Assert
    var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
    ex.Message.Should().Contain("Record not found");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting Todo record with id")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
      Times.Never);
    _mockMapper.Verify(
      x => x.Map<TodoDto>(It.IsAny<Todo>()), Times.Never);
    _mockRepository.Verify(
      x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
  }
}
