﻿namespace FirstReactBackend;

public class TasksListViewModel
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<Task> Tasks { get; set; } = [];
}
