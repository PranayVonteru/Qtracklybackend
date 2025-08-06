using Demoproject.Data;
using Demoproject.Dtos;
using Demoproject.Models;
using Demoproject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demoproject.Services
{
    public class DependencyService : IDependencyService
    {
        private readonly QTraklyDBContext _dbContext;
        private readonly ILogger<DependencyService> _logger;

        public DependencyService(QTraklyDBContext dbContext, ILogger<DependencyService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(Dependency, string)> CreateDependencyAsync(DependencyCreateDto dto, string userId)
        {
            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == dto.TaskItemId && t.CreatedBy == userId);

            //if (task == null)
            //    return (null, "Task not found or you don't have access to it.");

            if (dto.DependsOnTaskId.HasValue)
            {
                var dependsOnTask = await _dbContext.Tasks
                    .FirstOrDefaultAsync(t => t.Id == dto.DependsOnTaskId && t.CreatedBy == userId);

                if (dependsOnTask == null)
                    return (null, "Dependent task not found or you don't have access to it.");
            }

            var dependency = new Dependency
            {
                TaskItemId = dto.TaskItemId,
                DependsOnTaskId = dto.DependsOnTaskId,
                AssignedTo = dto.AssignedTo ?? userId,
                Notes = dto.Notes
            };

            _dbContext.Dependencies.Add(dependency);
            await _dbContext.SaveChangesAsync();

            return (dependency, "Dependency added successfully.");
        }

        public async Task<string> DeleteDependencyAsync(int id, string userId)
        {
            var dependency = await _dbContext.Dependencies
                .Include(d => d.TaskItem)
                .FirstOrDefaultAsync(d => d.Id == id && (d.TaskItem.CreatedBy == userId || d.AssignedTo == userId));

            if (dependency == null)
                return "Dependency not found or you don't have permission to delete it.";

            _dbContext.Dependencies.Remove(dependency);
            await _dbContext.SaveChangesAsync();

            return "Dependency deleted successfully.";
        }

        public async Task<List<DependencyDto>> GetAllDependenciesAsync(string userId)
        {
            return await _dbContext.Dependencies
                .Include(d => d.TaskItem)
                .Where(d => d.TaskItem.CreatedBy == userId || d.AssignedTo == userId)
                .Select(d => new DependencyDto
                {
                    Id = d.Id,
                    TaskItemId = d.TaskItemId,
                    TaskName = d.TaskItem.TaskName,
                    DependsOnTaskId = d.DependsOnTaskId,
                    DependsOnTaskName = d.DependsOnTaskId.HasValue ?
                        _dbContext.Tasks.Where(t => t.Id == d.DependsOnTaskId).Select(t => t.TaskName).FirstOrDefault() : null,
                    AssignedTo = d.AssignedTo,
                    Notes = d.Notes
                })
                .ToListAsync();
        }

        public async Task<(List<DependencyDto>, string)> GetDependenciesByTaskIdAsync(int taskId, string userId)
        {
            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedBy == userId);

            if (task == null)
                return (null, "Task not found or you don't have access to it.");

            var dependencies = await _dbContext.Dependencies
                .Where(d => d.TaskItemId == taskId)
                .Select(d => new DependencyDto
                {
                    Id = d.Id,
                    TaskItemId = d.TaskItemId,
                    DependsOnTaskId = d.DependsOnTaskId,
                    DependsOnTaskName = d.DependsOnTaskId.HasValue ?
                        _dbContext.Tasks.Where(t => t.Id == d.DependsOnTaskId).Select(t => t.TaskName).FirstOrDefault() : null,
                    AssignedTo = d.AssignedTo,
                    Notes = d.Notes
                })
                .ToListAsync();

            return (dependencies, "Dependencies retrieved successfully.");
        }

        public async Task<List<DependencyDto>> GetDependenciesAssignedToMeAsync(string userId)
        {
            return await _dbContext.Dependencies
                .Include(d => d.TaskItem)
                .Where(d => d.AssignedTo == userId)
                .Select(d => new DependencyDto
                {
                    Id = d.Id,
                    TaskItemId = d.TaskItemId,
                    TaskName = d.TaskItem.TaskName,
                    DependsOnTaskId = d.DependsOnTaskId,
                    DependsOnTaskName = d.DependsOnTaskId.HasValue ?
                        _dbContext.Tasks.Where(t => t.Id == d.DependsOnTaskId).Select(t => t.TaskName).FirstOrDefault() : null,
                    AssignedTo = d.AssignedTo,
                    Notes = d.Notes
                })
                .ToListAsync();
        }

        public async Task<(Dependency, string)> UpdateDependencyAsync(int id, DependencyUpdateDto dto, string userId)
        {
            var dependency = await _dbContext.Dependencies
                .Include(d => d.TaskItem)
                .FirstOrDefaultAsync(d => d.Id == id && (d.TaskItem.CreatedBy == userId || d.AssignedTo == userId));

            if (dependency == null)
                return (null, "Dependency not found or you don't have permission to update it.");

            if (!string.IsNullOrEmpty(dto.AssignedTo) && dependency.TaskItem.CreatedBy == userId)
                dependency.AssignedTo = dto.AssignedTo;

            if (dto.Notes != null)
                dependency.Notes = dto.Notes;

            if (dto.DependsOnTaskId.HasValue && dependency.TaskItem.CreatedBy == userId)
            {
                var dependsOnTask = await _dbContext.Tasks
                    .FirstOrDefaultAsync(t => t.Id == dto.DependsOnTaskId && t.CreatedBy == userId);

                if (dependsOnTask == null)
                    return (null, "Dependent task not found or you don't have access to it.");

                dependency.DependsOnTaskId = dto.DependsOnTaskId;
            }

            _dbContext.Dependencies.Update(dependency);
            await _dbContext.SaveChangesAsync();

            return (dependency, "Dependency updated successfully.");
        }




        public Task<Object> UserviewTaskDependencies(int id)
        {
            return Task.Run(() =>
            {
                var obj = _dbContext.taskDependencyFacts.Where(e => e.TaskId == id)
                .Select(e => e.DependencyTaskId)
                .ToList();

                var requestDetails = _dbContext.DependencyRequests
                 .Where(e => obj.Contains(e.DependencyTaskId))
                 .Join(_dbContext.DependencyFacts,
                 request => request.DependencyTaskId,
                 fact => fact.DependencyTaskId,
                 (request, fact) => new { request, fact })
                 .Select(result => new
                 {
                     result.request,
                     result.fact.Status
                 })
                 .ToList();

                return (Object)requestDetails;
            });
        }

        public Task<Object> UserviewTaskDependent(int id)
        {
            return Task.Run(() =>
            {
                var obj = _dbContext.DependencyFacts.Where(e => e.DependsOnTaskId == id)
                .Select(e => e.DependencyTaskId)
                .ToList();
                var requestDetails = _dbContext.DependencyRequests
                  .Where(e => obj.Contains(e.DependencyTaskId))
                  .Join(_dbContext.DependencyFacts,
                  request => request.DependencyTaskId,
                  fact => fact.DependencyTaskId,
                  (request, fact) => new { request, fact })
                  .Select(result => new
                  {
                      result.request,
                      result.fact.Status
                  })
                  .ToList();
                return (Object)requestDetails;
            });
        }
        public Task<Object> UserviewTaskDependenciesAccepted(int id)
        {
            return Task.Run(() =>
            {

                var task = _dbContext.DependencyFacts
                 .Where(e => e.DependencyTaskId == id )
                 .Select(p => p.DependsOnTaskId)
                 .FirstOrDefault();

                var details = _dbContext.Tasks
                 .Where(e => e.Id == task)
                 .FirstOrDefault();

                return (Object)details;

            });

         }

        public async Task<Object> RejectDependency(int DependencyTaskId)
        {
            var dependency = _dbContext.DependencyFacts.Where(p => p.DependencyTaskId == DependencyTaskId).FirstOrDefault();
            dependency.Status = "Rejected";
            _dbContext.Update(dependency);
            _dbContext.SaveChanges();

            var dependencydetails = _dbContext.DependencyRequests.Where(p => p.DependencyTaskId == DependencyTaskId).FirstOrDefault();

            var feedback = new Feedback
            {
                UserId = dependency.UserId,
                ManagerId = dependency.TargetUserId,
                Message = "Your Dependency " + dependencydetails.TaskName + " CreatedAt :" + dependencydetails.RequestedDate + " has been Rejected",
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _dbContext.Feedbacks.Add(feedback);
            _dbContext.SaveChanges();
            return dependency;
        }
    }

}