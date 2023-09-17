namespace Lib;

/// <summary>
/// Helper to work with environment variables.
/// </summary>
public static class Env
{
    /// <summary>
    /// Load environment variables from file if file exists.
    /// If file doesn't exist then it just do nothing.
    /// </summary>
    public static void LoadFile(string path)
    {
        if (!File.Exists(path)) return;

        foreach (var line in File.ReadAllLines(path))
        {
            var separatorIndex = line.IndexOf("=");

            var key = line.Substring(0, separatorIndex).Trim();
            var value = line.Substring(separatorIndex + 1).Trim();

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    /// <summary>
    /// Get required environment variable, throw if variable isn't found.
    /// </summary>
    public static string GetRequired(string key)
         => Environment.GetEnvironmentVariable(key)
        ?? throw new KeyNotFoundException($"Environment variable: {key} wasn't found!");

    /// <summary>Get optional environment variable.</summary>
    /// <returns>Null if not found, otherwise value</returns>
    public static string? GetOptional(string key) => Environment.GetEnvironmentVariable(key);
}