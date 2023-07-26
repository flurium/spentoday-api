using Data.Models.ShopTables;
using Microsoft.AspNetCore.Identity;

namespace Data.Models.UserTables;

public class User : IdentityUser
{
    public int Version = 0;

    public IReadOnlyCollection<Shop> Shops { get; set; } = default!;

    public string Name { get; set; }

    public UserImage? Image { get; set; }

    public User(string name, string email)
    {
        Name = name;
        Email = email;
    }
}