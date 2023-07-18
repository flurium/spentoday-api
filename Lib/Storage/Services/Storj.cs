using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Storage.Services;

public class Storj : IStorage
{
    private AmazonS3Client client;

    public Storj()
    {
        client = new AmazonS3Client();
    }

    public (StorageItem item, Exception? error) Upload()
    {
        throw new NotImplementedException();
    }
}