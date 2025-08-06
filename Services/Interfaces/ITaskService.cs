using Demoproject.Dtos;
using Demoproject.Models;

namespace Demoproject.Services.Interfaces
{
    public interface ITaskService
    {
        Task<(TaskItem, string)> CreateTaskAsync(TaskItemCreateDto dto, string userId);
        //Task<List<TaskItemDto>> GetAllTasksAsync(string userId);
        Task<(TaskItemDetailDto, string)> GetTaskAsync(int id, string userId);
        Task<string> DeleteTaskAsync(int id, string userId);
        Task<(TaskItem, string)> UpdateTaskAsync(TaskItemUpdateDto dto, string userId);
        Task<bool> CheckSubtaskExistsAsync(int taskId);
        Task<List<TaskItemDto>> GetMyTasksAsync(string userId);
        Task<List<TaskDependencyDto>> GetTaskDependenciesUserAsync(int id, string userId);
        Task<PaginatedResult<TaskItemDto>> GetAllTasksGlobalAsync(int pageNumber = 1, int pageSize = 10);
        Task<List<TaskItemDto>> GetAllTasksforPerformanceGlobalAsync();
        Task<(TaskItem, string)> UpdateTaskWorkedHoursAsync(TaskWorkedHoursUpdateDto dto, string userId);
        //Task<PaginatedResult<TaskItemDto>> GetAllTasksAsync(string userId, int pageNumber = 1, int pageSize = 5);

        Task<PaginatedResult<TaskItemDto>> GetAllTasksAsync(
            string userId,
            int pageNumber = 1,
            int pageSize = 5,
            string? priority = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? dueDate = null,
            string? searchTerm = null);
        Task<List<TaskItemDto>> GetMyInProgressTasksAsync(string userId);
        Task UpdateLinkTask(int taskId, int dependencyId);



    }

    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}