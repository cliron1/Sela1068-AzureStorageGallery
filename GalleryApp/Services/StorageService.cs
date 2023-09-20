using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GalleryApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GalleryApp.Services;

public interface IStorageService
{
    Task<List<string>> GetFiles();
    Task Upload(IFormFile file);
}

public class StorageService : IStorageService
{
    private BlobContainerClient container;

    public StorageService(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Storage");
        var client = new BlobServiceClient(connectionString);
        container = client.GetBlobContainerClient("images");
        container.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<List<string>> GetFiles()
    {
        var ret = new List<string>();
        await foreach (BlobItem blobItem in container.GetBlobsAsync())
        {
            BlobClient blob = container.GetBlobClient(blobItem.Name);
            ret.Add(blob.Uri.ToString());
        }

        return ret;
    }

    public async Task Upload(IFormFile file)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var now = DateTime.Now;
        var filePath = $"{now.Year}/{now.Month}/{fileName}";
        BlobClient blob = container.GetBlobClient(filePath);
        await blob.UploadAsync(file.OpenReadStream(), true);

        blob.SetHttpHeaders(new BlobHttpHeaders
        {
            ContentType = file.ContentType // "image/jpeg"
        });
    }
}
