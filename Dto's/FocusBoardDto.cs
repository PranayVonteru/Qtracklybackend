using System.Collections.Generic;

namespace Demoproject.Dtos
{
    public class TaskSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
    }

    public class TaskStatsDto
    {
        public int ToDo { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Waiting { get; set; }
        public int Overdue { get; set; }
    }
}