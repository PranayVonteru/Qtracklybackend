//using Demoproject.Data;
//using Demoproject.Dtos;
//using Demoproject.Models;
//using Demoproject.Services.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Demoproject.Services
//{
//    public class SubTaskService : ISubTaskService
//    {
//        private readonly QTraklyDBContext _dbContext;
//        private readonly ILogger<SubTaskService> _logger;

//        public SubTaskService(QTraklyDBContext dbContext, ILogger<SubTaskService> logger)
//        {
//            _dbContext = dbContext;
//            _logger = logger;
//        }

//        public async Task<Object> CreateSubTaskAsync(SubTaskCreateDto dto, string userId)
//        {

//                var subtask = new SubTask
//                {
//                    SubTaskName = dto.SubTaskName,
//                    Description = dto.Description,
//                    Priority = dto.Priority,
//                    Status = dto.Status,
//                    StartDate = dto.StartDate,
//                    DueDate = dto.DueDate,
//                    EstimatedHours = dto.EstimatedHours,
//                    CompletedHours = dto.WorkedHours,
//                    TaskItemId = dto.TaskItemId,
//                    CreatedAt= DateTime.UtcNow
//                };

//                _dbContext.SubTasks.Add(subtask);
//               _dbContext.SaveChanges();

//                var taskdate = new TaskDateWorkedHours
//                {
//                    DateTime = DateTime.UtcNow,
//                    WorkedHours = dto.WorkedHours,
//                    TaskId = dto.TaskItemId
//                };

//                _dbContext.TaskDateworkedHours.Add(taskdate);
//                 _dbContext.SaveChanges();

//            var task = _dbContext.Tasks.Where(p => p.Id == dto.TaskItemId).FirstOrDefault();
//            task.CompletedHours += dto.WorkedHours;
//            _dbContext.Update(task);
//            _dbContext.SaveChanges();


//                return new { Message = "Subtask created successfully." };

//        }


//        public async Task<(SubTaskDto, string)> GetSubTaskAsync(int id, string userId)
//        {
//            var subTask = await _dbContext.SubTasks
//                .Include(st => st.TaskItem)
//                .Where(st => st.Id == id && st.TaskItem.CreatedBy == userId)
//                .Select(st => new SubTaskDto
//                {
//                    Id = st.Id,
//                    SubTaskName = st.SubTaskName,
//                    Status = st.Status,
//                    Description = st.Description,
//                    StartDate = st.StartDate,
//                    DueDate = st.DueDate,
//                    CompletedHours = st.CompletedHours,
//                    EstimatedHours = st.EstimatedHours,
//                    TaskItemId = st.TaskItemId,
//                    CreatedAt = st.CreatedAt,
//                    UpdatedAt = st.UpdatedAt
//                })
//                .FirstOrDefaultAsync();

//            return subTask == null ? (null, "SubTask not found or you don't have access to it.") : (subTask, null);
//        }

//        public async Task<List<SubTaskDto>> GetAllSubTasksAsync(string userId)
//        {
//            return await _dbContext.SubTasks
//                .Include(st => st.TaskItem)
//                .Where(st => st.TaskItem.CreatedBy == userId)
//                .Select(st => new SubTaskDto
//                {
//                    Id = st.Id,
//                    SubTaskName = st.SubTaskName,
//                    Status = st.Status,
//                    Description = st.Description,
//                    StartDate = st.StartDate,
//                    DueDate = st.DueDate,
//                    CompletedHours = st.CompletedHours,
//                    EstimatedHours=st.EstimatedHours,
//                    TaskItemId = st.TaskItemId,
//                    CreatedAt = st.CreatedAt,
//                    UpdatedAt = st.UpdatedAt
//                })
//                .ToListAsync();
//        }

//        public async Task<string> DeleteSubTaskAsync(int id, string userId)
//        {
//            var subTask = await _dbContext.SubTasks
//                .Include(st => st.TaskItem)
//                .FirstOrDefaultAsync(st => st.Id == id && st.TaskItem.CreatedBy == userId);

//            if (subTask == null)
//                return "SubTask not found or you don't have permission to delete it.";

//            if (subTask.Status == "In Progress")
//                return "Cannot delete a subtask that is In Progress.";

//            _dbContext.SubTasks.Remove(subTask);
//            try
//            {
//                await _dbContext.SaveChangesAsync();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting subtask {SubTaskId} for user {UserId}", id, userId);
//                return "Failed to delete subtask due to a database error.";
//            }

//            return "SubTask deleted successfully.";
//        }

//        public async Task<(SubTask, string)> UpdateSubTaskAsync(int id, SubTaskUpdateDto dto, string userId)
//        {
//            using var transaction = await _dbContext.Database.BeginTransactionAsync();

