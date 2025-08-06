using System;

namespace Demoproject.Models
{
    public class TaskLog
    {
        public int Id { get; set; }
        public int? TaskId { get; set; }
        public TaskItem? Task { get; set; }
        public int? SubTaskId { get; set; }
        public SubTask? SubTask { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Team { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal HoursWorked { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}