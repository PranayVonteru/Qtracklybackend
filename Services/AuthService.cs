using Demoproject.Dtos;
using Demoproject.Services.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demoproject.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;

        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;
        }

        public async Task<AuthUserDto> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            _logger.LogInformation("GetCurrentUserAsync called");

            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
            _logger.LogInformation("All claims: {Claims}", claims);

            var userId = user.FindFirst("oid")?.Value
                       ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                       ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No user ID found in claims.");
                return null;
            }

            var userName = user.FindFirst("name")?.Value
                        ?? user.FindFirst(ClaimTypes.Name)?.Value
                        ?? user.FindFirst("given_name")?.Value
                        ?? user.FindFirst("preferred_username")?.Value;

            var userEmail = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value
                         ?? user.FindFirst("preferred_username")?.Value
                         ?? user.FindFirst("email")?.Value
                         ?? user.FindFirst(ClaimTypes.Email)?.Value;

            var roles = user.FindAll("roles")
                .Concat(user.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                .Select(c => c.Value.ToLower())
                .Distinct()
                .ToList();

            _logger.LogInformation("User ID: {UserId}, Name: {UserName}, Email: {UserEmail}, Roles: {Roles}",
                userId, userName, userEmail, roles);

            return await Task.FromResult(new AuthUserDto
            {
                Id = userId,
                Name = userName ?? "Unknown",
                Email = userEmail ?? "Unknown",
                Roles = roles,
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                AuthenticationType = user.Identity?.AuthenticationType
            });
        }
    }
}