//            try
//            {
//                var task = await _dbContext.Tasks.FirstOrDefaultAsync(p => p.Id == dto.TaskItemId);
//                if (task != null)
//                {
//                    task.CompletedHours += dto.WorkeddHours;
//                }

//                var subtask = await _dbContext.SubTasks.FirstOrDefaultAsync(p => p.Id == id);
//                if (subtask == null)
//                    return (null, "SubTask not found.");

//                subtask.CompletedHours += dto.WorkeddHours;
//                subtask.Status = dto.Status;
//                subtask.Priority = dto.Priority;
//                subtask.UpdatedAt = DateTime.UtcNow;

//                var taskWork = new TaskDateWorkedHours
//                {
//                    DateTime = dto.Datetime,
//                    WorkedHours = dto.WorkeddHours,
//                    TaskId = dto.TaskItemId
//                };

//                _dbContext.TaskDateworkedHours.Add(taskWork);
//                await _dbContext.SaveChangesAsync();
//                await transaction.CommitAsync();

//                return (subtask, "SubTask updated successfully");
//            }
//            catch (Exception)
//            {
//                await transaction.RollbackAsync();
//                throw;
//            }
//        }




//        public async Task<Object> Updatesubtask(int id, SubtaskupdatedDto dto)
//        {
//            var subtask = await _dbContext.SubTasks
//                .Where(p => p.Id == id)
//                .FirstOrDefaultAsync();

//            if (subtask != null)
//            {
//                subtask.Status = dto.Status;
//                subtask.UpdatedAt = DateTime.UtcNow;
//                subtask.Priority = dto.Priority;
//                subtask.CompletedHours = subtask.CompletedHours+dto.WorkedHours;
//                _dbContext.Update(subtask);
//                await _dbContext.SaveChangesAsync();
//            }

//            var taskdate = new TaskDateWorkedHours
//            {
//                DateTime = dto.Datetime,
//                WorkedHours = dto.WorkedHours,
//                TaskId = dto.TaskItemId
//            };

//            _dbContext.TaskDateworkedHours.Add(taskdate);
//            await _dbContext.SaveChangesAsync();

//            var task = await _dbContext.Tasks
//                .Where(p => p.Id == dto.TaskItemId)
//                .FirstOrDefaultAsync();

//            if (task != null)
//            {
//                task.CompletedHours = task.CompletedHours+dto.WorkedHours;
//                _dbContext.Update(task);
//                await _dbContext.SaveChangesAsync();
//            }

//            return new { Message = "SubTask updated successfully" };
//        }


//        public async Task<(List<SubTaskDto>, string)> GetSubTasksByTaskIdAsync(int taskId, string userId)
//        {
//            var task = await _dbContext.Tasks
//                .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedBy == userId);

//            if (task == null)
//                return (null, "Task not found or you don't have access to it.");

//            var subTasks = await _dbContext.SubTasks
//                .Where(st => st.TaskItemId == taskId)
//                .Select(st => new SubTaskDto
//                {
//                    Id = st.Id,
//                    SubTaskName = st.SubTaskName,
//                    Status = st.Status,
//                    Description = st.Description,
//                    StartDate = st.StartDate,
//                    DueDate = st.DueDate,
//                    CompletedHours = st.CompletedHours,
//                    TaskItemId = st.TaskItemId,
//                    CreatedAt = st.CreatedAt,
//                    UpdatedAt = st.UpdatedAt
//                })
//                .ToListAsync();

