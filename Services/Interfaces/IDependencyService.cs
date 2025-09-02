using Demoproject.Dtos;


namespace Demoproject.Services.Interfaces
{
    public interface IDependencyService
    {
        Task<(Dependency, string)> CreateDependencyAsync(DependencyCreateDto dto, string userId);
        Task<string> DeleteDependencyAsync(int id, string userId);
        Task<List<DependencyDto>> GetAllDependenciesAsync(string userId);
        Task<(List<DependencyDto>, string)> GetDependenciesByTaskIdAsync(int taskId, string userId);
        Task<List<DependencyDto>> GetDependenciesAssignedToMeAsync(string userId);
        Task<(Dependency, string)> UpdateDependencyAsync(int id, DependencyUpdateDto dto, string userId);
        Task<Object> UserviewTaskDependencies(int id);
        Task<Object> UserviewTaskDependent(int id);
        Task<Object> UserviewTaskDependenciesAccepted(int id);
        Task<Object> RejectDependency(int DependencyTaskId );
        Task<int> GetDependencyRequestCount(string userid);

    }
}