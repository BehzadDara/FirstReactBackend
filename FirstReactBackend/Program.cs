using FirstReactBackend;
using Microsoft.EntityFrameworkCore;
using Task = FirstReactBackend.Task;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FierstReactBackendDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/tasks", async (FierstReactBackendDBContext db) =>
{
    var tasks = await db.Tasks.OrderBy(x => x.IsDone).ThenBy(x => x.Priority).ThenByDescending(x => x.Id).ToListAsync();
    return Results.Ok(tasks);
});

app.MapGet("/tasks/{id:int}", async (FierstReactBackendDBContext db, int id) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null)
        return Results.NotFound($"Task with ID {id} not found.");

    return Results.Ok(task);
});

app.MapPost("/tasks", async (FierstReactBackendDBContext db, CreateTaskDTO input) =>
{
    if (string.IsNullOrEmpty(input.Title))
    {
        return Results.BadRequest($"Title can not be null or empty");
    }
    if (!Enum.IsDefined(typeof(PriorityType), input.Priority))
    {
        return Results.BadRequest($"Invalid priority value: {input.Priority}. Allowed values are: High (0), Low (1), Medium (2).");
    }

    var task = new Task
    {
        Title = input.Title,
        Priority = input.Priority,
    };

    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapDelete("/tasks/{id:int}", async (FierstReactBackendDBContext db, int id) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null)
        return Results.NotFound($"Task with ID {id} not found.");

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.Ok(task);
});

app.MapPatch("/tasks/{id:int}", async (FierstReactBackendDBContext db, int id) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task == null)
        return Results.NotFound($"Task with ID {id} not found.");

    task.IsDone = !task.IsDone;
    await db.SaveChangesAsync();
    return Results.Ok(task);
});

app.Run();