//            return (subTasks, null);
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
    public class SubTaskService : ISubTaskService
    {
        private readonly QTraklyDBContext _dbContext;
        private readonly ILogger<SubTaskService> _logger;

        public SubTaskService(QTraklyDBContext dbContext, ILogger<SubTaskService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // Helper method to update parent task status based on subtask statuses
        private async Task UpdateParentTaskStatusAsync(int taskId)
        {
            // Fetch all subtasks for the parent task
            var subtasks = await _dbContext.SubTasks
                .Where(st => st.TaskItemId == taskId)
                .ToListAsync();

            var parentTask = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (parentTask == null)
            {
                _logger.LogWarning("Parent task {TaskId} not found when updating status.", taskId);
                return;
            }

            // Determine new parent status
            string newStatus;
            if (!subtasks.Any())
            {
                // If no subtasks, keep the current status or set to "Not Started" if undefined
                newStatus = parentTask.Status ?? "Not Started";
            }
            else if (subtasks.All(st => st.Status == "Completed"))
            {
                newStatus = "Completed";
            }
            else if (subtasks.Any(st => st.Status == "In Progress" || st.Status == "Completed"))
            {
                newStatus = "In Progress";
            }
            else
            {
                newStatus = "Not Started";
            }

            // Update parent task if status changed
            if (parentTask.Status != newStatus)
            {
                parentTask.Status = newStatus;
                parentTask.UpdatedAt = DateTime.UtcNow;
                _dbContext.Update(parentTask);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Updated parent task {TaskId} status to {NewStatus}.", taskId, newStatus);
            }
        }

        public async Task<object> CreateSubTaskAsync(SubTaskCreateDto dto, string userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
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
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.SubTasks.Add(subtask);
                await _dbContext.SaveChangesAsync();

                var taskdate = new TaskDateWorkedHours
                {
                    DateTime = DateTime.UtcNow,
                    WorkedHours = dto.WorkedHours,
                    TaskId = dto.TaskItemId
                };

                _dbContext.TaskDateworkedHours.Add(taskdate);
                await _dbContext.SaveChangesAsync();

                var task = await _dbContext.Tasks.FirstOrDefaultAsync(p => p.Id == dto.TaskItemId);
                if (task != null)
                {
                    task.CompletedHours += dto.WorkedHours;
                    task.HasSubtask = "Yes"; // Update HasSubtask flag
                    _dbContext.Update(task);
                    await _dbContext.SaveChangesAsync();

                    // Update parent task status after subtask creation
                    await UpdateParentTaskStatusAsync(dto.TaskItemId);
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Parent task {TaskId} not found during subtask creation.", dto.TaskItemId);
                    return new { Message = "Parent task not found." };
                }

                await transaction.CommitAsync();
                return new { Message = "Subtask created successfully." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating subtask for task {TaskId} by user {UserId}", dto.TaskItemId, userId);
                return new { Message = "Failed to create subtask due to an error." };
            }
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
                    EstimatedHours = st.EstimatedHours,
                    TaskItemId = st.TaskItemId,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<string> DeleteSubTaskAsync(int id, string userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var subTask = await _dbContext.SubTasks
                    .Include(st => st.TaskItem)
                    .FirstOrDefaultAsync(st => st.Id == id && st.TaskItem.CreatedBy == userId);

                if (subTask == null)
                    return "SubTask not found or you don't have permission to delete it.";

                if (subTask.Status == "In Progress")
                    return "Cannot delete a subtask that is In Progress.";


                _dbContext.SubTasks.Remove(subTask);
                await _dbContext.SaveChangesAsync();

                // Update parent task status after deletion
                await UpdateParentTaskStatusAsync(subTask.TaskItemId);

                await transaction.CommitAsync();
                return "SubTask deleted successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting subtask {SubTaskId} for user {UserId}", id, userId);
                return "Failed to delete subtask due to a database error.";
            }
        }

        public async Task<(SubTask, string)> UpdateSubTaskAsync(int id, SubTaskUpdateDto dto, string userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var task = await _dbContext.Tasks.FirstOrDefaultAsync(p => p.Id == dto.TaskItemId);
                if (task == null)
                    return (null, "Parent task not found.");

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
                task.CompletedHours += dto.WorkeddHours;
                _dbContext.Update(task);
                await _dbContext.SaveChangesAsync();

                // Update parent task status
                await UpdateParentTaskStatusAsync(dto.TaskItemId);

                await transaction.CommitAsync();
                return (subtask, "SubTask updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating subtask {SubTaskId} for user {UserId}", id, userId);
                return (null, "Failed to update subtask due to an error.");
            }
        }

        public async Task<object> Updatesubtask(int id, SubtaskupdatedDto dto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var subtask = await _dbContext.SubTasks
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (subtask == null)
                    return new { Message = "SubTask not found" };

                subtask.Status = dto.Status;
                subtask.UpdatedAt = DateTime.UtcNow;
                subtask.Priority = dto.Priority;
                subtask.CompletedHours += dto.WorkedHours;
                _dbContext.Update(subtask);
                await _dbContext.SaveChangesAsync();

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
                    task.CompletedHours += dto.WorkedHours;
                    _dbContext.Update(task);
                    await _dbContext.SaveChangesAsync();

                    // Update parent task status
                    await UpdateParentTaskStatusAsync(dto.TaskItemId);
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Parent task {TaskId} not found during subtask update.", dto.TaskItemId);
                    return new { Message = "Parent task not found." };
                }

                await transaction.CommitAsync();
                return new { Message = "SubTask updated successfully" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating subtask {SubTaskId}", id);
                return new { Message = "Failed to update subtask due to an error." };
            }
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
                    EstimatedHours = st.EstimatedHours,
                    TaskItemId = st.TaskItemId,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt
                })
                .ToListAsync();

            return (subTasks, null);
        }
    }
}