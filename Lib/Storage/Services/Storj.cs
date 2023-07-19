using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

namespace Lib.Storage.Services;

/// <summary>
/// Storj storage.
/// </summary>
public class Storj : IStorage
{
    private const string provider = "storj";
    private readonly AmazonS3Client client;
    private readonly string publicKey;

    /// <param name="accessKey">Access key to S3 compatible gateway.</param>
    /// <param name="secretKey">Secret key to S3 compatible gateway.</param>
    /// <param name="endpoint">Endpoint to S3 compatible gateway.</param>
    /// <param name="publicKey">Public key to create url to a public file.</param>
    public Storj(string accessKey, string secretKey, string endpoint, string publicKey)
    {
        var config = new AmazonS3Config() { ServiceURL = endpoint };
        client = new AmazonS3Client(accessKey, secretKey, config);
        this.publicKey = publicKey;
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

            return Http.IsSuccessful((int)response.HttpStatusCode);
        }
        catch
        {
            // TODO: log exception
            return false;
        }
    }

    public string Url(StorageItem item)
    {
        return $"https://link.storjshare.io/raw/{publicKey}/{item.Bucket}/{item.Key}";
    }
}