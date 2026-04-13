using AsyncProgrammingScenarios.Models;
using AsyncProgrammingScenarios.Services;
using AsyncProgrammingScenarios.Services.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace AsyncProgrammingScenarios.Controllers
{
    public class HomeController(IHubContext<LoggerHub> hubContext, IDotNetAPIService dotNetService, IUserDBService userService, ICalculateCPU calculateCPU) : Controller
    {
        private readonly IHubContext<LoggerHub> _hubContext = hubContext;

        private readonly IDotNetAPIService _dotNetService = dotNetService;
        private readonly IUserDBService _userService = userService;        
        private readonly ICalculateCPU _calculateCPU = calculateCPU;

        private static readonly IEnumerable<string> _urlList =
        [
            "https://learn.microsoft.com",
            "https://learn.microsoft.com/aspnet/core",
            "https://learn.microsoft.com/azure",
            "https://learn.microsoft.com/azure/devops",
            "https://learn.microsoft.com/dotnet",
            "https://learn.microsoft.com/dotnet/desktop/wpf/get-started/create-app-visual-studio",
            "https://learn.microsoft.com/education",
            "https://learn.microsoft.com/shows/net-core-101/what-is-net",
            "https://learn.microsoft.com/enterprise-mobility-security",
            "https://learn.microsoft.com/gaming",
            "https://learn.microsoft.com/graph",
            "https://learn.microsoft.com/microsoft-365",
            "https://learn.microsoft.com/office",
            "https://learn.microsoft.com/powershell",
            "https://learn.microsoft.com/sql",
            "https://learn.microsoft.com/surface",
            "https://dotnetfoundation.org",
            "https://learn.microsoft.com/visualstudio",
            "https://learn.microsoft.com/windows"
        ];

        public IActionResult Index()
        {
            return View();
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



        [HttpGet, Route("DotNetCountAPI")]
        public async Task<IActionResult> GetDotNetCountAsync(string URL)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLog", "DotNetCount API started.");


            await _hubContext.Clients.All.SendAsync("ReceiveLog", "Counting '.NET' phrase in websites...");

            int total = 0;
            foreach (string url in _urlList)
            {
                var result = await _dotNetService.GetDotNetCountAsync(url);
                await _hubContext.Clients.All.SendAsync("ReceiveLog", $"{url}: {result}");
                total += result;
            }

            await _hubContext.Clients.All.SendAsync("ReceiveLog", "Total: " + total);


            await _hubContext.Clients.All.SendAsync("ReceiveLog", "DotNetCount API ending.");

            return Ok();
        }


        [HttpGet, Route("UsersDB")]
        public async Task<IActionResult> GetUsersAsync(string URL)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLog", "Users DB started.");


            await _hubContext.Clients.All.SendAsync("ReceiveLog", "Retrieving User objects with list of IDs...");

            IEnumerable<int> ids = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var users = await _userService.GetUsersAsync(ids);
            foreach (User? user in users)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveLog", $"{user.id}: isEnabled={user.isEnabled}");
            }

            await _hubContext.Clients.All.SendAsync("ReceiveLog", "Processing tasks as they complete...");

            await ProcessTasksAsTheyCompleteAsync(ids);


            await _hubContext.Clients.All.SendAsync("ReceiveLog", "Users DB ending.");

            return Ok();
        }

        private async Task ProcessTasksAsTheyCompleteAsync(IEnumerable<int> userIds)
        {
            var getUserTasks = userIds.Select(id => _userService.GetUserAsync(id)).ToList();

            while (getUserTasks.Count > 0)
            {
                Task<User> completedTask = await Task.WhenAny(getUserTasks);
                getUserTasks.Remove(completedTask);

                User user = await completedTask;
                Console.WriteLine($"Processed user {user.id}");
            }
        }


        [HttpGet, Route("CalculateCPU")]
        public async Task<IActionResult> GetCalculateCPUAsync(string URL)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLog", "Calculate CPU started.");


            var damageResult = await Task.Run(() => _calculateCPU.CalculateDamageDone(1000));

            await _hubContext.Clients.All.SendAsync("ReceiveLog", damageResult.Damage);


            await _hubContext.Clients.All.SendAsync("ReceiveLog", "Calculate CPU ending.");

            return Ok();
        }        
    }
}
