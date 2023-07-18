using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

namespace Lib.Storage.Services;

public class Storj : IStorage
{
    private readonly AmazonS3Client client;
    private const string provider = "storj";

    public Storj(string accessKey, string secretKey, string endpoint)
    {
        var config = new AmazonS3Config() { ServiceURL = endpoint };
        client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<StorageItem?> Upload(string bucket, string filename, Stream fileStream)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = filename,
                InputStream = fileStream,
                CannedACL = S3CannedACL.PublicRead
            };

            var response = await client.PutObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK) return null;
            return new StorageItem(bucket, filename, provider);
        }
        catch
        {
            // TODO: log exception
            return null;
        }
    }

    public async Task<bool> Delete(string bucket, string key)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = key
            };

            var response = await client.DeleteObjectAsync(request);

            return response.HttpStatusCode == HttpStatusCode.OK;
        }
        catch
        {
            // TODO: log exception
            return false;
        }
    }
}