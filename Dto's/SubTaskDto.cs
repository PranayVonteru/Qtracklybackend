using System;
using System.ComponentModel.DataAnnotations;

namespace Demoproject.Dtos
{
    public class SubTaskDto
    {
        public int Id { get; set; }
        public string SubTaskName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal CompletedHours { get; set; }
        public int TaskItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SubTaskCreateDto
    {
        [Required]
        public string SubTaskName { get; set; } = string.Empty;
        public string Status { get; set; } = "Not Started";
        public string Priority { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
       
        public DateTime DueDate { get; set; }

        public decimal EstimatedHours { get; set; }
        public decimal WorkedHours { get; set; }
   
        public int TaskItemId { get; set; }
    }

    public class SubTaskUpdateDto
    {
        public string SubTaskName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime Datetime { get; set; }
        public decimal WorkeddHours { get; set; }
        public int TaskItemId { get; set; }
    }


    public class SubtaskupdatedDto
    {
        public string SubTaskName { get; set; } = string.Empty;
        public string Status { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public DateTime Datetime { get; set; }
        public decimal WorkedHours { get; set; }
        public int TaskItemId { get; set; }
    }

}