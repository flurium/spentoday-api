using Lib;
using System.ComponentModel.DataAnnotations;

namespace Backend.Services;

public static class SlugExtension
{
    public static bool IsSlug(this string slug)
    {
        for (int i = 0; i < slug.Length; ++i)
        {
            char c = slug[i];
            if (!(char.IsLetter(c) || char.IsDigit(c) || c == '-')) return false;
        }
        return true;
    }
}

public static class EmailExtension
{
    public static bool IsValidEmail(this string email)
    {
        return new EmailAddressAttribute().IsValid(email);
    }
}

public static class Dev
{
    private static bool IsDevEnv() => (Env.GetOptional("ASPNETCORE_ENVIRONMENT") ?? "") == "Development";

    /// <summary>
    /// Logging to Console if Dev, otherwise doing nothing.
    /// </summary>
    public static readonly Action<string> Log = IsDevEnv() ? (message) => Console.WriteLine(message) : (message) => { };
}