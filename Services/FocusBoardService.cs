//using Demoproject.Data;
//using Demoproject.Dtos;
//using Demoproject.Models;
//using Demoproject.Services.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Graph.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Demoproject.Services
//{
//    public class FocusBoardService : IFocusBoardService
//    {
//        private readonly QTraklyDBContext _dbContext;
//        private readonly ILogger<FocusBoardService> _logger;

//        public FocusBoardService(QTraklyDBContext dbContext, ILogger<FocusBoardService> logger)
//        {
//            _dbContext = dbContext;
//            _logger = logger;
//        }

//        public async Task<Object> GetTodaysFocusAsync(string userId)
//        {
//            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
//                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;

//            var userTasks = _dbContext.Tasks
//                .Where(t => today >= t.StartDate && today <= t.DueDate && t.CreatedBy == userId && t.Status == "In Progress")
//                .ToList();

//            return  userTasks;
//        }


//        public async Task<Object> GetOverdueTasksAsync(string userId)
//        {
//            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
//                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;

//            var userTasks = _dbContext.Tasks
//                .Where(t =>today>t.DueDate && t.CreatedBy == userId && t.Status=="In Progress")
//                .ToList();
//            return userTasks;

//        }

//        public async Task<List<TaskSummaryDto>> GetWaitingTasksAsync(string userId)
//        {
//            return await _dbContext.Dependencies
//                .Include(d => d.TaskItem)
//                .Where(d => d.AssignedTo == userId && d.TaskItem.Status != "Completed")
//                .Select(d => new TaskSummaryDto
//                {
//                    Id = d.TaskItem.Id,
//                    Name = d.TaskItem.TaskName,
//                    Status = d.TaskItem.Status,
//                    DueDate = d.TaskItem.DueDate.ToString("dd/MM/yyyy"),
//                    AssignedTo = d.AssignedTo
//                })
//                .Distinct()
//                .ToListAsync();
//        }

//        public async Task<List<TaskSummaryDto>> GetDependentTasksAsync(string userId)
//        {
//            return await _dbContext.Dependencies
//                .Include(d => d.TaskItem)
//                .Where(d => d.DependsOnTaskId.HasValue && _dbContext.Tasks.Any(t => t.Id == d.DependsOnTaskId && t.CreatedBy == userId))
//                .Select(d => new TaskSummaryDto
//                {
//                    Id = d.TaskItem.Id,
//                    Name = d.TaskItem.TaskName,
//                    Status = d.TaskItem.Status,
//                    DueDate = d.TaskItem.DueDate.ToString("dd/MM/yyyy"),
//                    AssignedTo = d.AssignedTo
//                })
//                .Distinct()
//                .ToListAsync();
//        }
//        public async Task<Object> GetDependencyTasksByUserAsync(string userId)
//        {
//            // Get all tasks created by the user, then join with dependency fact table to find what they depend on
//            //var result = await _dbContext.Tasks
//            //    .Where(userTask => userTask.CreatedBy == userId)
//            //    .Join(_dbContext.DependencyFacts,
//            //        userTask => userTask.Id,  // Fixed: Join on Task.Id instead of DependencyTaskId
//            //        dependencyFact => dependencyFact.DependencyTaskId,
//            //        (userTask, dependencyFact) => new { userTask, dependencyFact })
//            //    .Join(_dbContext.Tasks,
//            //        combined => combined.dependencyFact.DependsOnTaskId,
//            //        dependentTask => dependentTask.Id,
//            //        (combined, dependentTask) => new TaskDependencyDto
//            //        {
//            //            // The user's task information
//            //            TaskId = combined.userTask.Id,
//            //            TaskName = combined.userTask.TaskName,
//            //            Status = combined.userTask.Status ?? "Not Started",
//            //            DueDate = combined.userTask.DueDate,
//            //            Priority = combined.userTask.Priority ?? "Medium",

//            //            // The task that the user's task depends on
//            //            DependencyTaskId = dependentTask.Id,
//            //            DependencyTaskName = dependentTask.TaskName,
//            //            DependencyTaskStatus = dependentTask.Status ?? "No Status",
//            //            DependencyTaskEstimatedHours = dependentTask.EstimatedHours,
//            //            DependencyTaskDueDate = dependentTask.DueDate,
//            //            DependencyTaskPriority = dependentTask.Priority ?? "Medium"
//            //        })
//            //    .OrderBy(t => t.DueDate)
//            //    .ThenBy(t => t.TaskName)
//            //    .ToListAsync();


//            var result = _dbContext.Tasks.Where(e => e.CreatedBy == userId && e.Status == "In Progress").Select(p => p.Id).ToList();

//            var dependencies = _dbContext.taskDependencyFacts.Where(e => result.Contains(e.TaskId)).Select(p => p.TaskId).Distinct().ToList();

//            var dependenciesDetails = _dbContext.Tasks.Where(p => dependencies.Contains(p.Id)).ToList();

//            return dependenciesDetails;
//        }

