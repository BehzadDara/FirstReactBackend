using FirstReactBackend;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "http://localhost:28747/",
        ValidAudience = "http://localhost:28747/",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("FirsReactBackendBEHZADDARAFirsReactBackendBEHZADDARAFirsReactBackendBEHZADDARA" ?? "AlternativeKey"))
    };
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<CurrentUser>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<FierstReactBackendDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("AllowSpecificOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region User

app.MapPost("/Users/Login", async (FierstReactBackendDBContext db, TokenService tokenService, LoginRegisterDTO input) =>
{
    if (string.IsNullOrEmpty(input.UserName))
    {
        return Results.BadRequest($"UserName can not be null or empty");
    }
    if (string.IsNullOrEmpty(input.Password))
    {
        return Results.BadRequest($"Password can not be null or empty");
    }

    var hashedPassword = HashPassword(input.Password);

    var user = await db.Users.FirstOrDefaultAsync(x => x.UserName == input.UserName && x.HashedPassword == hashedPassword);
    if (user is null)
        return Results.NotFound($"User with UserName {input.UserName} and Password {input.Password} not found.");

    var token = tokenService.Generate(user.Id);

    return Results.Ok(token);
});

app.MapPost("/Users/Register", async (FierstReactBackendDBContext db, TokenService tokenService, LoginRegisterDTO input) =>
{
    if (string.IsNullOrEmpty(input.UserName))
    {
        return Results.BadRequest($"UserName can not be null or empty");
    }
    if (string.IsNullOrEmpty(input.Password))
    {
        return Results.BadRequest($"Password can not be null or empty");
    }

    var user = new UserEntity
    {
        UserName = input.UserName,
        HashedPassword = HashPassword(input.Password),
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    var token = tokenService.Generate(user.Id);

    return Results.Ok(token);
});

app.MapPatch("/Users", [Authorize] async (FierstReactBackendDBContext db, CurrentUser currentUser, ChangePasswordDTO input) =>
{
    if (string.IsNullOrEmpty(input.CurrentPassword))
    {
        return Results.BadRequest($"CurrentPassword can not be null or empty");
    }
    if (string.IsNullOrEmpty(input.NewPassword))
    {
        return Results.BadRequest($"NewPassword can not be null or empty");
    }

    var hashedPassword = HashPassword(input.CurrentPassword);

    var user = await db.Users.FirstOrDefaultAsync(x => x.Id == currentUser.Id && x.HashedPassword == hashedPassword);
    if (user is null)
        return Results.NotFound($"Incorrect password.");

    user.HashedPassword = HashPassword(input.NewPassword);
    await db.SaveChangesAsync();
    return Results.Ok();
});

#endregion

#region Task

app.MapGet("/tasks", [Authorize] async (FierstReactBackendDBContext db, CurrentUser currentUser, int pageNumber, int pageSize) =>
{
    Thread.Sleep(1000);

    var tasks = await
    db
    .Tasks
    .Where(x => x.UserId == currentUser.Id)
    .OrderBy(x => x.IsDone)
    .ThenBy(x => x.Priority)
    .ThenByDescending(x => x.Id)
    .Skip(pageSize * (pageNumber - 1))
    .Take(pageSize)
    .ToListAsync();

    var totalCount = await
    db
    .Tasks
    .CountAsync();

    var result = new TasksListViewModel
    {
        TotalCount = totalCount,
        PageSize = pageSize,
        PageNumber = pageNumber,
        Tasks = tasks
    };

    return Results.Ok(result);
});

app.MapGet("/tasks/{id:int}", [Authorize] async (FierstReactBackendDBContext db, CurrentUser currentUser, int id) =>
{
    var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == currentUser.Id);
    if (task is null)
        return Results.NotFound($"Task with ID {id} not found.");

    return Results.Ok(task);
});

app.MapPost("/tasks", [Authorize] async (FierstReactBackendDBContext db, CurrentUser currentUser, CreateTaskDTO input) =>
{
    if (string.IsNullOrEmpty(input.Title))
    {
        return Results.BadRequest($"Title can not be null or empty");
    }
    if (!Enum.IsDefined(typeof(PriorityType), input.Priority))
    {
        return Results.BadRequest($"Invalid priority value: {input.Priority}. Allowed values are: High (0), Low (1), Medium (2).");
    }

    var task = new TaskEntity
    {
        Title = input.Title,
        Priority = input.Priority,
        UserId = currentUser.Id
    };

    await db.Tasks.AddAsync(task);
    await db.SaveChangesAsync();
    return Results.Ok(input);
});

app.MapDelete("/tasks/{id:int}", [Authorize] async (FierstReactBackendDBContext db, CurrentUser currentUser, int id) =>
{
    var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == currentUser.Id);
    if (task is null)
        return Results.NotFound($"Task with ID {id} not found.");

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.Ok(task);
});

app.MapPatch("/tasks/{id:int}", [Authorize] async (FierstReactBackendDBContext db, CurrentUser currentUser, int id) =>
{
    var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == currentUser.Id);
    if (task == null)
        return Results.NotFound($"Task with ID {id} not found.");

    task.IsDone = !task.IsDone;
    await db.SaveChangesAsync();
    return Results.Ok(task);
});

#endregion

app.MapHub<TimerHub>("/timer");

app.Run();


static string HashPassword(string password)
{
    byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
    return BitConverter.ToString(bytes).ToLower();
}