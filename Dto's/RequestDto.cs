namespace Demoproject.Dto_s
{
    public class RequestDto
    {  
        public string UserId { get; set; }
        public string TaskName { get; set; }
        public string TargetUser { get; set; }
        public string RequestedTask { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public string EstimatedImpact { get; set; }
        public int TaskId {  get; set; }    

    }
}