//        //public async Task<List<TaskDependencyDto>> GetDependentTasksByUserAsync(string userId)
//        //{
//        //    // Get all tasks created by the user, then join with dependency fact table and dependent tasks
//        //    var result = await _dbContext.Tasks
//        //        .Where(userTask => userTask.CreatedBy == userId)
//        //        .Join(_dbContext.DependencyFacts,
//        //            userTask => userTask.DependencyTaskId,
//        //            dependencyFact => dependencyFact.DependencyTaskId,
//        //            (userTask, dependencyFact) => new { userTask, dependencyFact })
//        //        .Join(_dbContext.Tasks,
//        //            combined => combined.dependencyFact.DependsOnTaskId,
//        //            dependentTask => dependentTask.Id,
//        //            (combined, dependentTask) => new TaskDependencyDto
//        //            {
//        //                TaskId = dependentTask.Id,
//        //                TaskName = dependentTask.TaskName,
//        //                DependencyTaskId = combined.userTask.Id,
//        //                DependencyTaskName = combined.userTask.TaskName,
//        //                DependencyTaskStatus = combined.userTask.Status ?? "No Status",
//        //                DependencyTaskEstimatedHours = dependentTask.EstimatedHours,
//        //                DependencyTaskDueDate = combined.userTask.DueDate,
//        //                DependencyTaskPriority = combined.userTask.Priority,
//        //                Status = dependentTask.Status ?? "Not Started",
//        //                DueDate = dependentTask.DueDate,
//        //                Priority = dependentTask.Priority ?? "Medium"
//        //            })
//        //        .OrderBy(t => t.DueDate)
//        //        .ThenBy(t => t.TaskName)
//        //        .ToListAsync();

//        //    return result;
//        //}

//        public  async Task<Object> GetDependentTasksByUserAsync(string userId)
//        {
//            //var result = await _dbContext.DependencyFacts
//            //    .Where(dependencyFact => dependencyFact.TargetUserId == userId)
//            //    .Join(_dbContext.DependencyRequests,
//            //        dependencyFact => dependencyFact.Id,
//            //        dependencyRequest => dependencyRequest.DependencyTaskId,
//            //        (dependencyFact, dependencyRequest) => new { dependencyFact, dependencyRequest })
//            //    .OrderBy(x => x.dependencyRequest.RequestedDate)
//            //    .ThenBy(x => x.dependencyRequest.TaskName)
//            //    .Select(x => new TaskDependencyDto
//            //    {
//            //        TaskId = x.dependencyFact.DependencyTaskId,
//            //        //DependencyTaskId = x.dependencyFact.DependsOnTaskId,
//            //        //DependencyFactStatus = x.dependencyFact.Status,
//            //        TaskName = x.dependencyRequest.TaskName,
//            //        DependencyTaskName = x.dependencyRequest.RequestedTask,
//            //        Priority = x.dependencyRequest.Priority ?? "Medium",
//            //        //DependencyTaskEstimatedHours = x.dependencyRequest.EstimatedImpact,
//            //        DependencyTaskDueDate = x.dependencyRequest.RequestedDate,
//            //        RequestId = x.dependencyRequest.DependencyTaskId,
//            //        RequestDescription = x.dependencyRequest.Description,
//            //        RequestCreatedDate = x.dependencyRequest.RequestedDate
//            //    })
//            //    .ToListAsync();


//            var result = _dbContext.Tasks.Where(e => e.CreatedBy == userId && e.Status == "In Progress").Select(p => p.Id).ToList();
//            var dependent = _dbContext.DependencyFacts.Where(p => result.Contains(p.DependsOnTaskId.Value)).Select(m => m.DependsOnTaskId.Value).Distinct().ToList();
//            var tasks =  _dbContext.Tasks.Where(e => dependent.Contains(e.Id)).ToList();
//            return tasks;
//        }



//        // Alternative method if you want to get tasks that are dependencies for other tasks

//        public async Task<TaskStatsDto> GetTaskStatsAsync(string userId)
//        {
//            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
//                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;

//            var ToDo = await _dbContext.Tasks
//                .Where(t => today >= t.StartDate && today <= t.DueDate && t.CreatedBy == userId && t.Status == "In Progress")
//                .CountAsync();

//            var Overdue = await _dbContext.Tasks
//                .Where(t => today > t.DueDate && t.CreatedBy == userId && t.Status == "In Progress")
//                .CountAsync();

//            var result = await _dbContext.Tasks
//                .Where(e => e.CreatedBy == userId && e.Status == "In Progress")
//                .Select(p => p.Id)
//                .ToListAsync();

//            var dependencies = await _dbContext.taskDependencyFacts
//                .Where(e => result.Contains(e.TaskId))
//                .Select(p => p.TaskId)
//                .Distinct()
//                .ToListAsync();

//            var Waiting = await _dbContext.Tasks
//                .Where(p => dependencies.Contains(p.Id))
//                .CountAsync();

//            var InProgress = await _dbContext.Tasks
//                .Where(p => p.Status == "In Progress" && p.CreatedBy == userId)
//                .CountAsync();

//            var Completed = await _dbContext.Tasks
//                .Where(p => p.Status == "Completed" && p.CreatedBy == userId)
//                .CountAsync();

//            return new TaskStatsDto
//            {
//                ToDo = ToDo,
//                InProgress = InProgress,
//                Completed = Completed,
//                Waiting = Waiting,
//                Overdue = Overdue
//            };
//        }

//    }
//}



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