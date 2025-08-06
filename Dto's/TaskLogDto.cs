using System;

namespace Demoproject.Dtos
{
    public class TaskLogDto
    {
        public int Id { get; set; }
        public int? TaskId { get; set; }
        public int? SubTaskId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal HoursWorked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TaskLogCreateDto
    {
        public int? TaskId { get; set; }
        public int? SubTaskId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal HoursWorked { get; set; }
    }
}