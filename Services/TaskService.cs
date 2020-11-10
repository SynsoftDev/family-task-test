using AutoMapper;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Domain.Commands;
using Domain.DataModels;
using Domain.Queries;
using Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TaskService : ITaskService
    {

        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public TaskService(IMapper mapper, ITaskRepository taskRepository)
        {
            _mapper = mapper;
            _taskRepository = taskRepository;
        }

        public async Task<CreateTaskCommandResult> CreateTaskCommandHandler(CreateTaskCommand command)
        {
            try
            {


                var member = _mapper.Map<Domain.DataModels.Task>(command);
                var persistedMember = await _taskRepository.CreateRecordAsync(member);

                var vm = _mapper.Map<TaskVm>(persistedMember);

                return new CreateTaskCommandResult()
                {
                    Payload = vm
                };

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<GetAllTaskQueryResult> GetAllTaskQueryHandler()
        {

            IEnumerable<TaskVm> vm = new List<TaskVm>();

            var task = await _taskRepository.Reset().ToListAsync();

            if (task != null && task.Any())
                vm = _mapper.Map<IEnumerable<TaskVm>>(task);

         
            return new GetAllTaskQueryResult()
            {
                Payload = vm
            };


        }



        public async Task<UpdateTaskCommandResult> UpdateTaskCommandHandler(UpdateTaskCommand command)
        {
            var isSucceed = true;
            var task = await _taskRepository.ByIdAsync(command.Id);

            _mapper.Map<UpdateTaskCommand, Domain.DataModels.Task>(command, task);

            var affectedRecordsCount = await _taskRepository.UpdateRecordAsync(task);

            if (affectedRecordsCount < 1)
                isSucceed = false;

            return new UpdateTaskCommandResult()
            {
                Succeed = isSucceed
            };
        }

    }
}
