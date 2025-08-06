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
    public class SubTaskService : ISubTaskService
    {
        private readonly QTraklyDBContext _dbContext;
        private readonly ILogger<SubTaskService> _logger;

        public SubTaskService(QTraklyDBContext dbContext, ILogger<SubTaskService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Object> CreateSubTaskAsync(SubTaskCreateDto dto, string userId)
        {
            
                var subtask = new SubTask
                {
                    SubTaskName = dto.SubTaskName,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = dto.Status,
                    StartDate = dto.StartDate,
                    DueDate = dto.DueDate,
                    EstimatedHours = dto.EstimatedHours,
                    CompletedHours = dto.WorkedHours,
                    TaskItemId = dto.TaskItemId,
                    CreatedAt= DateTime.UtcNow
                };

                _dbContext.SubTasks.Add(subtask);
               _dbContext.SaveChanges();

                var taskdate = new TaskDateWorkedHours
                {
                    DateTime = DateTime.UtcNow,
                    WorkedHours = dto.WorkedHours,
                    TaskId = dto.TaskItemId
                };

                _dbContext.TaskDateworkedHours.Add(taskdate);
                 _dbContext.SaveChanges();

            var task = _dbContext.Tasks.Where(p => p.Id == dto.TaskItemId).FirstOrDefault();
            task.CompletedHours += dto.WorkedHours;
            _dbContext.Update(task);
            _dbContext.SaveChanges();
     

                return new { Message = "Subtask created successfully." };

        }


        public async Task<(SubTaskDto, string)> GetSubTaskAsync(int id, string userId)
        {
            var subTask = await _dbContext.SubTasks
                .Include(st => st.TaskItem)
                .Where(st => st.Id == id && st.TaskItem.CreatedBy == userId)
                .Select(st => new SubTaskDto
                {
                    Id = st.Id,
                    SubTaskName = st.SubTaskName,
                    Status = st.Status,
                    Description = st.Description,
                    StartDate = st.StartDate,
                    DueDate = st.DueDate,
                    CompletedHours = st.CompletedHours,
                    EstimatedHours = st.EstimatedHours,
                    TaskItemId = st.TaskItemId,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return subTask == null ? (null, "SubTask not found or you don't have access to it.") : (subTask, null);
        }

        public async Task<List<SubTaskDto>> GetAllSubTasksAsync(string userId)
        {
            return await _dbContext.SubTasks
                .Include(st => st.TaskItem)
                .Where(st => st.TaskItem.CreatedBy == userId)
                .Select(st => new SubTaskDto
                {
                    Id = st.Id,
                    SubTaskName = st.SubTaskName,
                    Status = st.Status,
                    Description = st.Description,
                    StartDate = st.StartDate,
                    DueDate = st.DueDate,
                    CompletedHours = st.CompletedHours,
                    EstimatedHours=st.EstimatedHours,
                    TaskItemId = st.TaskItemId,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<string> DeleteSubTaskAsync(int id, string userId)
        {
            var subTask = await _dbContext.SubTasks
                .Include(st => st.TaskItem)
                .FirstOrDefaultAsync(st => st.Id == id && st.TaskItem.CreatedBy == userId);

            if (subTask == null)
                return "SubTask not found or you don't have permission to delete it.";

            if (subTask.Status == "In Progress")
                return "Cannot delete a subtask that is In Progress.";

            _dbContext.SubTasks.Remove(subTask);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subtask {SubTaskId} for user {UserId}", id, userId);
                return "Failed to delete subtask due to a database error.";
            }

            return "SubTask deleted successfully.";
        }

        public async Task<(SubTask, string)> UpdateSubTaskAsync(int id, SubTaskUpdateDto dto, string userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var task = await _dbContext.Tasks.FirstOrDefaultAsync(p => p.Id == dto.TaskItemId);
                if (task != null)
                {
                    task.CompletedHours += dto.WorkeddHours;
                }

                var subtask = await _dbContext.SubTasks.FirstOrDefaultAsync(p => p.Id == id);
                if (subtask == null)
                    return (null, "SubTask not found.");

                subtask.CompletedHours += dto.WorkeddHours;
                subtask.Status = dto.Status;
                subtask.Priority = dto.Priority;
                subtask.UpdatedAt = DateTime.UtcNow;

                var taskWork = new TaskDateWorkedHours
                {
                    DateTime = dto.Datetime,
                    WorkedHours = dto.WorkeddHours,
                    TaskId = dto.TaskItemId
                };

                _dbContext.TaskDateworkedHours.Add(taskWork);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (subtask, "SubTask updated successfully");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }




        public async Task<Object> Updatesubtask(int id, SubtaskupdatedDto dto)
        {
            var subtask = await _dbContext.SubTasks
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (subtask != null)
            {
                subtask.Status = dto.Status;
                subtask.UpdatedAt = DateTime.UtcNow;
                subtask.Priority = dto.Priority;
                subtask.CompletedHours = subtask.CompletedHours+dto.WorkedHours;
                _dbContext.Update(subtask);
                await _dbContext.SaveChangesAsync();
            }

            var taskdate = new TaskDateWorkedHours
            {
                DateTime = dto.Datetime,
                WorkedHours = dto.WorkedHours,
                TaskId = dto.TaskItemId
            };

            _dbContext.TaskDateworkedHours.Add(taskdate);
            await _dbContext.SaveChangesAsync();

            var task = await _dbContext.Tasks
                .Where(p => p.Id == dto.TaskItemId)
                .FirstOrDefaultAsync();

            if (task != null)
            {
                task.CompletedHours = task.CompletedHours+dto.WorkedHours;
                _dbContext.Update(task);
                await _dbContext.SaveChangesAsync();
            }

            return new { Message = "SubTask updated successfully" };
        }


        public async Task<(List<SubTaskDto>, string)> GetSubTasksByTaskIdAsync(int taskId, string userId)
        {
            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedBy == userId);

            if (task == null)
                return (null, "Task not found or you don't have access to it.");

            var subTasks = await _dbContext.SubTasks
                .Where(st => st.TaskItemId == taskId)
                .Select(st => new SubTaskDto
                {
                    Id = st.Id,
                    SubTaskName = st.SubTaskName,
                    Status = st.Status,
                    Description = st.Description,
                    StartDate = st.StartDate,
                    DueDate = st.DueDate,
                    CompletedHours = st.CompletedHours,
                    TaskItemId = st.TaskItemId,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt
                })
                .ToListAsync();

            return (subTasks, null);
        }
    }
}