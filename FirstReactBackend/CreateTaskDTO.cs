namespace FirstReactBackend;

public class CreateTaskDTO
{
    public required string Title { get; set; }
    public PriorityType Priority { get; set; }
}
