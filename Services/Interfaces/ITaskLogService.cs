using System.Collections.Generic;
using System.Threading.Tasks;
using Demoproject.Dtos;

namespace Demoproject.Services.Interfaces
{
    public interface ITaskLogService
    {
        Task<(TaskLogDto, string)> CreateTaskLogAsync(TaskLogCreateDto taskLogDto, string createdBy);
        Task<List<TaskLogDto>> GetTaskLogsByUserAsync(string userId, int? taskId, int? subTaskId);
        Task<List<TaskLogDto>> GetTaskLogsByTaskIdAsync(int taskId);
    }
}