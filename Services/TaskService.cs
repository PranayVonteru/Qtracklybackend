using Demoproject.Data;
using Demoproject.Dtos;
using Demoproject.Models;
using Demoproject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Demoproject.Services
{
    public class TaskService : ITaskService
    {
        private readonly QTraklyDBContext _context;
        private readonly ITaskLogService _taskLogService;
        private readonly IUserService _userService;
        private readonly ILogger<TaskService> _logger;
        private readonly TimeZoneInfo _istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public TaskService(
            QTraklyDBContext context,
            ITaskLogService taskLogService,
            IUserService userService,
            ILogger<TaskService> logger)
        {
            _context = context;
            _taskLogService = taskLogService;
            _userService = userService;
            _logger = logger;
        }

        private DateTime NormalizeDateTime(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            }
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _istTimeZone);
            }
            return dateTime;
        }

        public async Task<(TaskItem, string)> CreateTaskAsync(TaskItemCreateDto dto, string userId)
        {
            if (string.IsNullOrEmpty(dto.TaskName))
                return (null, "Task name is required.");

            var validStatuses = new[] { "Not Started", "In Progress", "Completed" };
            if (!validStatuses.Contains(dto.Status))
                return (null, $"Invalid Status. Valid values are: {string.Join(", ", validStatuses)}");

            var startDate = dto.StartDate == default
                ? DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _istTimeZone), DateTimeKind.Local)
                : NormalizeDateTime(dto.StartDate);

            var dueDate = dto.DueDate == default ? startDate : NormalizeDateTime(dto.DueDate);

            if (startDate > dueDate)
                return (null, "StartDate cannot be after DueDate.");

            if (dto.Status == "Not Started" && dto.CompletedHours > 0)
                return (null, "Tasks with 'Not Started' status cannot have completed hours.");

            decimal workedHours = Math.Round(dto.WorkedHours, 2);

            var task = new TaskItem
            {
                TaskName = dto.TaskName,
                Priority = dto.Priority ?? "Medium",
                Status = dto.Status ?? "Not Started",
                Description = dto.Description,
                StartDate = startDate,
                DueDate = dueDate,
                // AssignedTo=null, // REMOVED - Column doesn't exist in database
                EstimatedHours = Math.Round(dto.EstimatedHours, 2),
                CompletedHours = workedHours,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                HasSubtask = dto.HasSubtask ?? "No"
            };

            try
            {
                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                var taskDate = new TaskDateWorkedHours
                {
                    WorkedHours = workedHours,
                    TaskId = task.Id,
                    DateTime = startDate
                };

                _context.TaskDateworkedHours.Add(taskDate);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return (null, "An error occurred while creating the task.");
            }

            return (task, "Task created successfully.");
        }

        public async Task<(TaskItem, string)> UpdateTaskAsync(TaskItemUpdateDto dto, string userId)
        {
            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(e => e.Id == dto.TaskId);
                if (task == null)
                {
                    return (null, "Task not found.");
                }

                if (task.CreatedBy != userId)
                {
                    return (null, "User ID does not match.");
                }

                task.CompletedHours = task.CompletedHours + dto.WorkedHours;
                task.Status = dto.Status;
                task.Priority = dto.Priority;
                task.UpdatedAt = dto.UpdateDate;
                task.HasSubtask = dto.HasSubtask ?? "No";
                _context.Update(task);
                await _context.SaveChangesAsync();

                var taskwork = new TaskDateWorkedHours
                {
                    TaskId = task.Id,
                    DateTime = dto.UpdateDate,
                    WorkedHours = dto.WorkedHours,
                };
                _context.TaskDateworkedHours.Add(taskwork);
                await _context.SaveChangesAsync();

                return (task, "Task updated successfully.");
            }
            catch (Exception ex)
            {
                return (null, "An error occurred while updating the task.");
            }
        }

        public async Task<bool> CheckSubtaskExistsAsync(int taskId)
        {
            return await _context.SubTasks.AnyAsync(s => s.TaskItemId == taskId);
        }

        public async Task<PaginatedResult<TaskItemDto>> GetAllTasksAsync(
            string userId,
            int pageNumber = 1,
            int pageSize = 5,
            string? priority = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? dueDate = null,
            string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 5;
                if (pageSize > 100) pageSize = 100;

                var query = _context.Tasks
                    .Where(t => t.CreatedBy == userId);

                // Apply filters
                if (!string.IsNullOrEmpty(priority))
                {
                    query = query.Where(t => t.Priority == priority);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status == status);
                }

                if (startDate.HasValue)
                {
                    var normalizedStartDate = NormalizeDateTime(startDate.Value);
                    query = query.Where(t => t.StartDate.Date == normalizedStartDate.Date);
                }

                if (dueDate.HasValue)
                {
                    var normalizedDueDate = NormalizeDateTime(dueDate.Value);
                    query = query.Where(t => t.DueDate.Date == normalizedDueDate.Date);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    query = query.Where(t => t.TaskName.ToLower().Contains(searchTerm) ||
                                            (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (pageNumber - 1) * pageSize;

                var tasks = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(t => new TaskItemDto
                    {
                        Id = t.Id,
                        TaskName = t.TaskName,
                        Priority = t.Priority,
                        Status = t.Status,
                        Description = t.Description,
                        StartDate = t.StartDate,
                        DueDate = t.DueDate,
                        EstimatedHours = t.EstimatedHours,
                        CompletedHours = t.CompletedHours,
                        WorkedHours = t.CompletedHours,
                        PrevCompletedHours = t.PrevCompletedHours,
                        CurrentDayEfforts = t.CurrentDayEfforts,
                        CreatedBy = t.CreatedBy,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        SubTasksCount = t.SubTasks.Count,
                        DependenciesCount = t.Dependencies.Count,
                        HasSubtask = t.HasSubtask
                    })
                    .ToListAsync();

                return new PaginatedResult<TaskItemDto>
                {
                    Data = tasks,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                    //HasPreviousPage = pageNumber > 1,
                    //HasNextPage = pageNumber < totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated tasks for user {UserId}. PageNumber: {PageNumber}, PageSize: {PageSize}, Filters: {Priority}, {Status}, {StartDate}, {DueDate}, {SearchTerm}",
                    userId, pageNumber, pageSize, priority, status, startDate, dueDate, searchTerm);
                throw;
            }
        }


        //public async Task<PaginatedResult<TaskItemDto>> GetAllTasksAsync(
        //string userId,
        //int pageNumber = 1,
        //int pageSize = 5,
        //string? priority = null,
        //string? status = null,
        //DateTime? startDate = null,
        //DateTime? dueDate = null)
        //{
        //    try
        //    {
        //        if (pageNumber < 1) pageNumber = 1;
        //        if (pageSize < 1) pageSize = 5;
        //        if (pageSize > 100) pageSize = 100;

        //        var query = _context.Tasks
        //            .Where(t => t.CreatedBy == userId);

        //        // Apply filters
        //        if (!string.IsNullOrEmpty(priority))
        //        {
        //            query = query.Where(t => t.Priority == priority);
        //        }

        //        if (!string.IsNullOrEmpty(status))
        //        {
        //            query = query.Where(t => t.Status == status);
        //        }

        //        if (startDate.HasValue)
        //        {
        //            var normalizedStartDate = NormalizeDateTime(startDate.Value);
        //            query = query.Where(t => t.StartDate.Date == normalizedStartDate.Date);
        //        }

        //        if (dueDate.HasValue)
        //        {
        //            var normalizedDueDate = NormalizeDateTime(dueDate.Value);
        //            query = query.Where(t => t.DueDate.Date == normalizedDueDate.Date);
        //        }

        //        var totalCount = await query.CountAsync();
        //        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        //        var skip = (pageNumber - 1) * pageSize;

        //        var tasks = await query
        //            .OrderByDescending(t => t.CreatedAt)
        //            .Skip(skip)
        //            .Take(pageSize)
        //            .Select(t => new TaskItemDto
        //            {
        //                Id = t.Id,
        //                TaskName = t.TaskName,
        //                Priority = t.Priority,
        //                Status = t.Status,
        //                Description = t.Description,
        //                StartDate = t.StartDate,
        //                DueDate = t.DueDate,
        //                EstimatedHours = t.EstimatedHours,
        //                CompletedHours = t.CompletedHours,
        //                WorkedHours = t.CompletedHours,
        //                PrevCompletedHours = t.PrevCompletedHours,
        //                CurrentDayEfforts = t.CurrentDayEfforts,
        //                CreatedBy = t.CreatedBy,
        //                CreatedAt = t.CreatedAt,
        //                UpdatedAt = t.UpdatedAt,
        //                SubTasksCount = t.SubTasks.Count,
        //                DependenciesCount = t.Dependencies.Count,
        //                HasSubtask = t.HasSubtask
        //            })
        //            .ToListAsync();

        //        return new PaginatedResult<TaskItemDto>
        //        {
        //            Data = tasks,
        //            TotalCount = totalCount,
        //            PageNumber = pageNumber,
        //            PageSize = pageSize,
        //            TotalPages = totalPages
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching paginated tasks for user {UserId}. PageNumber: {PageNumber}, PageSize: {PageSize}, Filters: {Priority}, {Status}, {StartDate}, {DueDate}",
        //            userId, pageNumber, pageSize, priority, status, startDate, dueDate);
        //        throw;
        //    }
        //}

        public async Task<(TaskItemDetailDto, string)> GetTaskAsync(int id, string userId)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.SubTasks)
                    .Include(t => t.Dependencies)
                    .FirstOrDefaultAsync(t => t.Id == id && t.CreatedBy == userId);

                if (task == null)
                    return (null, "Task not found or you don't have permission.");

                var taskDto = new TaskItemDetailDto
                {
                    Id = task.Id,
                    TaskName = task.TaskName,
                    Priority = task.Priority,
                    Status = task.Status,
                    Description = task.Description,
                    StartDate = task.StartDate,
                    DueDate = task.DueDate,
                    EstimatedHours = task.EstimatedHours,
                    CompletedHours = task.CompletedHours,
                    CreatedBy = task.CreatedBy,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    HasSubtask = task.HasSubtask,
                    SubTasks = task.SubTasks.Select(s => new SubTaskDto
                    {
                        Id = s.Id,
                        SubTaskName = s.SubTaskName,
                        Status = s.Status,
                        Description = s.Description,
                        StartDate = s.StartDate,
                        DueDate = s.DueDate,
                        CompletedHours = s.CompletedHours,
                        TaskItemId = s.TaskItemId,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    }).ToList(),
                    Dependencies = task.Dependencies.Select(d => new DependencyDto
                    {
                        Id = d.Id,
                        TaskItemId = d.TaskItemId,
                        DependsOnTaskId = d.DependsOnTaskId,
                        AssignedTo = d.AssignedTo,
                        Notes = d.Notes,
                        TaskName = _context.Tasks.FirstOrDefault(t => t.Id == d.TaskItemId)?.TaskName ?? "",
                        DependsOnTaskName = _context.Tasks.FirstOrDefault(t => t.Id == d.DependsOnTaskId)?.TaskName ?? ""
                    }).ToList()
                };

                return (taskDto, "Task retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task {TaskId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<string> DeleteTaskAsync(int id, string userId)
        {
            try
            {
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.CreatedBy == userId);

                if (task == null)
                    return "Task not found or you don't have permission.";

                if (task.Status == "In Progress")
                    return "Cannot delete a task that is In Progress.";

                _context.Tasks.Remove(task);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting task {TaskId} for user {UserId}", id, userId);
                    return "Failed to delete task due to a database error.";
                }

                return "Task deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId} for user {UserId}", id, userId);
                return "Failed to delete task: " + ex.Message;
            }
        }

        public async Task<List<TaskItemDto>> GetMyTasksAsync(string userId)
        {
            try
            {
                var tasks = await _context.Tasks
                    .Where(t => t.CreatedBy == userId)
                    .Select(t => new TaskItemDto
                    {
                        Id = t.Id,
                        TaskName = t.TaskName,
                        Priority = t.Priority,
                        Status = t.Status,
                        Description = t.Description,
                        StartDate = t.StartDate,
                        DueDate = t.DueDate,
                        EstimatedHours = t.EstimatedHours,
                        CompletedHours = t.CompletedHours,
                        WorkedHours = t.CompletedHours,
                        PrevCompletedHours = t.PrevCompletedHours,
                        CurrentDayEfforts = t.CurrentDayEfforts,
                        CreatedBy = t.CreatedBy,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        SubTasksCount = t.SubTasks.Count,
                        DependenciesCount = t.Dependencies.Count,
                        HasSubtask = t.HasSubtask
                    })
                    .ToListAsync();

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching my tasks for user {UserId}", userId);
                throw;
            }
        }

        private async Task UpdateTaskStatsAsync(string userId, string newStatus, bool isCreate, DateTime dueDate, string oldStatus = null)
        {
            try
            {
                var stats = await _context.TaskStats.FirstOrDefaultAsync(s => s.UserId == userId);
                if (stats == null)
                {
                    stats = new TaskStats { UserId = userId };
                    _context.TaskStats.Add(stats);
                }

                if (isCreate)
                {
                    switch (newStatus)
                    {
                        case "Not Started":
                            stats.NotStarted++;
                            break;
                        case "In Progress":
                            stats.InProgress++;
                            break;
                        case "Completed":
                            stats.Completed++;
                            break;
                    }
                }
                else if (oldStatus != null && newStatus != oldStatus)
                {
                    switch (oldStatus)
                    {
                        case "Not Started":
                            stats.NotStarted = Math.Max(0, stats.NotStarted - 1);
                            break;
                        case "In Progress":
                            stats.InProgress = Math.Max(0, stats.InProgress - 1);
                            break;
                        case "Completed":
                            stats.Completed = Math.Max(0, stats.Completed - 1);
                            break;
                    }

                    switch (newStatus)
                    {
                        case "Not Started":
                            stats.NotStarted++;
                            break;
                        case "In Progress":
                            stats.InProgress++;
                            break;
                        case "Completed":
                            stats.Completed++;
                            break;
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving task stats for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task stats for user {UserId}", userId);
            }
        }

        public async Task<List<TaskDependencyDto>> GetTaskDependenciesUserAsync(int id, string userId)
        {
            var result = await _context.Tasks
                .Where(userTask => userTask.Id == id && userTask.CreatedBy == userId)
                .Join(_context.DependencyFacts,
                    userTask => userTask.Id,
                    dependencyFact => dependencyFact.DependencyTaskId,
                    (userTask, dependencyFact) => new { userTask, dependencyFact })
                .Join(_context.Tasks,
                    combined => combined.dependencyFact.DependsOnTaskId,
                    dependentTask => dependentTask.Id,
                    (combined, dependentTask) => new TaskDependencyDto
                    {
                        TaskId = dependentTask.Id,
                        TaskName = dependentTask.TaskName,
                        DependencyTaskId = combined.userTask.Id,
                        DependencyTaskName = combined.userTask.TaskName,
                        DependencyTaskStatus = combined.userTask.Status ?? "No Status",
                        DependencyTaskEstimatedHours = dependentTask.EstimatedHours,
                        DependencyTaskDueDate = combined.userTask.DueDate,
                        DependencyTaskPriority = combined.userTask.Priority,
                        Status = dependentTask.Status ?? "Not Started",
                        DueDate = dependentTask.DueDate,
                        Priority = dependentTask.Priority ?? "Medium"
                    })
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.TaskName)
                .ToListAsync();

            return result;
        }

        public async Task<List<TaskItemDto>> GetAllTasksforPerformanceGlobalAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all tasks for performance");
                var tasks = await _context.Tasks
                    .Select(t => new TaskItemDto
                    {
                        Id = t.Id,
                        TaskName = t.TaskName,
                        Priority = t.Priority,
                        Status = t.Status,
                        Description = t.Description,
                        StartDate = t.StartDate,
                        DueDate = t.DueDate,
                        EstimatedHours = t.EstimatedHours,
                        CompletedHours = t.CompletedHours,
                        WorkedHours = t.CompletedHours,
                        PrevCompletedHours = t.PrevCompletedHours,
                        CurrentDayEfforts = t.CurrentDayEfforts,
                        CreatedBy = t.CreatedBy,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        SubTasksCount = t.SubTasks.Count,
                        DependenciesCount = t.Dependencies.Count,
                        HasSubtask = t.HasSubtask
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} tasks for performance", tasks.Count);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all global tasks for performance. Exception: {Exception}", ex.ToString());
                throw;
            }
        }

        public async Task<PaginatedResult<TaskItemDto>> GetAllTasksGlobalAsync(int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 5;
                if (pageSize > 100) pageSize = 100;

                var totalCount = await _context.Tasks.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (pageNumber - 1) * pageSize;

                var tasks = await _context.Tasks
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(t => new TaskItemDto
                    {
                        Id = t.Id,
                        TaskName = t.TaskName,
                        Priority = t.Priority,
                        Status = t.Status,
                        Description = t.Description,
                        StartDate = t.StartDate,
                        DueDate = t.DueDate,
                        EstimatedHours = t.EstimatedHours,
                        CompletedHours = t.CompletedHours,
                        WorkedHours = t.CompletedHours,
                        PrevCompletedHours = t.PrevCompletedHours,
                        CurrentDayEfforts = t.CurrentDayEfforts,
                        CreatedBy = t.CreatedBy,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        SubTasksCount = t.SubTasks.Count,
                        DependenciesCount = t.Dependencies.Count,
                        HasSubtask = t.HasSubtask
                    })
                    .ToListAsync();

                return new PaginatedResult<TaskItemDto>
                {
                    Data = tasks,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated global tasks. PageNumber: {PageNumber}, PageSize: {PageSize}, Exception: {Exception}", pageNumber, pageSize, ex.ToString());
                throw;
            }
        }

        public async Task<(TaskItem, string)> UpdateTaskWorkedHoursAsync(TaskWorkedHoursUpdateDto dto, string userId)
        {
            try
            {
                if (dto.WorkedHours < 0)
                    return (null, "WorkedHours must be non-negative.");

                if (string.IsNullOrEmpty(userId))
                    return (null, "Invalid UserId; cannot be null or empty.");

                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == dto.TaskId && t.CreatedBy == userId);

                if (task == null)
                    return (null, "Task not found or you don't have permission to update it.");

                var newWorkedHours = Math.Round(dto.WorkedHours, 2);
                var newCompletedHours = Math.Round(task.CompletedHours + newWorkedHours, 2);
                var updateDate = NormalizeDateTime(dto.UpdateDate);

                task.CompletedHours = newCompletedHours;
                task.UpdatedAt = DateTime.UtcNow;

                var taskUpdateLog = new TaskUpdateLog
                {
                    TaskId = task.Id,
                    UpdateDate = updateDate,
                    WorkedHours = newWorkedHours
                };

                _context.TaskUpdateLogs.Add(taskUpdateLog);
                _context.Entry(task).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Task {TaskId} updated with WorkedHours={WorkedHours}, New CompletedHours={CompletedHours}", task.Id, newWorkedHours, task.CompletedHours);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error updating task {TaskId} for user {UserId}. InnerException: {InnerException}", task.Id, userId, ex.InnerException?.Message);
                    return (null, $"Failed to update task due to a database error: {ex.InnerException?.Message ?? ex.Message}");
                }

                if (newWorkedHours > 0)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null)
                        return (task, "Task and update log created, but failed to create TaskLog: User not found.");

                    string team = "Other";
                    try
                    {
                        var roles = !string.IsNullOrEmpty(user.Roles)
                            ? JsonSerializer.Deserialize<List<string>>(user.Roles)
                            : new List<string>();
                        team = roles?.FirstOrDefault(r => r != "Manager" && r != "User") ?? "Other";
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize user roles for user {UserId}", userId);
                    }

                    var taskLogDate = updateDate.Kind == DateTimeKind.Utc
                        ? updateDate
                        : TimeZoneInfo.ConvertTimeToUtc(updateDate, _istTimeZone);

                    var taskLogDto = new TaskLogCreateDto
                    {
                        TaskId = task.Id,
                        UserId = userId,
                        HoursWorked = newWorkedHours,
                        Date = taskLogDate,
                        Team = team
                    };

                    var (taskLog, message) = await _taskLogService.CreateTaskLogAsync(taskLogDto, userId);
                    if (taskLog == null)
                        _logger.LogWarning("Failed to create TaskLog for task {TaskId}: {Message}", task.Id, message);
                }

                return (task, "Task worked hours and update log created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating task {TaskId} for user {UserId}", dto.TaskId, userId);
                return (null, $"Failed to update task: {ex.Message}");
            }
        }

        public async Task<List<TaskItemDto>> GetMyInProgressTasksAsync(string userId)

        {

            try

            {

                var tasks = await _context.Tasks

                    .Where(t => t.CreatedBy == userId && t.Status != "Completed")

                    .Select(t => new TaskItemDto

                    {

                        Id = t.Id,

                        TaskName = t.TaskName,

                        Priority = t.Priority,

                        Status = t.Status,

                        Description = t.Description,

                        StartDate = t.StartDate,

                        DueDate = t.DueDate,

                        EstimatedHours = t.EstimatedHours,

                        CompletedHours = t.CompletedHours,

                        CreatedBy = t.CreatedBy,

                        CreatedAt = t.CreatedAt,

                        UpdatedAt = t.UpdatedAt,

                        SubTasksCount = t.SubTasks.Count,

                        DependenciesCount = t.Dependencies.Count

                    })

                    .ToListAsync();

                return tasks;

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error fetching my tasks for user {UserId}", userId);

                throw;

            }

        }


        public async Task UpdateLinkTask(int taskId, int dependencyId)

        {

            try

            {

                // Get the matching dependency entries

                var dependenciesToUpdate = await _context.DependencyFacts

                    .Where(d => d.DependencyTaskId == dependencyId)

                    .ToListAsync();

                if (dependenciesToUpdate == null || dependenciesToUpdate.Count == 0)

                {

                    Console.WriteLine("No matching dependencies found.");

                    return;

                }

                // Update each DependsOnTaskId to the new taskId

                foreach (var dependency in dependenciesToUpdate)

                {

                    dependency.DependsOnTaskId = taskId;

                    dependency.Status = "Accepted";

                }

                // Save changes to the database

                await _context.SaveChangesAsync();

                Console.WriteLine("Task link updated successfully.");

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Error while updating task link: {ex.Message}");

                // Optionally log or throw

            }

        }


    }
}