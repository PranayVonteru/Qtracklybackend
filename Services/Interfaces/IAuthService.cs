using Demoproject.Dtos;

namespace Demoproject.Services.Interfaces { 
    public interface IAuthService
    {
        Task<AuthUserDto> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal user);
    }
}