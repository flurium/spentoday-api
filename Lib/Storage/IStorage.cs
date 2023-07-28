namespace Lib.Storage;

public interface IStorageFile
{
    public string Provider { get; }
    public string Bucket { get; }
    public string Key { get; }
}

public record class StorageFile(string Bucket, string Key, string Provider) : IStorageFile;

public interface IStorage
{
    /// <summary>
    /// Upload file to a storage.
    /// </summary>
    /// <param name="key">Key/path to item</param>
    /// <param name="fileStream">Stream of file</param>
    /// <returns>
    /// Bucket name, path and provider. Provider is used for migrations,
    /// multistorage usage, creating link to the file.
    /// </returns>
    public Task<IStorageFile?> Upload(string key, Stream fileStream);

    /// <param name="bucket">Bucket name</param>
    /// <param name="key">Key/path to item</param>
    /// <returns>True if success, false if failed</returns>
    public Task<bool> Delete(IStorageFile file);

    /// <summary>
    /// Create url to a public item. Not signed url!
    /// This method must not make request to a server.
    /// You can to create this function of the frontend and use it from there.
    /// </summary>
    public string Url(IStorageFile file);
}