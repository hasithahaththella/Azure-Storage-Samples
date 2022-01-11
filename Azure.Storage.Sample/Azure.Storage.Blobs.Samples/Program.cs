using Azure.Storage.Blobs.Models;
using System;
using System.Resources;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Azure.Storage.Blobs.Samples
{
    class Program
    {
        private static string _connectionString;
        private static readonly string _blobContainer = "pictures";
        private static string _blobName = nameof(Properties.Resources.Whakapapa);

        private async static Task Main(string[] args)
        {
            Console.WriteLine("Azure Storage Connection String : ");
            _connectionString = Console.ReadLine().Trim();

            // Create the blob container
           var blobContainerClient = await CreateContainer();

            // Upload a blob
            await UploadBlob(blobContainerClient);

            // List Containers with their contents i.e. blobs
            await ListContainersWithTheirBlobsAsync();

            // Download the uploaded blob
            await DownloadBlob();

            Console.WriteLine("End of the sample");
        }

        private async static Task<BlobContainerClient> CreateContainer()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_blobContainer);
            Console.WriteLine($"\r\n1. Creating blob container '{_blobContainer}'");
            var result = await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
            Console.WriteLine($"    ETag : {result?.Value?.ETag}\r\n");
            return blobContainerClient;
        }

        private async static Task UploadBlob(BlobContainerClient blobContainerClient)
        {
            var blobClient = blobContainerClient.GetBlobClient(_blobName);
            
            // Reading and parsing the image file from the resource
            var resourceManager = Properties.Resources.ResourceManager;
            var memoryStream = resourceManager.GetMemoryStream(_blobName);

            Console.WriteLine($"2. Uploading blob '{blobClient.Name}'");
            Console.WriteLine($"   URL > {blobClient.Uri}"); 

            var updateResults = await blobClient.UploadAsync(
                memoryStream,
                new BlobHttpHeaders { ContentType = "Image" });

            Console.WriteLine($"    Blob has been uploaded.");
            Console.WriteLine($"    ETag > {updateResults.Value.ETag}\r\n");
        }

        private async static Task ListContainersWithTheirBlobsAsync()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            Console.WriteLine($"3. Listing containers and blobs of {blobServiceClient.AccountName}.");

            await foreach (var blobContainerItem in blobServiceClient.GetBlobContainersAsync())
            {
                Console.WriteLine($"    > {blobContainerItem.Name}");

                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_blobContainer);
                await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
                {
                    Console.WriteLine($"        > {blobItem.Name}");
                }
            }
        }

        private async static Task DownloadBlob()
        {
            var localFile = $"{System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{_blobName}.jpg")}";

            Console.WriteLine($"4. Downloading blob {_blobName} to local file : {localFile}");

            var blobClient = new BlobClient(_connectionString, _blobContainer, _blobName);

            var exists = await blobClient.ExistsAsync();

            if (exists)
            {
                var blobDownloadResult = await blobClient.DownloadContentAsync();

                using (var fileStream = File.OpenWrite(localFile))
                {
                    fileStream.Write(blobDownloadResult.Value.Content);
                }

                Process.Start($"Explorer {System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory)}");
            }


        }

        private async static Task DeleteContainer()
        {

        }
    }
}


