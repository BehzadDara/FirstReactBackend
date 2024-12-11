namespace FirstReactBackend;

public class TaskEntity
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public PriorityType Priority { get; set; }
    public bool IsDone { get; set; } = false;
    public int UserId { get; set; }
    public UserEntity? User { get; set; }
}
