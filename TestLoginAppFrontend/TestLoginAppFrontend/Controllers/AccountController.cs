using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TestLoginAppFrontend.Dtos;
using TestLoginAppFrontend.Models;

namespace TestLoginAppFrontend.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("users/sign-in", new LoginDto
        {
            Username = model.Username,
            Password = model.Password,
            IPs = model.IPs
        });

        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(errorJson);

            string errorMsg = errorObj != null && errorObj.TryGetValue("error", out var msg) ? msg : "Login failed: unexpected error";

            ModelState.AddModelError(string.Empty, errorMsg);
            return View(model);
        }

        var result = await response.Content.ReadFromJsonAsync<SignInResponse>();

        if (result is not null)
        {
            TempData["UserData"] = JsonSerializer.Serialize(result);
            return RedirectToAction("Index", "Home");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Login failed: unexpected error");
            return View(model);
        }
    }

    public IActionResult Index()
    {
        return View();
    }
}

public class SignInResponse
{
    public int EntityId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EmailConfirm { get; set; }
    public int MobileConfirm { get; set; }
    public int CountryID { get; set; }
    public int Status { get; set; }
    public string lid { get; set; } = string.Empty;
    public string FTPHost { get; set; } = string.Empty;
    public int FTPPort { get; set; }
}
