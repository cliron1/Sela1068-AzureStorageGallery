using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GalleryApp.Models;
using GalleryApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GalleryApp.Controllers;
public class HomeController : Controller {
    private readonly IStorageService storage;
    private readonly ILogger<HomeController> logger;
    private BlobContainerClient container;

    public HomeController(IStorageService storage, ILogger<HomeController> logger) {
        this.storage = storage;
        this.logger = logger;
	}

	public async Task<IActionResult> Index() {
        var images = await storage.GetFiles();
		return View(images);
	}

	[HttpPost]
    public async Task<IActionResult> Index([FromForm] IFormCollection formData) {
        var allowedContentTypes = new List<string> { "image/png", "image/jpeg" };

        foreach(var file in formData.Files) {
            if(!allowedContentTypes.Contains(file.ContentType))
                continue;
            await storage.Upload(file);
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
