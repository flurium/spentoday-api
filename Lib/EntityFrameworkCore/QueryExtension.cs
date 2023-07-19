using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Lib.EntityFrameworkCore;

public static class QueryExtension
{
    public static async Task<T?> QueryOne<T>(this IQueryable<T> query)
    {
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public static async Task<T?> QueryOne<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return await query.FirstOrDefaultAsync(where).ConfigureAwait(false);
    }

    public static async Task<IEnumerable<T>> QueryMany<T>(this IQueryable<T> query)
    {
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public static async Task<IEnumerable<T>> QueryMany<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return await query.Where(where).ToListAsync().ConfigureAwait(false);
    }
}