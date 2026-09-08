using Entities;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;

    public CliApp(IUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }

    public async Task StartAsync()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("=== Menu ===");
            Console.WriteLine("1. Create new user");
            Console.WriteLine("2. Create new post");
            Console.WriteLine("3. Add comment to post");
            Console.WriteLine("4. View posts overview");
            Console.WriteLine("5. View specific post");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await CreateUserAsync();
                    break;
                case "2":
                    await CreatePostAsync();
                    break;
                case "3":
                    await AddCommentAsync();
                    break;
                case "4":
                    await ViewPostsOverviewAsync();
                    break;
                case "5":
                    await ViewSpecificPostAsync();
                    break;
                case "0":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option, try again.");
                    break;
            }
        }
    }

    private async Task CreateUserAsync()
    {
        Console.Write("Enter username: ");
        string username = Console.ReadLine()!;

        Console.Write("Enter password: ");
        string password = Console.ReadLine()!;

        User user = new User
        {
            Username = username,
            Password = password
        };

        User created = await userRepository.AddAsync(user);

        Console.WriteLine($"User created with ID: {created.Id}");
    }

    private async Task CreatePostAsync()
    {
        Console.Write("Enter title: ");
        string title = Console.ReadLine()!;

        Console.Write("Enter body: ");
        string body = Console.ReadLine()!;

        Console.Write("Enter your user ID: ");
        int userId = int.Parse(Console.ReadLine()!);

        Post post = new Post
        {
            Title = title,
            Body = body,
            UserId = userId
        };

        Post created = await postRepository.AddAsync(post);

        Console.WriteLine($"Post created with ID: {created.Id}");
    }

    private async Task AddCommentAsync()
    {
        Console.Write("Enter post ID to comment on: ");
        int postId = int.Parse(Console.ReadLine()!);

        Console.Write("Enter your user ID: ");
        int userId = int.Parse(Console.ReadLine()!);

        Console.Write("Enter comment body: ");
        string body = Console.ReadLine()!;

        Comment comment = new Comment
        {
            Body = body,
            PostId = postId,
            UserId = userId
        };

        Comment created = await commentRepository.AddAsync(comment);

        Console.WriteLine($"Comment created with ID: {created.Id}");
    }

    private async Task ViewPostsOverviewAsync()
    {
        IQueryable<Post> posts = postRepository.GetManyAsync();

        Console.WriteLine();
        Console.WriteLine("=== Posts ===");
        foreach (Post post in posts)
        {
            Console.WriteLine($"[{post.Id}] {post.Title}");
        }
    }

    private async Task ViewSpecificPostAsync()
    {
        Console.Write("Enter post ID: ");
        int postId = int.Parse(Console.ReadLine()!);

        Post post = await postRepository.GetSingleAsync(postId);

        Console.WriteLine();
        Console.WriteLine($"Title: {post.Title}");
        Console.WriteLine($"Body: {post.Body}");

        IQueryable<Comment> comments = commentRepository.GetManyAsync()
            .Where(c => c.PostId == postId);

        Console.WriteLine("Comments:");
        foreach (Comment comment in comments)
        {
            Console.WriteLine($"- {comment.Body}");
        }
    }
}