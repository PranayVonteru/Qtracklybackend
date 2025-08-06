namespace Demoproject.Models
{
    public class TaskStats
    {
        public int Id { get; set; }
        public int NotStarted { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
        public int Waiting { get; set; }
        public string UserId { get; set; }
    }
}