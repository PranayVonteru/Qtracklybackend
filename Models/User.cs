using System;
using System.Collections.Generic;

namespace Demoproject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty; // JSON serialized

        public string? Department { get; set; }
        public string? SubDepartment { get; set; }

        public string? Manager { get; set; }

        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
    }
}