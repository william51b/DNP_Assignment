using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;
    private readonly ICommentRepository commentRepo;

    public PostsController(IPostRepository postRepo, IUserRepository userRepo, ICommentRepository commentRepo)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
        this.commentRepo = commentRepo;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("Title and body are required.");

        User author = await GetAuthorOrNullAsync(request.UserId)
                      ?? throw new NotFoundException($"User with ID '{request.UserId}' not found");

        Post created = await postRepo.AddAsync(new Post
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId
        });

        PostDto dto = ToDto(created, author.Username);
        return Created($"/posts/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDto>> UpdatePost([FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("Title and body are required.");

        Post existing = await postRepo.GetSingleAsync(id);
        existing.Title = request.Title;
        existing.Body = request.Body;
        await postRepo.UpdateAsync(existing);

        User? author = await GetAuthorOrNullAsync(existing.UserId);
        return Ok(ToDto(existing, author?.Username ?? "Unknown"));
    }

    // GET /posts/{id}?includeComments=true
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetPost([FromRoute] int id, [FromQuery] bool includeComments = false)
    {
        IQueryable<Post> query = postRepo.GetManyAsync().Where(p => p.Id == id);
        PostDto? dto;

        if (includeComments)
        {
            dto = await query.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                AuthorName = p.User.Username,
                Comments = p.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Body = c.Body,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    AuthorName = c.User.Username
                }).ToList()
            }).FirstOrDefaultAsync();
        }
        else
        {
            dto = await query.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                AuthorName = p.User.Username
            }).FirstOrDefaultAsync();
        }

        return dto == null ? NotFound() : Ok(dto);
    }

    // GET /posts?titleContains=abc&userId=1&userName=bob
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts(
        [FromQuery] string? titleContains,
        [FromQuery] int? userId,
        [FromQuery] string? userName)
    {
        IQueryable<Post> posts = postRepo.GetManyAsync();

        if (!string.IsNullOrWhiteSpace(titleContains))
            posts = posts.Where(p => p.Title.ToLower().Contains(titleContains.ToLower()));

        if (userId.HasValue)
            posts = posts.Where(p => p.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(userName))
            posts = posts.Where(p => p.User.Username.ToLower().Contains(userName.ToLower()));

        List<PostDto> result = await posts.Select(p => new PostDto
        {
            Id = p.Id,
            Title = p.Title,
            Body = p.Body,
            UserId = p.UserId,
            AuthorName = p.User.Username
        }).ToListAsync();

        return Ok(result);
    }

    // GET /posts/{postId}/comments
    [HttpGet("{postId:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPost([FromRoute] int postId)
    {
        await postRepo.GetSingleAsync(postId);

        List<CommentDto> comments = await commentRepo.GetManyAsync()
            .Where(c => c.PostId == postId)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                PostId = c.PostId,
                UserId = c.UserId,
                AuthorName = c.User.Username
            }).ToListAsync();

        return Ok(comments);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePost([FromRoute] int id)
    {
        await postRepo.DeleteAsync(id);
        return NoContent();
    }

    private async Task<User?> GetAuthorOrNullAsync(int userId)
    {
        try { return await userRepo.GetSingleAsync(userId); }
        catch (NotFoundException) { return null; }
    }

    private static PostDto ToDto(Post p, string authorName) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Body = p.Body,
        UserId = p.UserId,
        AuthorName = authorName
    };
}