using Microsoft.AspNetCore.Mvc;
using StockAnalyzer.Core.Domain;
using StockAnalyzer.Web.Models;
using System.Diagnostics;
using System.Text.Json;

namespace StockAnalyzer.Web.Controllers;

public class HomeController : Controller
{
    private static string API_URL = "https://ps-async.fekberg.com/api/stocks";
    private static readonly HttpClient _httpClient = new();

    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync($"{API_URL}/MSFT");

        var content = await response.Content.ReadAsStringAsync();

        // Simulate that the web call takes a very long time
        await Task.Delay(10000);

        var data = JsonSerializer.Deserialize<IEnumerable<StockPrice>>(content, JsonSerializerOptions.Web);

        return View(data);
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