using Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class PlanLimits
{
    public static async Task<bool> ReachedShopLimit(Db db, string userId)
    {
        var shopsCount = await db.Shops
            .Where(x => x.OwnerId == userId)
            .CountAsync().ConfigureAwait(false);
        return shopsCount >= 1;
    }

    public static async Task<bool> ReachedProductLimit(Db db, string userId)
    {
        var productsCount = await db.Products
            .Where(x => x.Shop.OwnerId == userId)
            .CountAsync().ConfigureAwait(false);
        return productsCount >= 10;
    }

    public static async Task<bool> ReachedPagesLimit(Db db, string userId)
    {
        var pagesCout = await db.InfoPages
            .Where(x => x.Shop.OwnerId == userId)
            .CountAsync().ConfigureAwait(false);
        return pagesCout >= 5;
    }
}