namespace FirstReactBackend;

public class UserEntity
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string HashedPassword { get; set; }
    public List<TaskEntity> Tasks { get; set; } = [];
}
