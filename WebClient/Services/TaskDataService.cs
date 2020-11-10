using Domain.Commands;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebClient.Abstractions;
using WebClient.Shared.Models;
using Core.Extensions.ModelConversion;
using Domain.ViewModel;
using Domain.Queries;
using WebClient.Pages;

namespace WebClient.Services
{
    public class TaskDataService : ITaskDataService
    {
        private readonly HttpClient httpClient;

        public TaskDataService(IHttpClientFactory clientFactory)
        {
            httpClient = clientFactory.CreateClient("FamilyTaskAPI");

            Tasks = new List<TaskVm>();
            LoadTask();
        }

        // public List<TaskVm> Tasks { get; private set; }
        public TaskVm SelectedTask { get; private set; }

        IEnumerable<TaskVm> ITaskDataService.Tasks => Tasks;

        private IEnumerable<TaskVm> Tasks;

        public event EventHandler TasksUpdated;
        public event EventHandler TaskSelected;
        public event EventHandler ShowAllTasks;
        public event EventHandler<string> UpdateTaskFailed;
        public event EventHandler<string> CreateTaskFailed;

        private async void LoadTask()
        {
            Tasks = (await GetAllTasks()).Payload;


            TasksUpdated?.Invoke(this, null);
        }

        private async Task<CreateTaskCommandResult> Create(CreateTaskCommand command)
        {
            return await httpClient.PostJsonAsync<CreateTaskCommandResult>("task", command);
        }

        private async Task<UpdateTaskCommandResult> Update(UpdateTaskCommand command)
        {
            return await httpClient.PutJsonAsync<UpdateTaskCommandResult>($"task/{command.Id}", command);
        }

        private async Task<GetAllTaskQueryResult> GetAllTasks()
        {
            return await httpClient.GetJsonAsync<GetAllTaskQueryResult>("task");
        }

        public void SelectTask(Guid id)
        {
            SelectedTask = Tasks.SingleOrDefault(t => t.Id == id);
            TasksUpdated?.Invoke(this, null);
        }

        public async Task ToggleTask(Guid id)
        {
            try
            {


                var updateToggletask = new TaskVm();
                foreach (var taskModel in Tasks)
                {
                    if (taskModel.Id == id)
                    {
                        taskModel.IsComplete = !taskModel.IsComplete;
                        updateToggletask = taskModel;
                    }
                }

                var update = await Update(updateToggletask.ToUpdateTaskCommand());

                if (update != null)
                {
                    TasksUpdated?.Invoke(this, null);
                    return;
                }
                UpdateTaskFailed?.Invoke(this, "Unable to save changes.");
            }
            catch (Exception ex)
            {

                UpdateTaskFailed?.Invoke(this, ex.Message);
            }

        }

        public async Task AddTask(TaskVm model)
        {
            try
            {
                var result = await Create(model.ToCreateTaskCommand());

                if (result != null)
                {
                    var updatedTasklist = (await GetAllTasks()).Payload;

                    if (updatedTasklist != null)
                    {
                        Tasks = updatedTasklist;
                        TasksUpdated?.Invoke(this, null);
                        return;
                    }

                    CreateTaskFailed?.Invoke(this, "The task was successfully saved, but we can no longer get an updated list of tasks from the server.");
                    return;

                }
                CreateTaskFailed?.Invoke(this, "Unable to save changes.");
            }
            catch (Exception ex)
            {
                CreateTaskFailed?.Invoke(this, ex.Message);

            }
        }

        public async Task SelectDraggableTask(TaskVm draggableTask)
        {

            SelectedTask = draggableTask;

        }

        public async Task UpdateTaskOnDrop(Guid memberID)
        {
            try
            {
                var droppedTask = SelectedTask;
                if (droppedTask.AssignedToId == null || droppedTask.AssignedToId == Guid.Empty)
                {
                    droppedTask.AssignedToId = memberID;
                    var update = await Update(droppedTask.ToUpdateTaskCommand());
                    if (update != null)
                    {
                        var updatedTasklist = (await GetAllTasks()).Payload;

                        if (updatedTasklist != null)
                        {
                            Tasks = updatedTasklist;
                            TasksUpdated?.Invoke(this, null);
                            return;
                        }
                        UpdateTaskFailed?.Invoke(this, "The task was successfully assigned, but we can no longer get an updated list of tasks from the server.");
                        return;
                    }
                    UpdateTaskFailed?.Invoke(this, "Unable to assign task.");
                    return;
                }
                UpdateTaskFailed?.Invoke(this, "Unable to save changes. This task already assigned to other member");
            }
            catch (Exception ex)
            {
                UpdateTaskFailed?.Invoke(this, ex.Message);
            }
        }

    }
}