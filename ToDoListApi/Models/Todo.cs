namespace ToDoListApi.Models;

public class Todo
{
  public Guid Id { get; set; }
  public string Title { get; set; }
  public DateTime Appointment { get; set; }
  public DateTime Created { get; set; }
  public DateTime Updated { get; set; }
}
