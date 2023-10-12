using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

namespace Lib.Storage.Services;

/// <summary>
/// Handle files inside storage of Storj service.
/// </summary>
public class Storj : IStorage
{
    /// <summary>Never change this value. It's key for this provider.</summary>
    private const string provider = "storj";

    private readonly AmazonS3Client client;
    private readonly string publicKey;
    private readonly string bucket;

    /// <param name="accessKey">Access key to S3 compatible gateway.</param>
    /// <param name="secretKey">Secret key to S3 compatible gateway.</param>
    /// <param name="endpoint">Endpoint to S3 compatible gateway.</param>
    /// <param name="publicKey">Public key to create url to a public file.</param>
    public Storj(string accessKey, string secretKey, string endpoint, string publicKey, string bucket)
    {
        var config = new AmazonS3Config() { ServiceURL = endpoint };
        client = new AmazonS3Client(accessKey, secretKey, config);
        this.publicKey = publicKey;
        this.bucket = bucket;
    }

    /// <summary>
    /// Upload file to Stroj. Includes Cache-Control header for 1 year.
    /// </summary>
    public async Task<StorageFile?> Upload(string key, Stream fileStream)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                InputStream = fileStream,
                CannedACL = S3CannedACL.PublicRead,
                Headers =
                {
                    CacheControl = "public, max-age=31536000"
                }
            };

            var response = await client.PutObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK) return null;
            return new StorageFile(bucket, key, provider);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> Delete(StorageFile file)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = file.Bucket,
                Key = file.Key
            };

            var response = await client.DeleteObjectAsync(request);

            return Http.IsSuccessful((int)response.HttpStatusCode);
        }
        catch
        {
            return false;
        }
    }

    public async void Download(StorageFile file)
    {
        var req = new GetObjectRequest { BucketName = file.Bucket, Key = file.Key };
        var res = await client.GetObjectAsync(req);
    }

    public string Url(StorageFile file)
    {
        return $"https://link.storjshare.io/raw/{publicKey}/{file.Bucket}/{file.Key}";
    }
}