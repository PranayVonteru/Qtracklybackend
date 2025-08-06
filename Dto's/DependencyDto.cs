using System.ComponentModel.DataAnnotations;

namespace Demoproject.Dtos
{
    public class DependencyDto
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public int? DependsOnTaskId { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public string DependsOnTaskName { get; set; } = string.Empty;
    }

    public class DependencyCreateDto
    {
        [Required]
        public int TaskItemId { get; set; }
        public int? DependsOnTaskId { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class DependencyUpdateDto
    {
        public int? DependsOnTaskId { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}