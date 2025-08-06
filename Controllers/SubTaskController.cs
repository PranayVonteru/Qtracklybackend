using Demoproject.Dtos;
using Demoproject.Services;
using Demoproject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demoproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class SubTaskController : ControllerBase
    {
        private readonly ISubTaskService _subTaskService;
        private readonly ILogger<SubTaskController> _logger;

        public SubTaskController(ISubTaskService subTaskService, ILogger<SubTaskController> logger)
        {
            _subTaskService = subTaskService;
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

        [HttpPost]
        public async Task<IActionResult> CreateSubTask([FromBody] SubTaskCreateDto subTaskDto)
        {
            

           
                var userId = GetCurrentUserId();
                var varee = await _subTaskService.CreateSubTaskAsync(subTaskDto, userId);

            if (varee == null)
                return BadRequest("data Not submitted");
            else return Ok(varee);

             
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (subTask, message) = await _subTaskService.GetSubTaskAsync(id, userId);

                if (subTask == null)
                    return NotFound(new { message });

                return Ok(subTask);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve subtask.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var subTasks = await _subTaskService.GetAllSubTasksAsync(userId);
                return Ok(subTasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve subtasks.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = await _subTaskService.DeleteSubTaskAsync(id, userId);

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
                return StatusCode(500, new { message = "Failed to delete subtask.", error = ex.Message });
            }
        }

        [HttpPut("Hello/{id}")]
        public async Task<IActionResult> UpdateSubTaskss(int id, [FromBody] SubTaskUpdateDto subTaskDto)
        {
            if (subTaskDto == null)
                return BadRequest(new { message = "No update data provided." });

            try
            {
                var userId = GetCurrentUserId();
                var (subTask, message) = await _subTaskService.UpdateSubTaskAsync(id, subTaskDto, userId);

                if (subTask == null)
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
                return StatusCode(500, new { message = "Failed to update subtask.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public Task<Object> UpdateSubTask(int id, [FromBody] SubtaskupdatedDto dto)
        {

            Console.WriteLine("pppppppppppppppppppppppppsuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu"+dto.WorkedHours);
            var obj=_subTaskService.Updatesubtask(id, dto);
            return obj;

        }



        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetSubTasksByTaskId(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (subTasks, message) = await _subTaskService.GetSubTasksByTaskIdAsync(taskId, userId);

                if (subTasks == null)
                    return NotFound(new { message });

                return Ok(subTasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve subtasks.", error = ex.Message });
            }
        }
    }
}