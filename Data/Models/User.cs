using Microsoft.AspNetCore.Identity;

namespace Data.Models;

public class User : IdentityUser
{
    public int Version = 0;

    public IReadOnlyCollection<Shop> Shops { get; set; } = default!;
}