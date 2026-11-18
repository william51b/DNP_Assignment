using Entities;
using Microsoft.EntityFrameworkCore;

namespace EfcRepositories;

public class AppDbContext : DbContext
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={GetDbPath()}");
    }

    // Walks up from the running folder until it finds the .sln file,
    // then points at EfcRepositories/app.db. Works for dotnet ef and the WebAPI.
    private static string GetDbPath()
    {
        DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !dir.GetFiles("*.sln*").Any())
        {
            dir = dir.Parent;
        }

        string root = dir?.FullName ?? AppContext.BaseDirectory;
        return Path.Combine(root, "EfcRepositories", "app.db");
    }
}