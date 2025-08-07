using Demoproject.Dto_s;
using Demoproject.Dtos;
using Demoproject.Models;
using Microsoft.Graph;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demoproject.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(GraphServiceClient graphClient, ClaimsPrincipal user);
        UserProfileDto GetUserProfileFromClaims(ClaimsPrincipal user);
        Task SaveUserToDatabaseAsync(Microsoft.Graph.Models.User graphUser, List<string> roles);
        Task SaveBasicUserToDatabaseAsync(string userId, ClaimsPrincipal user);
        Task<(UserDto, string)> GetUserDetailsAsync(string userId);
        Task<List<UserDto>> GetDetailsListAsync();

        Task<IEnumerable<object>> GetRequestAsync(string id);
        Task<IEnumerable<object>> GetRequestIncomeAsync(string id);

        Task<(bool, string)> DepartmentDetailsAsync(string userId, DepartmentDto department);




        Task RequestDetailsAsync(RequestDto request);

        Task ChangeStatus(int id);
        Task AcceptedStatus(DependencyTaskDto request );

        Task<List<UserDepartmentDto>> GetAllUserDetailsAsync();
        Task<(UserDto, string)> GetUserDetailsAsyncNotification(string userId);
        Task<List<User>> GetManagerDetailsAsync();
        (string Key, string IV) GetEncryptionKeys();
    }
}