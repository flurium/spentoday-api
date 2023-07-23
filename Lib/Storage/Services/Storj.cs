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

    public async Task<IStorageFile?> Upload(string key, Stream fileStream)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                InputStream = fileStream,
                CannedACL = S3CannedACL.PublicRead
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

    public async Task<bool> Delete(IStorageFile file)
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

    public string Url(IStorageFile file)
    {
        return $"https://link.storjshare.io/raw/{publicKey}/{file.Bucket}/{file.Key}";
    }
}