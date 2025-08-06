using System.ComponentModel.DataAnnotations;

namespace Demoproject.Models
{
    public class DependencyFact
    {
        

        [Key]
        public int Id { get; set; }

        [Required]
        public int DependencyTaskId { get; set; } // Foreign key
        public DependencyRequest DependencyRequest { get; set; } // Navigation property
        public int? DependsOnTaskId { get; set; }
        public string UserId { get; set; }
        public string TargetUserId { get; set; }
        public string Status { get; set; }
    }
}
