using Infrastructure.Persistence;
using Application.Interfaces.Command;
using Domain.Entities;
using System;
using Microsoft.EntityFrameworkCore;
using Application.Response;

namespace Infrastructure.Command
{
    public class ProjectCommand : IProjectCommand
    {
        private readonly AppDbContext _context;

        public ProjectCommand(AppDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task InsertProject(Domain.Entities.Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateProject(Domain.Entities.Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task AddProjectInteractions(Interaction interaction)
        {
            _context.Add(interaction);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task AddProjectTasks(Domain.Entities.Task task)
        {
            _context.Add(task);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateProjectTasks(Domain.Entities.Task task)
        {
            _context.Update(task);
            await _context.SaveChangesAsync();
        }
    }
}