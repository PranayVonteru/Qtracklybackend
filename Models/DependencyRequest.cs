using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Demoproject.Models
{
    public class DependencyRequest
    {
        //[Key]
        //[Required]

        //public string DependencyTaskId { get; set; } // Fixed definition

        //public string TaskName { get; set; }
        //public string RequestedTask { get; set; }
        //public string Priority { get; set; }
        //public string Description { get; set; }
        //public string EstimatedImpact { get; set; }
        //public DateTime RequestedDate { get; set; }

        [Key]
        [Required]
        public int DependencyTaskId { get; set; }

        public string TaskName { get; set; }
        public string RequestedTask { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public string EstimatedImpact { get; set; }
        public DateTime RequestedDate { get; set; }

        public DependencyFact DependencyFact { get; set; } // Navigation property
    }
}
