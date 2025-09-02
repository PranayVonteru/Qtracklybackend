using Demoproject.Dto_s;
using Demoproject.Dtos;
using Demoproject.Services;
using Demoproject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demoproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DependencyController : ControllerBase
    {
        private readonly IDependencyService _dependencyService;
        private readonly ILogger<DependencyController> _logger;

        public DependencyController(IDependencyService dependencyService, ILogger<DependencyController> logger)
        {
            _dependencyService = dependencyService;
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
        public async Task<IActionResult> CreateDependency([FromBody] DependencyCreateDto dependencyDto)
        {
            if (dependencyDto == null)
                return BadRequest(new { message = "Dependency data is required." });

            try
            {
                var userId = GetCurrentUserId();
                var (dependency, message) = await _dependencyService.CreateDependencyAsync(dependencyDto, userId);

                if (dependency == null)
                    return NotFound(new { message });

                return Ok(new { dependencyId = dependency.Id, message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create dependency.", error = ex.Message });
            }
        }
        [HttpGet("DependencyRequestsCount")]
        public async Task<int> GetDependencyRequestCount()
        {
            // Assuming you have access to the current user's ID (e.g., from claims or context)
            string userId = GetCurrentUserId(); // Replace with actual logic to get user ID

            var count = await _dependencyService.GetDependencyRequestCount(userId);
            return count;
        }






        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDependency(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = await _dependencyService.DeleteDependencyAsync(id, userId);

                if (message.Contains("not found") || message.Contains("permission"))
                    return NotFound(new { message });

                return Ok(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete dependency.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDependencies()
        {
            try
            {
                var userId = GetCurrentUserId();
                var dependencies = await _dependencyService.GetAllDependenciesAsync(userId);
                return Ok(dependencies);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve dependencies.", error = ex.Message });
            }
        }

        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetDependenciesByTaskId(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (dependencies, message) = await _dependencyService.GetDependenciesByTaskIdAsync(taskId, userId);

                if (dependencies == null)
                    return NotFound(new { message });

                return Ok(dependencies);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve dependencies for task.", error = ex.Message });
            }
        }

        [HttpGet("assigned-to-me")]
        public async Task<IActionResult> GetDependenciesAssignedToMe()
        {
            try
            {
                var userId = GetCurrentUserId();
                var dependencies = await _dependencyService.GetDependenciesAssignedToMeAsync(userId);
                return Ok(dependencies);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve assigned dependencies.", error = ex.Message });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateDependency(int id, [FromBody] DependencyUpdateDto dependencyDto)
        {
            if (dependencyDto == null)
                return BadRequest(new { message = "No update data provided." });

            try
            {
                var userId = GetCurrentUserId();
                var (dependency, message) = await _dependencyService.UpdateDependencyAsync(id, dependencyDto, userId);

                if (dependency == null)
                    return NotFound(new { message });

                return Ok(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update dependency.", error = ex.Message });
            }
        }

        [HttpGet("GetDependencyRequests/{taskId}")]
        public async Task<Object> UserviewTaskDependencies(int taskId)
        {
            var obj = await _dependencyService.UserviewTaskDependencies(taskId);
            return obj;
        }

        [HttpGet("GetDependentTasks/{taskId}")]
        public async Task<Object> UserviewTaskDependent(int taskId)
        {
            var obj = await _dependencyService.UserviewTaskDependent(taskId);
            return obj;
        }

        [HttpGet("GetDependentAcceptedTasks/{DependencytaskId}")] 
        public async Task<Object> UserviewTaskDependenciesAccepted(int DependencytaskId)
        {
            var obj = await _dependencyService.UserviewTaskDependenciesAccepted(DependencytaskId);
            return obj;
        }

        [HttpPost("RejectDependency/{DependencytaskId}")]
        public async Task<Object> RejectDependency(int DependencytaskId)
        {
            var obj = await _dependencyService.RejectDependency(DependencytaskId);
            return obj;
        }
    }
}