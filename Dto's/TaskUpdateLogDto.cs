//namespace Demoproject.Dto_s
//{
//    public class TaskUpdateLogDto
//    {
//    }
//}
using System;

namespace Demoproject.Dtos
{
    public class TaskUpdateLogDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime UpdateDate { get; set; }
        public decimal WorkedHours { get; set; }
    }
}