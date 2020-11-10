using AutoMapper;
using Domain.Commands;
using Domain.DataModels;
using Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;


namespace WebApi.AutoMapper
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            CreateMap<CreateTaskCommand, Task>();
            CreateMap<UpdateTaskCommand, Task>();
            CreateMap<Task, TaskVm>();
        }
    }
}
