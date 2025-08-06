using Demoproject.Dtos;
using Demoproject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;
using Demoproject.Services.Interfaces;
using Demoproject.Dto_s;

namespace QuadrantTechnologies.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IConfiguration configuration,
            ITokenAcquisition tokenAcquisition,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _tokenAcquisition = tokenAcquisition;
            _logger = logger;
        }

        [HttpGet("profile")]
        //[Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized("No valid authorization header found.");
                }

                var incomingToken = authHeader.Substring("Bearer ".Length).Trim();
                var scopes = new[] { "https://graph.microsoft.com/User.Read" };

                try
                {
                    string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes, user: User);
                    var graphClient = new GraphServiceClient(new AccessTokenCredential(accessToken));
                    var profile = await _userService.GetUserProfileAsync(graphClient, User);
                    return Ok(profile);
                }
                catch (MicrosoftIdentityWebChallengeUserException ex)
                {
                    _logger.LogWarning("OBO flow failed, falling back to basic user info from token claims: {Error}", ex.Message);
                    var profile = _userService.GetUserProfileFromClaims(User);
                    await _userService.SaveBasicUserToDatabaseAsync(profile.Id, User);
                    return Ok(profile);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, "An error occurred while retrieving user profile.");
            }
        }

        [HttpGet("profile-simple")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> GetUserProfileSimple()
        {
            try
            {
                var profile = _userService.GetUserProfileFromClaims(User);
                await _userService.SaveBasicUserToDatabaseAsync(profile.Id, User);
                return Ok(profile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile from claims");
                return StatusCode(500, "An error occurred while retrieving user profile.");
            }
        }

        [HttpGet("users")]
        // [Authorize(Policy = "UserAccess")]

        //[HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var userDetails = await _userService.GetDetailsListAsync();
            return Ok(userDetails);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestsMyRequest(string id)
        {
            var listRequests = await _userService.GetRequestAsync(id);
            Console.WriteLine(listRequests+"");
            if (listRequests == null || !listRequests.Any())
            {
                return NotFound();
            }
            return Ok(listRequests);
        }


        [HttpGet("Income/{id}")]
        public async Task<IActionResult> GetRequestsIncomingRequest(string id)
        {
            var listRequests = await _userService.GetRequestIncomeAsync(id);
            if (listRequests == null || !listRequests.Any())
            {
                return NotFound();
            }
            return Ok(listRequests);
        }



        [HttpPost]
        public async Task<IActionResult> RequestDetails([FromBody] RequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request payload");
            }

            _userService.RequestDetailsAsync(request);

            return Ok("Request processed successfully");
        }


        [HttpGet("user1/{id}")]

        [Authorize(Policy = "UserAccess")]

        public async Task<IActionResult> GetUserById(string id)

        {

            try

            {

                _logger.LogInformation("Received request for user ID: {Id}", id);

                var (user, message) = await _userService.GetUserDetailsAsyncNotification(id);

                if (user == null)

                {

                    _logger.LogWarning("User not found for ID: {Id}", id);

                    return NotFound(new { message });

                }

                return Ok(new { id = user.UserId, name = user.Name });

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error fetching user with ID: {Id}", id);

                return StatusCode(500, "An error occurred while fetching user details.");

            }

        }



        [HttpGet("details")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> GetUserDetails()
        {
            try
            {
                var userId = User.FindFirst("oid")?.Value
                    ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No user ID found in claims.");
                    throw new UnauthorizedAccessException("User ID not found in token claims");
                }

                var (user, message) = await _userService.GetUserDetailsAsync(userId);

                if (user == null)
                    return NotFound(new { message });

                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details");
                return StatusCode(500, "An error occurred while retrieving user details.");
            }
        }

        [HttpGet("all")]
        //  [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var allUsers = await _userService.GetAllUserDetailsAsync();
                return Ok(allUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all user details");
                return StatusCode(500, "An error occurred while retrieving user details.");
            }
        }

        [HttpPut("{taskId}")]

        public async Task ChangeTaskStatus(int taskId)
        {
            _userService.ChangeStatus(taskId);
        }

        [HttpPost("DependencyTask")]

        public async Task ChangeAcceptStatus([FromBody] DependencyTaskDto request)
        {
            _userService.AcceptedStatus(request);
        }

        private class AccessTokenCredential : Azure.Core.TokenCredential
        {
            private readonly string _accessToken;

            public AccessTokenCredential(string accessToken)
            {
                _accessToken = accessToken;
            }

            public override Azure.Core.AccessToken GetToken(Azure.Core.TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return new Azure.Core.AccessToken(_accessToken, DateTimeOffset.Now.AddHours(1));
            }

            public override async ValueTask<Azure.Core.AccessToken> GetTokenAsync(Azure.Core.TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return await Task.FromResult(GetToken(requestContext, cancellationToken));
            }
        }

        [HttpPost("{userId}/department")]
        public async Task<IActionResult> DepartmentStatus(string userId, [FromBody] DepartmentDto department)
        {
            try
            {
                var (success, message) = await _userService.DepartmentDetailsAsync(userId, department);
                if (!success)
                {
                    _logger.LogWarning("Failed to update department details for userId: {UserId}, Message: {Message}", userId, message);
                    return BadRequest(new { message });
                }
                return Ok(new { message = "Department details updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department details for userId: {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while updating department details." });
            }
        }

        [HttpGet("managers")]
        public async Task<IActionResult> GetAllManagerDetails()
        {
            try
            {
                _logger.LogInformation("Fetching all manager details");
                var managers = await _userService.GetManagerDetailsAsync();
                return Ok(managers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all manager details");
                return StatusCode(500, new { message = "An error occurred while fetching manager details." });
            }
        }
    }
}