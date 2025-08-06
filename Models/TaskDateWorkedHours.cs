using System.ComponentModel.DataAnnotations;

namespace Demoproject.Models
{
    public class TaskDateWorkedHours
    {
        [Key]
        public int Id   { get; set; }
        public DateTime DateTime { get; set; }
        public int TaskId { get; set; }
        public decimal WorkedHours {  get; set; }
    }
}
