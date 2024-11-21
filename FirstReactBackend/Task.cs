﻿namespace FirstReactBackend;

public class Task
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public PriorityType Priority { get; set; }
    public bool IsDone { get; set; } = false;
}
