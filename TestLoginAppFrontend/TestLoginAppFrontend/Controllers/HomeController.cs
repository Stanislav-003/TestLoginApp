using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using TestLoginAppFrontend.Models;

namespace TestLoginAppFrontend.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        SignInResponse? user = null;

        if (TempData["UserData"] is string userJson)
        {
            user = JsonSerializer.Deserialize<SignInResponse>(userJson);
        }

        return View(user);
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
