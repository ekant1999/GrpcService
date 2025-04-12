namespace  TodoGrpc.Models;

public class TodoItem
{
    public int Id {get; set;}
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string TodoStatus { get; set; } = "New";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

}