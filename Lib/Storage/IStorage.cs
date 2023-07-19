namespace Lib.Storage;

public record class StorageItem(string Bucket, string Key, string Provider);

public interface IStorage
{
    /// <summary>
    /// Upload file to a storage.
    /// </summary>
    /// <param name="bucket">Bucket name</param>
    /// <param name="filename">Key/path to item</param>
    /// <param name="fileStream">Stream of file</param>
    /// <returns>
    /// Bucket name, path and provider. Provider is used for migrations,
    /// multistorage usage, creating link to the file.
    /// </returns>
    public Task<StorageItem?> Upload(string bucket, string filename, Stream fileStream);

    /// <param name="bucket">Bucket name</param>
    /// <param name="key">Key/path to item</param>
    /// <returns>True if success, false if failed</returns>
    public Task<bool> Delete(string bucket, string key);

    /// <summary>
    /// Create url to a public item. Not signed url!
    /// This method must not make request to a server.
    /// You can to create this function of the frontend and use it from there.
    /// </summary>
    public string Url(StorageItem item);
}