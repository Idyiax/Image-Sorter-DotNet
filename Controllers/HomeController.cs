using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Models;

namespace Image_Sorter_DotNet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult LibraryView()
    {
        return View();
    }

    public IActionResult SplitView()
    {
        return View();
    }

    public IActionResult AddImage()
    {
        return View();
    }

    public IActionResult ImageViewer(int id)
    {
        return View(id);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
