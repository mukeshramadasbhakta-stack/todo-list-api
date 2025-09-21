using AutoMapper;
using ToDoListApi.Dtos;
using ToDoListApi.Models;

namespace ToDoListApi.Mappings;

public class TodoMapper : Profile
{
  public TodoMapper()
  {
    CreateMap<TodoDto, Todo>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
      .ForMember(dest => dest.Appointment, opt => opt.MapFrom(src => src.Appointment))
      .ReverseMap();
  }
}
