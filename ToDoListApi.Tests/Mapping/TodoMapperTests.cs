using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoListApi.Dtos;
using ToDoListApi.Mappings;
using ToDoListApi.Models;

namespace ToDoListApi.Tests.Mapping;

public class TodoMapperTests
{
  private readonly MapperConfiguration _config;
  private readonly Fixture _fixture;
  public TodoMapperTests()
  {
    var loggerFactoryMock = new Mock<ILoggerFactory>();
    loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
      .Returns(new Mock<ILogger<TodoMapperTests>>().Object);

    _config = new MapperConfiguration(cfg =>
    {
      cfg.AddProfile<TodoMapper>();
    }, loggerFactoryMock.Object);

    _fixture = new Fixture();
  }

  [Fact]
  public void TodoDto_To_Todo_Success()
  {
    // Arrange
    var dto = _fixture.Create<TodoDto>();
    var mapper = new Mapper(_config);

    // Act
    var model = mapper.Map<Todo>(dto);

    // Assert
    model.Should().NotBeNull();
    model.Id.Should().Be(dto.Id);
    model.Title.Should().Be(dto.Title);
    model.Appointment.Should().Be(dto.Appointment);
    model.Created.Should().BeBefore(DateTime.UtcNow);
    model.Updated.Should().BeBefore(DateTime.UtcNow);
  }

  [Fact]
  public void Todo_To_TodoDto_Success()
  {
    // Arrange
    var model = _fixture.Create<Todo>();
    var mapper = new Mapper(_config);

    // Act
    var dto = mapper.Map<TodoDto>(model);

    // Assert
    model.Should().NotBeNull();
    model.Id.Should().Be(dto.Id);
    model.Title.Should().Be(dto.Title);
    model.Appointment.Should().Be(dto.Appointment);
  }
}
