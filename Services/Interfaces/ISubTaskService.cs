using Demoproject.Dtos;
using Demoproject.Models;

namespace Demoproject.Services.Interfaces
{
    public interface ISubTaskService
    {
        Task<object> CreateSubTaskAsync(SubTaskCreateDto dto, string userId);
        Task<(SubTaskDto, string)> GetSubTaskAsync(int id, string userId);
        Task<List<SubTaskDto>> GetAllSubTasksAsync(string userId);
        Task<string> DeleteSubTaskAsync(int id, string userId);
        Task<(SubTask, string)> UpdateSubTaskAsync(int id, SubTaskUpdateDto dto, string userId);
        Task<(List<SubTaskDto>, string)> GetSubTasksByTaskIdAsync(int taskId, string userId);

        Task<Object> Updatesubtask(int id, SubtaskupdatedDto dto);

    }
}
