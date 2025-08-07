



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
    [Authorize]
    public class FocusBoardController : ControllerBase
    {
        private readonly IFocusBoardService _focusBoardService;
        private readonly ILogger<FocusBoardController> _logger;

        public FocusBoardController(IFocusBoardService focusBoardService, ILogger<FocusBoardController> logger)
        {
            _focusBoardService = focusBoardService;
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

            _logger.LogInformation("Extracted User ID: {UserId}", userId);
            return userId;
        }

        [HttpGet("TodaysFocus")]
        public async Task<IActionResult> GetTodaysFocus()
        {
            try
            {
                var userId = GetCurrentUserId();
                var tasks = await _focusBoardService.GetTodaysFocusAsync(userId);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch today's focus tasks");
                return StatusCode(500, new { message = "Failed to fetch today's focus tasks.", error = ex.Message });
            }
        }

        [HttpGet("Overdue")]
        public async Task<IActionResult> GetOverdueTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var tasks = await _focusBoardService.GetOverdueTasksAsync(userId);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch overdue tasks");
                return StatusCode(500, new { message = "Failed to fetch overdue tasks.", error = ex.Message });
            }
        }

        [HttpGet("Dependent")]
        public async Task<IActionResult> GetDependentTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var tasks = await _focusBoardService.GetDependentTasksAsync(userId);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch dependent tasks");
                return StatusCode(500, new { message = "Failed to fetch dependent tasks.", error = ex.Message });
            }
        }

        [HttpGet("TaskStats")]
        public async Task<IActionResult> GetTaskStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var stats = await _focusBoardService.GetTaskStatsAsync(userId);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch task statistics");
                return StatusCode(500, new { message = "Failed to fetch task statistics.", error = ex.Message });
            }
        }

        [HttpGet("DependentTasks/{userId}")]
        public async Task<IActionResult> GetDependentTasksAsync(string userId)
        {
            try
            {
                var tasks = await _focusBoardService.GetDependentTasksByUserAsync(userId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch dependent tasks for user {UserId}", userId);
                return StatusCode(500, new { message = "Failed to fetch dependent tasks.", error = ex.Message });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetDependencyTasksAsync(string userId)
        {
            try
            {
                var tasks = await _focusBoardService.GetDependencyTasksByUserAsync(userId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch dependency tasks for user {UserId}", userId);
                return StatusCode(500, new { message = "Failed to fetch dependency tasks.", error = ex.Message });
            }
        }
    }
}