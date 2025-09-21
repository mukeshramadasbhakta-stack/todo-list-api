namespace ToDoListApi.Dtos;

public class TodoDto
{
  public Guid Id { get; set; }
  public string Title { get; set; }
  public DateTime Appointment { get; set; }
}
