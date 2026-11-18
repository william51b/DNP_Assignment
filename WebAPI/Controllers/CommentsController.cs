using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepo;
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;

    public CommentsController(ICommentRepository commentRepo, IPostRepository postRepo, IUserRepository userRepo)
    {
        this.commentRepo = commentRepo;
        this.postRepo = postRepo;
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("Body is required.");

        await postRepo.GetSingleAsync(request.PostId);
        User author = await userRepo.GetSingleAsync(request.UserId);

        Comment created = await commentRepo.AddAsync(new Comment
        {
            Body = request.Body,
            PostId = request.PostId,
            UserId = request.UserId
        });

        CommentDto dto = ToDto(created, author.Username);
        return Created($"/comments/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CommentDto>> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("Body is required.");

        Comment existing = await commentRepo.GetSingleAsync(id);
        existing.Body = request.Body;
        await commentRepo.UpdateAsync(existing);

        return Ok(ToDto(existing, await AuthorNameAsync(existing.UserId)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetComment([FromRoute] int id)
    {
        Comment comment = await commentRepo.GetSingleAsync(id);
        return Ok(ToDto(comment, await AuthorNameAsync(comment.UserId)));
    }

    // GET /comments?userId=1&userName=bob&postId=2
    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetComments(
        [FromQuery] int? userId,
        [FromQuery] string? userName,
        [FromQuery] int? postId)
    {
        IQueryable<Comment> comments = commentRepo.GetManyAsync();
        List<User> users = userRepo.GetManyAsync().ToList();

        if (userId.HasValue)
            comments = comments.Where(c => c.UserId == userId.Value);

        if (postId.HasValue)
            comments = comments.Where(c => c.PostId == postId.Value);

        if (!string.IsNullOrWhiteSpace(userName))
        {
            List<int> matchingIds = users
                .Where(u => u.Username.ToLower().Contains(userName.ToLower()))
                .Select(u => u.Id).ToList();
            comments = comments.Where(c => matchingIds.Contains(c.UserId));
        }

        List<CommentDto> result = comments.ToList()
            .Select(c => ToDto(c, users.FirstOrDefault(u => u.Id == c.UserId)?.Username ?? "Unknown"))
            .ToList();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComment([FromRoute] int id)
    {
        await commentRepo.DeleteAsync(id);
        return NoContent();
    }

    private async Task<string> AuthorNameAsync(int userId)
    {
        try { return (await userRepo.GetSingleAsync(userId)).Username; }
        catch (NotFoundException) { return "Unknown"; }
    }

    private static CommentDto ToDto(Comment c, string authorName) => new()
    {
        Id = c.Id,
        Body = c.Body,
        PostId = c.PostId,
        UserId = c.UserId,
        AuthorName = authorName
    };
}