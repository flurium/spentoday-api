using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Storage;

public record class StorageItem(string Bucket, string Key, string Provider);

public interface IStorage
{
    public Task<StorageItem?> Upload(string bucket, string filename, Stream fileStream);

    /// <param name="bucket">Bucket name</param>
    /// <param name="key">Key/path to item</param>
    /// <returns>True if success, false if failed</returns>
    public Task<bool> Delete(string bucket, string key);
}