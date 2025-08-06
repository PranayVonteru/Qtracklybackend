using System;
using System.Collections.Generic;

namespace Demoproject.Models
{
    public class SubTask
    {
        public int Id { get; set; }
        public string SubTaskName { get; set; } = string.Empty;
        public string Status { get; set; } = "Not Started";
        public string Description { get; set; } = string.Empty;
        public String Priority {  get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal CompletedHours { get; set; }
        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
    }
}