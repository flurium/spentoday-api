using Lib.Storage;

namespace Data.Models.UserTables
{
    public class UserImage : IStorageFile
    {
        public string Id { get; } = Guid.NewGuid().ToString();

        public string Provider { get; set; }
        public string Bucket { get; set; }
        public string Key { get; set; }

        public string UserId { get; set; }
        public User User { get; set; } = default!;

        public UserImage(string provider, string bucket, string key, string userId)
        {
            Provider = provider;
            Bucket = bucket;
            Key = key;
            UserId = userId;
        }
    }
}