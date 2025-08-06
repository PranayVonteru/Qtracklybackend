using Demoproject.Dtos;

namespace Demoproject.Services.Interfaces
{
    public interface IFocusBoardService
    {
        Task<Object> GetTodaysFocusAsync(string userId);
        Task<Object> GetOverdueTasksAsync(string userId);
        Task<List<TaskSummaryDto>> GetWaitingTasksAsync(string userId);
        Task<List<TaskSummaryDto>> GetDependentTasksAsync(string userId);
        Task<TaskStatsDto> GetTaskStatsAsync(string userId);
        //Task<List<TaskDependencyDto>> GetDependentTasksByUserAsync(string userId);
        public Task<Object> GetDependentTasksByUserAsync(string userId);
        public Task<Object> GetDependencyTasksByUserAsync(string userId);
       

    }
}