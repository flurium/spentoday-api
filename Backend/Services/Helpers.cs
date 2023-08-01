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