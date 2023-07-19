using Microsoft.EntityFrameworkCore;

namespace Lib.EntityFrameworkCore;

public static class DbExtension
{
    /// <summary>Save changes in database. But doesn't throw.</summary>
    /// <returns>True if successful, false if not.</returns>
    public static async Task<bool> Save(this DbContext db)
    {
        try
        {
            await db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}