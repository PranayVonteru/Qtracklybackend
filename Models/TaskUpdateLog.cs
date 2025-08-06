//namespace Demoproject.Models
//{
//    public class TaskUpdateLog
//    {

//    }
//}

using System;

namespace Demoproject.Models
{
    public class TaskUpdateLog
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime UpdateDate { get; set; }
        public decimal WorkedHours { get; set; }
        public virtual TaskItem Task { get; set; }
    }
}
