using FileRepositories;
using RepositoryContracts;
using WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IUserRepository, UserFileRepository>();
builder.Services.AddScoped<IPostRepository, PostFileRepository>();
builder.Services.AddScoped<ICommentRepository, CommentFileRepository>();

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapControllers();

app.Run();