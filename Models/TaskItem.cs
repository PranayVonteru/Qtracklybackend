using System;
using System.Collections.Generic;

namespace Demoproject.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "Not Started";
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal CompletedHours { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DependencyTaskId { get; set; }
        public List<SubTask> SubTasks { get; set; } = new List<SubTask>();
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
        public List<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
        public decimal PrevCompletedHours { get; set; }
        public decimal CurrentDayEfforts { get; set; }
        public string HasSubtask { get; set; } = "No"; // Added HasSubtask
        //public string AssignedTo { get; internal set; }
    }
}