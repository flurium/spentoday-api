using Microsoft.AspNetCore.Identity;

namespace Data.Models;

public class User : IdentityUser
{
    public int Version = 0;

    public IReadOnlyCollection<Shop> Shops { get; set; } = default!;

    public string Name { get; set; }

    public string? ImageId { get; set; }
    public UserImage? Image { get; set; } = default!;

    public User(string name, string email, string? imageId = null)
    {
        Name = name;
        Email = email;
        ImageId = imageId;
    }
}