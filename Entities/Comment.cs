namespace Entities;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;

    public int PostId { get; set; }                 // foreign key
    public Post Post { get; set; } = null!;         // navigation: the post it belongs to

    public int UserId { get; set; }                 // foreign key
    public User User { get; set; } = null!;         // navigation: the author
}