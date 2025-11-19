using Application.Request;
using Application.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Service
{
    public interface IProjectServices
    {
        Task<ProjectDetails> GetProjectById(Guid id);
        Task<ProjectDetails> CreateProject(ProjectRequest projectRequest);
        Task<Interactions> AddInteraction(Guid projectId, InteractionsRequest interactionRequest);
        Task<Tasks> AddTask(Guid projectId, TasksRequest taskRequest);
        Task<Tasks> UpdateTask(Guid taskId, TasksRequest taskRequest);
        Task<List<Project>> GetProjects(string? name, int? campaign, int? client, int? offset, int? size);
    }
}