namespace Data.Models.UserTables;

public class Question
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Email { get; set; }

    public string Content { get; set; }

    public Question(string email, string content)
    {
        Email = email;
        Content = content;
    }
}