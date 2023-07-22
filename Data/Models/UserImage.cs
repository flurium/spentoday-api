using Lib.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
     public class UserImage : IStorageFile
    {
        public string Id { get; } = Guid.NewGuid().ToString();

        public string Provider { get; }
        public string Bucket { get; }
        public string Key { get; }

        public string UserId { get; set; }
        public User? User { get; set; }

        public UserImage(IStorageFile file, string productId)
        {
            Provider = file.Provider;
            Bucket = file.Bucket;
            Key = file.Key;
            UserId = productId;
        }
    }
}
