using Demoproject.Dtos;
using Demoproject.Services;
using Demoproject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demoproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize(Policy = "UserAccess")]
    public class TaskLogController : ControllerBase
    {
        private readonly ITaskLogService _taskLogService;
        private readonly ILogger<TaskLogController> _logger;

        public TaskLogController(ITaskLogService taskLogService, ILogger<TaskLogController> logger)
        {
            _taskLogService = taskLogService;
            _logger = logger;
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskLogDto>>> GetTaskLogsByTaskId(int taskId)
        {
            try
            {
                var taskLogs = await _taskLogService.GetTaskLogsByTaskIdAsync(taskId);
                return Ok(taskLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task logs for task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while retrieving task logs." });
            }
        }

        

        // Removed POST endpoint as per requirement
    }
}