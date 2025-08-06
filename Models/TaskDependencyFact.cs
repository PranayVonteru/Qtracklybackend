namespace Demoproject.Models
{
    public class TaskDependencyFact
    {
        public int Id { get; set; }
        public int TaskId {  get; set; }
        public int DependencyTaskId { get; set; }
  
    }
}
