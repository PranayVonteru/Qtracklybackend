using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Demoproject.Dtos
{
    public class TaskItemDto
    {
        public int Id { get; set; }
        [Required]
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
        public decimal WorkedHours { get; set; }
        public decimal PrevCompletedHours { get; set; }
        public decimal CurrentDayEfforts { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int SubTasksCount { get; set; }
        public int DependenciesCount { get; set; }
        public string HasSubtask { get; set; } = "No"; // Added HasSubtask
    }

    public class TaskItemCreateDto
    {
        [Required]
        public string TaskName { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "Not Started";
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        [Required]
        public decimal EstimatedHours { get; set; }
        public decimal CompletedHours { get; set; } = 0;
        public decimal WorkedHours { get; set; }
        public string HasSubtask { get; set; } = "No"; // Already includes HasSubtask
    }

    public class TaskItemUpdateDto
    {
        public int TaskId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UpdateDate { get; set; }
        public decimal WorkedHours { get; set; }
        public string Priority { set; get; }
        public string HasSubtask { get; set; } = "No"; // Already includes HasSubtask
    }

    public class TaskItemDetailDto
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal CompletedHours { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IEnumerable<SubTaskDto> SubTasks { get; set; } = new List<SubTaskDto>();
        public IEnumerable<DependencyDto> Dependencies { get; set; } = new List<DependencyDto>();
        public string HasSubtask { get; set; } = "No"; // Added HasSubtask
    }

    public class TaskDependencyDto
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int DependencyTaskId { get; set; }
        public string DependencyTaskName { get; set; }
        public string DependencyTaskStatus { get; set; }
        public decimal? DependencyTaskEstimatedHours { get; set; }
        public DateTime? DependencyTaskDueDate { get; set; }
        public string DependencyTaskPriority { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; }
    }

    public class TaskWorkedHoursUpdateDto
    {
        [Required]
        public int TaskId { get; set; }
        [Required]
        public DateTime UpdateDate { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "WorkedHours must be non-negative.")]
        public decimal WorkedHours { get; set; }
    }
}