using Demoproject.Dtos;
using Demoproject.Services;
using Demoproject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Demoproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ITaskService taskService, ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirst("oid")?.Value
                       ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No user ID found in claims. Available claims: {Claims}",
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}:{c.Value}")));
                throw new UnauthorizedAccessException("User ID not found in token claims");
            }

            return userId;
        }



        [HttpPost("updateCompleteMainTask")]
        public async Task<IActionResult> UpdateCompleteMainTask([FromQuery] int taskId)
        {
            var result = await _taskService.updateCompleteMainTask(taskId);

            if (result != null)
            {
                // Return 404 Not Found with message 
                // hii
                return NotFound(new { status = 404, message = "FAIL TO UPDATE MAIN TASK AS COMPLETED" });
            }
            return Ok(result);
        }

    




        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskItemCreateDto taskDto)
        {
            if (taskDto == null)
                return BadRequest(new { message = "Task data is required." });

            try
            {
                var userId = GetCurrentUserId();
                var (task, message) = await _taskService.CreateTaskAsync(taskDto, userId);

                if (task == null)
                    return BadRequest(new { message });

                var taskDtoResponse = new TaskItemDto
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
                    WorkedHours = task.CompletedHours,
                    PrevCompletedHours = task.PrevCompletedHours,
                    CurrentDayEfforts = task.CurrentDayEfforts,
                    CreatedBy = task.CreatedBy,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    SubTasksCount = task.SubTasks.Count,
                    DependenciesCount = task.Dependencies.Count,
                    HasSubtask = task.HasSubtask
                };

                return Ok(new { task = taskDtoResponse, message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create task.", error = ex.Message });
            }
        }

        [HttpGet("GetAllTasks")]
        public async Task<IActionResult> GetAllTasks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string? priority = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? dueDate = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var paginatedTasks = await _taskService.GetAllTasksAsync(userId, pageNumber, pageSize, priority, status, startDate, dueDate, searchTerm);
                var response = new
                {
                    data = paginatedTasks.Data,
                    pagination = new
                    {
                        totalCount = paginatedTasks.TotalCount,
                        pageNumber = paginatedTasks.PageNumber,
                        pageSize = paginatedTasks.PageSize,
                        totalPages = paginatedTasks.TotalPages,
                        hasPreviousPage = paginatedTasks.HasPreviousPage,
                        hasNextPage = paginatedTasks.HasNextPage
                    }
                };
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve paginated tasks for user {UserId}. PageNumber: {PageNumber}, PageSize: {PageSize}, Filters: {Priority}, {Status}, {StartDate}, {DueDate}, {SearchTerm}",
                    GetCurrentUserId(), pageNumber, pageSize, priority, status, startDate, dueDate, searchTerm);
                return StatusCode(500, new { message = "Failed to retrieve tasks.", error = ex.Message });
            }
        }
        //[HttpGet("GetAllTasks")]
        //public async Task<IActionResult> GetAllTasks(
        //[FromQuery] int pageNumber = 1,
        //[FromQuery] int pageSize = 5,
        //[FromQuery] string? priority = null,
        //[FromQuery] string? status = null,
        //[FromQuery] DateTime? startDate = null,
        //[FromQuery] DateTime? dueDate = null)
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        var paginatedTasks = await _taskService.GetAllTasksAsync(userId, pageNumber, pageSize, priority, status, startDate, dueDate);
        //        var response = new
        //        {
        //            data = paginatedTasks.Data,
        //            pagination = new
        //            {
        //                totalCount = paginatedTasks.TotalCount,
        //                pageNumber = paginatedTasks.PageNumber,
        //                pageSize = paginatedTasks.PageSize,
        //                totalPages = paginatedTasks.TotalPages,
        //                hasPreviousPage = paginatedTasks.HasPreviousPage,
        //                hasNextPage = paginatedTasks.HasNextPage
        //            }
        //        };
        //        return Ok(response);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to retrieve paginated tasks for user {UserId}. PageNumber: {PageNumber}, PageSize: {PageSize}, Filters: {Priority}, {Status}, {StartDate}, {DueDate}",
        //            GetCurrentUserId(), pageNumber, pageSize, priority, status, startDate, dueDate);
        //        return StatusCode(500, new { message = "Failed to retrieve tasks.", error = ex.Message });
        //    }
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (task, message) = await _taskService.GetTaskAsync(id, userId);

                if (task == null)
                    return NotFound(new { message });

                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve task.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = await _taskService.DeleteTaskAsync(id, userId);

                if (message.Contains("not found") || message.Contains("permission"))
                    return NotFound(new { message });

                if (message.Contains("In Progress"))
                    return BadRequest(new { message });

                return Ok(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete task.", error = ex.Message });
            }
        }

        [HttpPatch("UpdateTask")]
        public async Task<IActionResult> UpdateTask([FromBody] TaskItemUpdateDto taskDto)
        {
            if (taskDto == null)
                return BadRequest(new { message = "No update data provided." });

            try
            {
                var userId = GetCurrentUserId();
                var (task, message) = await _taskService.UpdateTaskAsync(taskDto, userId);

                if (task == null)
                    return message.Contains("not found") || message.Contains("permission")
                        ? NotFound(new { message })
                        : BadRequest(new { message });

                return Ok(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update task.", error = ex.Message });
            }
        }

        [HttpGet("SubtaskExists{taskid}")]
        public async Task<IActionResult> CheckSubtaskExists(int taskid)
        {
            var exists = await _taskService.CheckSubtaskExistsAsync(taskid);
            return Ok(exists);
        }

        [HttpGet("{taskid}/{userId}")]
        public async Task<List<TaskDependencyDto>> GetTasksUsersidAsync(int taskid, string userId)
        {
            return await _taskService.GetTaskDependenciesUserAsync(taskid, userId);
        }

        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var tasks = await _taskService.GetMyTasksAsync(userId);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve tasks.", error = ex.Message });
            }
        }

        [HttpGet("alltaskforPerformance")]
        public async Task<IActionResult> GetAllTasksGlobal()
        {
            try
            {
                var tasks = await _taskService.GetAllTasksforPerformanceGlobalAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all tasks");
                return StatusCode(500, new { message = "Failed to retrieve all tasks.", error = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTasksGlobal([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var paginatedTasks = await _taskService.GetAllTasksGlobalAsync(pageNumber, pageSize);
                var response = new
                {
                    data = paginatedTasks.Data,
                    pagination = new
                    {
                        totalCount = paginatedTasks.TotalCount,
                        pageNumber = paginatedTasks.PageNumber,
                        pageSize = paginatedTasks.PageSize,
                        totalPages = paginatedTasks.TotalPages,
                        hasPreviousPage = paginatedTasks.HasPreviousPage,
                        hasNextPage = paginatedTasks.HasNextPage
                    }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve paginated global tasks. PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
                return StatusCode(500, new { message = "Failed to retrieve all tasks.", error = ex.Message });
            }
        }

        [HttpPost("worked-hours")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTaskWorkedHours([FromBody] TaskWorkedHoursUpdateDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { message = "Invalid task data provided." });

            try
            {
                var userId = GetCurrentUserId();
                var (task, message) = await _taskService.UpdateTaskWorkedHoursAsync(dto, userId);

                if (task == null)
                    return message.Contains("not found") || message.Contains("permission")
                        ? NotFound(new { message })
                        : BadRequest(new { message });

                return Ok(new { taskId = task.Id, message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update task worked hours for task {TaskId}", dto.TaskId);
                return StatusCode(500, new { message = "Failed to update task worked hours.", error = ex.Message });
            }
        }


        [HttpGet("inprogress-tasks")]
        public async Task<IActionResult> GetMyInProgressTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var tasks = await _taskService.GetMyInProgressTasksAsync(userId);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve tasks.", error = ex.Message });
            }
        }

        [HttpPatch("link/{taskId}/{dependencytaskId}")]
        public async Task<IActionResult> UpdateLink(int taskId, int dependencytaskId)
        {
            try
            {
                await _taskService.UpdateLinkTask(taskId, dependencytaskId);
                return Ok("Dependency link updated successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                // If your service throws this when dependencies aren't found
                return NotFound($"No matching dependencies: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                // For invalid input cases
                return BadRequest($"Invalid arguments: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Fallback for unexpected errors
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
    }
}