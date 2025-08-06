using Demoproject.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Dependency
{
    [Key]
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public int? DependsOnTaskId { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    [JsonIgnore]
    public TaskItem TaskItem { get; set; }
}