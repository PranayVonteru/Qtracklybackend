



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
    public class FocusBoardService : IFocusBoardService
    {
        private readonly QTraklyDBContext _dbContext;
        private readonly ILogger<FocusBoardService> _logger;

        public FocusBoardService(QTraklyDBContext dbContext, ILogger<FocusBoardService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<object> GetTodaysFocusAsync(string userId)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;

            _logger.LogInformation("Fetching today's focus tasks for user {UserId} on {Today}", userId, today);

            var userTasks = await _dbContext.Tasks
                .Where(t => t.StartDate.Date <= today && t.DueDate.Date >= today && t.CreatedBy == userId && t.Status == "In Progress")
                .ToListAsync();

            _logger.LogInformation("Found {Count} tasks: {Tasks}", userTasks.Count,
                string.Join(", ", userTasks.Select(t => $"Id={t.Id}, Name={t.TaskName}")));

            return userTasks;
        }

        public async Task<object> GetOverdueTasksAsync(string userId)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;

            _logger.LogInformation("Fetching overdue tasks for user {UserId} on {Today}", userId, today);

            var userTasks = await _dbContext.Tasks
                .Where(t => t.DueDate.Date < today && t.CreatedBy == userId && t.Status == "In Progress")
                .ToListAsync();

            _logger.LogInformation("Found {Count} overdue tasks: {Tasks}", userTasks.Count,
                string.Join(", ", userTasks.Select(t => $"Id={t.Id}, Name={t.TaskName}")));

            return userTasks;
        }

        public async Task<List<TaskSummaryDto>> GetWaitingTasksAsync(string userId)
        {
            _logger.LogInformation("Fetching waiting tasks for user {UserId}", userId);

            var tasks = await _dbContext.Dependencies
                .Include(d => d.TaskItem)
                .Where(d => d.AssignedTo == userId && d.TaskItem.Status != "Completed")
                .Select(d => new TaskSummaryDto
                {
                    Id = d.TaskItem.Id,
                    Name = d.TaskItem.TaskName,
                    Status = d.TaskItem.Status,
                    DueDate = d.TaskItem.DueDate.ToString("dd/MM/yyyy"),
                    AssignedTo = d.AssignedTo
                })
                .Distinct()
                .ToListAsync();

            _logger.LogInformation("Found {Count} waiting tasks", tasks.Count);
            return tasks;
        }

        public async Task<List<TaskSummaryDto>> GetDependentTasksAsync(string userId)
        {
            _logger.LogInformation("Fetching dependent tasks for user {UserId}", userId);

            var tasks = await _dbContext.Dependencies
                .Include(d => d.TaskItem)
                .Where(d => d.DependsOnTaskId.HasValue && _dbContext.Tasks.Any(t => t.Id == d.DependsOnTaskId && t.CreatedBy == userId))
                .Select(d => new TaskSummaryDto
                {
                    Id = d.TaskItem.Id,
                    Name = d.TaskItem.TaskName,
                    Status = d.TaskItem.Status,
                    DueDate = d.TaskItem.DueDate.ToString("dd/MM/yyyy"),
                    AssignedTo = d.AssignedTo
                })
                .Distinct()
                .ToListAsync();

            _logger.LogInformation("Found {Count} dependent tasks", tasks.Count);
            return tasks;
        }

        public async Task<object> GetDependencyTasksByUserAsync(string userId)
        {
            _logger.LogInformation("Fetching dependency tasks for user {UserId}", userId);

            var result = await _dbContext.Tasks
                .Where(e => e.CreatedBy == userId && e.Status == "In Progress")
                .Select(p => p.Id)
                .ToListAsync();

            var dependencies = await _dbContext.taskDependencyFacts
                .Where(e => result.Contains(e.TaskId))
                .Select(p => p.TaskId)
                .Distinct()
                .ToListAsync();

            var dependenciesDetails = await _dbContext.Tasks
                .Where(p => dependencies.Contains(p.Id))
                .ToListAsync();

            _logger.LogInformation("Found {Count} dependency tasks", dependenciesDetails.Count);
            return dependenciesDetails;
        }

        public async Task<object> GetDependentTasksByUserAsync(string userId)
        {
            var result = _dbContext.Tasks.Where(e => e.CreatedBy == userId && e.Status == "In Progress").Select(p => p.Id).ToList();
            var dependent = _dbContext.DependencyFacts.Where(p => result.Contains(p.DependsOnTaskId.Value)).Select(m => m.DependsOnTaskId.Value).Distinct().ToList();
            var tasks = _dbContext.Tasks.Where(e => dependent.Contains(e.Id)).ToList();
            return tasks;
        }

        public async Task<TaskStatsDto> GetTaskStatsAsync(string userId)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;

            _logger.LogInformation("Fetching task stats for user {UserId} on {Today}", userId, today);

            var ToDo = await _dbContext.Tasks
                .Where(t => t.StartDate.Date <= today && t.DueDate.Date >= today && t.CreatedBy == userId && t.Status == "In Progress")
                .CountAsync();

            var Overdue = await _dbContext.Tasks
                .Where(t => t.DueDate.Date < today && t.CreatedBy == userId && t.Status == "In Progress")
                .CountAsync();

            var result = await _dbContext.Tasks
                .Where(e => e.CreatedBy == userId && e.Status == "In Progress")
                .Select(p => p.Id)
                .ToListAsync();

            var dependencies = await _dbContext.taskDependencyFacts
                .Where(e => result.Contains(e.TaskId))
                .Select(p => p.TaskId)
                .Distinct()
                .ToListAsync();

            var Waiting = await _dbContext.Tasks
                .Where(p => dependencies.Contains(p.Id))
                .CountAsync();

            var InProgress = await _dbContext.Tasks
                .Where(p => p.Status == "In Progress" && p.CreatedBy == userId)
                .CountAsync();

            var Completed = await _dbContext.Tasks
                .Where(p => p.Status == "Completed" && p.CreatedBy == userId)
                .CountAsync();

            var stats = new TaskStatsDto
            {
                ToDo = ToDo,
                InProgress = InProgress,
                Completed = Completed,
                Waiting = Waiting,
                Overdue = Overdue
            };

            _logger.LogInformation("Task stats: ToDo={ToDo}, InProgress={InProgress}, Completed={Completed}, Waiting={Waiting}, Overdue={Overdue}",
                stats.ToDo, stats.InProgress, stats.Completed, stats.Waiting, stats.Overdue);

            return stats;
        }
    }
}