using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GalleryApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GalleryApp.Controllers;
public class HomeController : Controller {
	private readonly ILogger<HomeController> _logger;
    private BlobContainerClient container;

    public HomeController(ILogger<HomeController> logger) {
		_logger = logger;
	}

	private async Task Connect() {
        var connectionString = "";
        var client = new BlobServiceClient(connectionString);
        container = client.GetBlobContainerClient("images");
        await container.CreateIfNotExistsAsync();
    }

	public async Task<IActionResult> Index() {
        await Connect();

        var images = new List<string>();
        await foreach(BlobItem blobItem in container.GetBlobsAsync()) {
            BlobClient blob = container.GetBlobClient(blobItem.Name);
            images.Add(blob.Uri.ToString());
        }

		return View(images);
	}

	[HttpPost]
    public async Task<IActionResult> Index([FromForm] IFormCollection formData) {
        var allowedContentTypes = new List<string> { "image/png", "image/jpeg" };

        await Connect();

        foreach(var file in formData.Files) {
            if(!allowedContentTypes.Contains(file.ContentType))
                continue;

            // uplaod to azure storage
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var now = DateTime.Now;
            var filePath = $"{now.Year}/{now.Month}/{fileName}";
            BlobClient blob = container.GetBlobClient(filePath);
            await blob.UploadAsync(file.OpenReadStream(), true);

            blob.SetHttpHeaders(new BlobHttpHeaders {
                ContentType = file.ContentType // "image/jpeg"
            });
        }

        return RedirectToAction(nameof(Index));
	}

	public IActionResult Privacy() {
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error() {
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}
