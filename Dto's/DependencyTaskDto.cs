using System.ComponentModel.DataAnnotations;

namespace Demoproject.Dto_s
{
    public class DependencyTaskDto
    {
        public string TaskName { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "Not Started";
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        [Required]
        public decimal EstimatedHours { get; set; }
        public decimal CompletedHours { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int SubTasksCount { get; set; }
        public int DependenciesCount { get; set; }

        public int DependencyTaskId { get; set; }
    }
}
