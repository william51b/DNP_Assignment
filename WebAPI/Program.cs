using EfcRepositories;
using RepositoryContracts;
using WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IUserRepository, EfcUserRepository>();
builder.Services.AddScoped<IPostRepository, EfcPostRepository>();
builder.Services.AddScoped<ICommentRepository, EfcCommentRepository>();

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapControllers();

app.Run();