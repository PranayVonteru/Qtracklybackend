using Demoproject.Data;
using Demoproject.Dtos;
using Demoproject.Models;
using Demoproject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Demoproject.Services
{
    public class TaskLogService : ITaskLogService
    {
        private readonly QTraklyDBContext _context;
        private readonly ILogger<TaskLogService> _logger;

        public TaskLogService(QTraklyDBContext context, ILogger<TaskLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(TaskLogDto, string)> CreateTaskLogAsync(TaskLogCreateDto inputTaskLogDto, string createdBy)
        {
            try
            {
                // Validate TaskId or SubTaskId exists
                if (inputTaskLogDto.TaskId.HasValue)
                {
                    var task = await _context.Tasks.FindAsync(inputTaskLogDto.TaskId);
                    if (task == null)
                        return (null, "Task not found.");
                }
                else if (inputTaskLogDto.SubTaskId.HasValue)
                {
                    var subTask = await _context.SubTasks.FindAsync(inputTaskLogDto.SubTaskId);
                    if (subTask == null)
                        return (null, "SubTask not found.");
                }
                else
                {
                    return (null, "Either TaskId or SubTaskId must be provided.");
                }

                // Validate UserId - Look up by string UserId to get integer Id
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == inputTaskLogDto.UserId);
                if (user == null)
                    return (null, "User not found.");

                // Derive team from user.Roles if not provided
                string team = inputTaskLogDto.Team;
                if (string.IsNullOrEmpty(team))
                {
                    try
                    {
                        var roles = JsonSerializer.Deserialize<List<string>>(user.Roles) ?? new List<string>();
                        team = roles.FirstOrDefault(r => r != "Manager" && r != "User") ?? "Other";
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize roles for user {UserId}. Defaulting team to 'Other'.", inputTaskLogDto.UserId);
                        team = "Other";
                    }
                }

                var taskLog = new TaskLog
                {
                    TaskId = inputTaskLogDto.TaskId,
                    SubTaskId = inputTaskLogDto.SubTaskId,
                    UserId = user.Id, // Use integer Id
                    Team = team,
                    Date = inputTaskLogDto.Date,
                    HoursWorked = inputTaskLogDto.HoursWorked,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TaskLogs.Add(taskLog);
                await _context.SaveChangesAsync();

                // Skip updating CompletedHours (handled in TaskService)
                // This avoids duplicate increments since TaskService sets CompletedHours and creates TaskLog

                var outputTaskLogDto = new TaskLogDto
                {
                    Id = taskLog.Id,
                    TaskId = taskLog.TaskId,
                    SubTaskId = taskLog.SubTaskId,
                    UserId = user.UserId, // Return string UserId
                    Team = taskLog.Team,
                    Date = taskLog.Date,
                    HoursWorked = taskLog.HoursWorked,
                    CreatedAt = taskLog.CreatedAt,
                    UpdatedAt = taskLog.UpdatedAt
                };

                return (outputTaskLogDto, "Task log created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task log for user {UserId}", inputTaskLogDto.UserId);
                throw;
            }
        }

        public async Task<List<TaskLogDto>> GetTaskLogsByUserAsync(string userId, int? taskId, int? subTaskId)
        {
            try
            {
                var query = _context.TaskLogs
                    .Include(tl => tl.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user != null)
                        query = query.Where(tl => tl.UserId == user.Id);
                    else
                        return new List<TaskLogDto>();
                }

                if (taskId.HasValue)
                    query = query.Where(tl => tl.TaskId == taskId);

                if (subTaskId.HasValue)
                    query = query.Where(tl => tl.SubTaskId == subTaskId);

                var taskLogs = await query
                    .Select(tl => new TaskLogDto
                    {
                        Id = tl.Id,
                        TaskId = tl.TaskId,
                        SubTaskId = tl.SubTaskId,
                        UserId = tl.User.UserId,
                        Team = tl.Team,
                        Date = tl.Date,
                        HoursWorked = tl.HoursWorked,
                        CreatedAt = tl.CreatedAt,
                        UpdatedAt = tl.UpdatedAt
                    })
                    .ToListAsync();

                return taskLogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task logs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<TaskLogDto>> GetTaskLogsByTaskIdAsync(int taskId)
        {
            try
            {
                var taskLogs = await _context.TaskLogs
                    .Include(tl => tl.User)
                    .Where(tl => tl.TaskId == taskId)
                    .Select(tl => new TaskLogDto
                    {
                        Id = tl.Id,
                        TaskId = tl.TaskId,
                        SubTaskId = tl.SubTaskId,
                        UserId = tl.User.UserId,
                        Team = tl.Team,
                        Date = tl.Date,
                        HoursWorked = tl.HoursWorked,
                        CreatedAt = tl.CreatedAt,
                        UpdatedAt = tl.UpdatedAt
                    })
                    .ToListAsync();

                return taskLogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task logs for task {TaskId}", taskId);
                throw;
            }
        }
    }
}