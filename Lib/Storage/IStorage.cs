using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Storage;

public record class StorageItem(string Url, string Provider);

public interface IStorage
{
    public (StorageItem item, Exception? error) Upload();
}