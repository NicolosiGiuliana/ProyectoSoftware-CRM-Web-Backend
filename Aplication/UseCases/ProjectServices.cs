using Application.Response;
using Application.Exceptions;
using Application.Interfaces.Command;
using Application.Interfaces.Query;
using Application.Interfaces.Service;
using Application.Request;
using Application.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.UseCases
{
    public class ProjectServices : IProjectServices
    {
        private readonly IProjectQuery _projectQuery;
        private readonly IProjectCommand _projectCommand;
        private readonly IInteractionTypeQuery _interactionTypeQuery;
        private readonly IUserQuery _userQuery;
        private readonly ITaskStatusQuery _taskStatusQuery;
        private readonly IClientQuery _clientQuery;
        private readonly ICampaignTypeQuery _campaignTypeQuery;
        private readonly ITaskQuery _taskQuery;

        public ProjectServices(IProjectQuery query, IProjectCommand command, IInteractionTypeQuery interactionTypeQuery, IUserQuery userQuery,
                                ITaskStatusQuery taskStatusQuery, IClientQuery clientQuery, ICampaignTypeQuery campaignTypeQuery, ITaskQuery taskQuery)
        {
            _projectQuery = query;
            _projectCommand = command;
            _interactionTypeQuery = interactionTypeQuery;
            _userQuery = userQuery;
            _taskStatusQuery = taskStatusQuery;
            _clientQuery = clientQuery;
            _campaignTypeQuery = campaignTypeQuery;
            _taskQuery = taskQuery;
        }

        public async Task<List<Response.Project>> GetProjects(string? name, int? campaign, int? client, int? offset, int? size)
        {
            //Validacion de numeros no negativos hablado en clase, no esta presente en la documentacion
            if ((size.HasValue && size <= 0) || (offset.HasValue && offset < 0))
            {
                throw new BadRequest("Both numbers must be positive.");
            }

            //Validacion de existencia de project
            var projects = await _projectQuery.GetProjects(name, campaign, client, offset, size);
            if (projects == null || !projects.Any())
            {
                return new List<Response.Project>();
            }

            var responseProjects = new List<Response.Project>();
            foreach (var project in projects)
            {
                var clientQuery = await _clientQuery.GetClientById(project.ClientID);
                var campaignType = await _campaignTypeQuery.GetCampaignTypeById(project.CampaignType);

                var responseProject = new Response.Project
                {
                    Id = project.ProjectID,
                    Name = project.ProjectName,
                    Start = project.StartDate,
                    End = project.EndDate,
                    Client = new Clients
                    {
                        Id = clientQuery.ClientID,
                        Name = clientQuery.Name,
                        Email = clientQuery.Email,
                        Company = clientQuery.Company,
                        Phone = clientQuery.Phone,
                        Address = clientQuery.Address,
                    },
                    CampaignType = new GenericResponse
                    {
                        Id = campaignType.Id,
                        Name = campaignType.Name,
                    }
                };
                responseProjects.Add(responseProject);
            }
          return responseProjects;
        }

        public async Task<ProjectDetails> CreateProject(ProjectRequest projectRequest)
        {
            var errors = new List<string>();

            //Validacion de repeticion de nombre
            var existingProject = await _projectQuery.GetProjectByName(projectRequest.Name);
            if (existingProject != null)
            {
                errors.Add("A project with the same name already exists.");
            }

            //Validacion de datos ingresados no nulos ni vacios y/o existentes
            await ValidateProjectRequest(projectRequest, errors);

            if (errors.Any())
            {
                throw new BadRequest(string.Join(" ; ", errors));
            }

            var project = new Domain.Entities.Project
            {
                ProjectName = projectRequest.Name,
                StartDate = projectRequest.Start,
                EndDate = projectRequest.End,
                ClientID = projectRequest.Client,
                CampaignType = projectRequest.CampaignType,
                CreateDate = DateTime.Now,
            };
            await _projectCommand.InsertProject(project);

            var client = await _clientQuery.GetClientById(projectRequest.Client);
            var campaignType = await _campaignTypeQuery.GetCampaignTypeById(projectRequest.CampaignType);
            return new ProjectDetails
            {
                Data = new Application.Response.Project
                {
                    Id = project.ProjectID,
                    Name = project.ProjectName,
                    Start = project.StartDate,
                    End = project.EndDate,
                    Client = new Clients
                    {
                        Id = client.ClientID,
                        Name = client.Name,
                        Email = client.Email,
                        Company = client.Company,
                        Phone = client.Phone,
                        Address = client.Address,
                    },
                    CampaignType = new GenericResponse
                    {
                        Id = campaignType.Id,
                        Name = campaignType.Name,
                    }
                },
                Interactions = new List<Interactions>(),
                Tasks = new List<Tasks>()
            };
        }

        public async Task<ProjectDetails> GetProjectById(Guid id)
        {
            Domain.Entities.Project project = await _projectQuery.GetProjectById(id);
            //Validacion de que el project exista
            if (project == null)
            {
                throw new NotFoundException("Project not found");
            }
            var client = await _clientQuery.GetClientById(project.ClientID);
            var campaignType = await _campaignTypeQuery.GetCampaignTypeById(project.CampaignType);

            var data = new Application.Response.Project
            {
                Id = project.ProjectID,
                Name = project.ProjectName,
                Start = project.StartDate,
                End = project.EndDate,
                Client = new Clients
                {
                    Id = client.ClientID,
                    Name = client.Name,
                    Email = client.Email,
                    Company = client.Company,
                    Phone = client.Phone,
                    Address = client.Address,
                },
                CampaignType = new GenericResponse
                {
                    Id = campaignType.Id,
                    Name = campaignType.Name
                }
            };

            var interactions = project.Interactions.Select(interaction => new Interactions
            {
                Id = interaction.InteractionID,
                Notes = interaction.Notes,
                Date = interaction.Date,
                ProjectId = interaction.ProjectID,
                InteractionType = new GenericResponse
                {
                    Id = interaction.InteractionTypes.Id,
                    Name = interaction.InteractionTypes.Name,
                }
            }).ToList();

            var tasks = project.Tasks.Select(task => new Tasks
            {
                Id = task.TaskID,
                Name = task.Name,
                DueDate = task.DueDate,
                ProjectId = task.ProjectID,
                Status = new GenericResponse
                {
                    Id = task.TaskStatus.Id,
                    Name = task.TaskStatus.Name
                },
                UserAssigned = new Users
                {
                    UserID = task.User.UserID,
                    Name = task.User.Name,
                    Email = task.User.Email
                }
            }).ToList();

            return await System.Threading.Tasks.Task.FromResult(new ProjectDetails
            {
                Data = data,
                Interactions = interactions,
                Tasks = tasks
            });
        }

        public async Task<Interactions> AddInteraction(Guid projectId, InteractionsRequest interactionRequest)
        {
            var errors = new List<string>();

            //Validacion de proyecto existente
            var project = await _projectQuery.GetProjectById(projectId);
            await ValidateProjectExistsAsync(projectId,errors);

            //Validacion de que tipo de interaccion existe
            var interactionTypeName = (await _interactionTypeQuery.GetInteractionTypeById(interactionRequest.InteractionType))?.Name;
            if (interactionTypeName == null)
            {
                errors.Add("Interaction type not found.");
            }

            //Validacion de datos ingresados no nulos ni vacios
            ValidateInteractionRequest(interactionRequest, errors);

            if (errors.Any())
            {
                throw new BadRequest(string.Join(" ; ", errors));
            }

            var newInteraction = new Interaction
                {
                    InteractionID = Guid.NewGuid(),
                    Notes = interactionRequest.Notes,
                    Date = interactionRequest.Date,
                    ProjectID = projectId,
                    InteractionType = interactionRequest.InteractionType,
                };

              await _projectCommand.AddProjectInteractions(newInteraction);
              project.UpdateDate = DateTime.Now;
              await _projectCommand.UpdateProject(project);

              return new Interactions
                {
                    Id = newInteraction.InteractionID,
                    Notes = newInteraction.Notes,
                    Date = newInteraction.Date,
                    ProjectId = newInteraction.ProjectID,
                    InteractionType = new GenericResponse
                    {
                        Id = newInteraction.InteractionType,
                        Name = interactionTypeName,
                    }
               };
        }

        public async Task<Tasks> AddTask(Guid projectId, TasksRequest task)
        {
            var errors = new List<string>();

            //Validacion de proyecto existente
            var project = await _projectQuery.GetProjectById(projectId);
            await ValidateProjectExistsAsync(projectId, errors);

            //Validacion de datos ingresados no nulos ni vacios
            ValidateTaskRequest(task, errors);
            if (errors.Any())
            {
                throw new BadRequest(string.Join(" ; ", errors));
            }

            var newTask = new Domain.Entities.Task
                {
                    TaskID = Guid.NewGuid(),
                    Name = task.Name,
                    DueDate = task.DueDate,
                    ProjectID = projectId,
                    Status = task.Status,
                    AssignedTo = task.User,
                    CreateDate = DateTime.Now,
                };
            await _projectCommand.AddProjectTasks(newTask);
            project.UpdateDate = DateTime.Now;
            await _projectCommand.UpdateProject(project);

            var newTaskStatus = await _taskStatusQuery.GetTaskStatusById(task.Status);
            var newUser = await _userQuery.GetUserById(task.User);
            return new Tasks
                {
                    Id = newTask.TaskID,
                    Name = newTask.Name,
                    DueDate = newTask.DueDate,
                    ProjectId = newTask.ProjectID,
                    Status = new GenericResponse 
                    {
                        Id = newTaskStatus.Id,
                        Name = newTaskStatus.Name,
                    },
                    UserAssigned = new Users
                    {
                        UserID = newUser.UserID,
                        Name = newUser.Name,
                        Email = newUser.Email,
                    }
            };
        }

        public async Task<Tasks> UpdateTask(Guid taskId, TasksRequest taskRequest)
        {
            var errors = new List<string>();

            //Validacion de task existente
            var task = await _taskQuery.GetTaskById(taskId);
            if (task == null)
            {
                errors.Add("Task not found");
            }

            //Validacion de datos ingresados no nulos ni vacios
            ValidateTaskRequest(taskRequest, errors);
            if (errors.Any())
            {
                throw new BadRequest(string.Join(" ; ", errors));
            }

            task.Name = taskRequest.Name;
            task.DueDate = taskRequest.DueDate;
            task.Status = taskRequest.Status;
            task.AssignedTo = taskRequest.User;
            task.UpdateDate = DateTime.Now;

            await _projectCommand.UpdateProjectTasks(task);
            
            var taskStatus = await _taskStatusQuery.GetTaskStatusById(taskRequest.Status);
            var user = await _userQuery.GetUserById(taskRequest.User);
            return new Tasks
            {
                Id = task.TaskID,
                Name = task.Name,
                DueDate = task.DueDate,
                ProjectId = task.ProjectID,
                Status = new GenericResponse
                {
                    Id = taskStatus.Id,
                    Name = taskStatus.Name,
                },
                UserAssigned = new Users
                {
                    UserID = user.UserID,
                    Name = user.Name,
                    Email = user.Email,
                }
            };
        }

        // Métodos privados para validaciones
        private async System.Threading.Tasks.Task ValidateProjectRequest(ProjectRequest projectRequest, List<string> errors)
        {

            if (string.IsNullOrWhiteSpace(projectRequest.Name))
            {
                errors.Add("Project name can't be null, empty, or whitespace.");
            }

            if (projectRequest.End < projectRequest.Start)
            {
                errors.Add("Project end date can't be earlier than the start date.");
            }

            if (projectRequest.Start > projectRequest.End)
            {
                errors.Add("Project end date can't be earlier than the start date.");
            }

            var client = await _clientQuery.GetClientById(projectRequest.Client);
            if (client == null)
            {
                errors.Add("Client not found.");
            }

            if (projectRequest.CampaignType < 1 || projectRequest.CampaignType > 4) //estático
            {
                errors.Add("Campaign type not found. Must be a number between 1 and 4 because it is preloaded.");
            }
        }

        private async System.Threading.Tasks.Task ValidateProjectExistsAsync(Guid projectId, List<string> errors)
        {
            var project = await _projectQuery.GetProjectById(projectId);
            if (project == null)
            {
                errors.Add("Project not found");
            }
        }

        private void ValidateInteractionRequest(InteractionsRequest interactionRequest, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(interactionRequest.Notes))
            {
                errors.Add("Notes cannot be null or empty.");
            }

            if (interactionRequest.Date < DateTime.Now) // DateTime nunca puede ser null
            {
                errors.Add("Interaction date can't be in the past.");
            }

            if (interactionRequest.InteractionType < 1 || interactionRequest.InteractionType > 4) // Estático
            {
                errors.Add("Interaction type not found. Must be a number between 1 and 4 because it is preloaded.");
            }
        }

        private void ValidateTaskRequest(TasksRequest taskRequest, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(taskRequest.Name))
            {
                errors.Add("Task name can't be null, empty, or whitespace.");
            }

            if (taskRequest.DueDate < DateTime.Now) // DateTime nunca puede ser null
            {
                errors.Add("Due date can't be in the past.");
            }

            if (taskRequest.User < 1 || taskRequest.User > 5) // Estático
            {
                errors.Add("User assigned to the task not found. Must be a number between 1 and 5 because it is preloaded.");
            }

            if (taskRequest.Status < 1 || taskRequest.Status > 5) // Estático
            {
                errors.Add("Task status not found. Must be a number between 1 and 5 because it is preloaded.");
            }
        } 
    }
}