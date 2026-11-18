namespace Entities;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public int UserId { get; set; }                 // foreign key
    public User User { get; set; } = null!;         // navigation: the author

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}