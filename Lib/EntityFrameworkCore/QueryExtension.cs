using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Lib.EntityFrameworkCore;

public static class QueryExtension
{
    /// <summary>Query one/first item with ConfigureAwait(false)</summary>
    /// <returns>Item if found otherwise null.</returns>
    public static async Task<T?> QueryOne<T>(this IQueryable<T> query)
    {
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Query one/first item that matches <paramref name="where"/> condition with ConfigureAwait(false)
    /// </summary>
    /// <param name="where">Condition to filter.</param>
    /// <returns>Item if found otherwise null.</returns>
    public static async Task<T?> QueryOne<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return await query.FirstOrDefaultAsync(where).ConfigureAwait(false);
    }

    /// <summary>
    /// Query many items that match where condition with ConfigureAwait(false)
    /// </summary>
    /// <returns>List of item.</returns>
    public static async Task<List<T>> QueryMany<T>(this IQueryable<T> query)
    {
        return await query.ToListAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Query many items that match <paramref name="where"/> condition with ConfigureAwait(false)
    /// </summary>
    /// <param name="where">Condition to filter.</param>
    /// <returns>List of items</returns>
    public static async Task<List<T>> QueryMany<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return await query.Where(where).ToListAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Query many items that match <paramref name="where"/> condition with ConfigureAwait(false)
    /// </summary>
    /// <param name="where">Condition to filter.</param>
    /// <returns>List of items</returns>
    public static async Task<bool> Have<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return await query.AnyAsync(where).ConfigureAwait(false);
    }
}