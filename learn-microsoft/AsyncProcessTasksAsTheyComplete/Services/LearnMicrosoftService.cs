using AsyncProcessTasksAsTheyComplete.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace AsyncProcessTasksAsTheyComplete.Services
{
    public class LearnMicrosoftService(IHubContext<NotificationHub> hubContext) : ILearnMicrosoftService
    {
        static readonly HttpClient _client = new()
        {
            MaxResponseContentBufferSize = 1_000_000
        };

        static readonly IEnumerable<string> _urlList =
        [
            "https://learn.microsoft.com",
            "https://learn.microsoft.com/aspnet/core",
            "https://learn.microsoft.com/azure",
            "https://learn.microsoft.com/azure/devops",
            "https://learn.microsoft.com/dotnet",
            "https://learn.microsoft.com/dynamics365",
            "https://learn.microsoft.com/education",
            "https://learn.microsoft.com/enterprise-mobility-security",
            "https://learn.microsoft.com/gaming",
            "https://learn.microsoft.com/graph",
            "https://learn.microsoft.com/microsoft-365",
            "https://learn.microsoft.com/office",
            "https://learn.microsoft.com/powershell",
            "https://learn.microsoft.com/sql",
            "https://learn.microsoft.com/surface",
            "https://learn.microsoft.com/system-center",
            "https://learn.microsoft.com/visualstudio",
            "https://learn.microsoft.com/windows"
        ];

        private readonly IHubContext<NotificationHub> _hubContext = hubContext;

        public async Task SumPageSizesAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                IEnumerable<Task<int>> downloadTasksQuery =
                    from url in _urlList
                    select ProcessUrlAsync(url);

                List<Task<int>> downloadTasks = [.. downloadTasksQuery];

                int total = 0;
                await foreach (Task<int> task in Task.WhenEach(downloadTasks))
                {
                    total += await task;
                }

                stopwatch.Stop();

                await _hubContext.Clients.All.SendAsync("notify", $"Total bytes returned: {total:#,#}");
                await _hubContext.Clients.All.SendAsync("notify", $"Elapsed time: {stopwatch.Elapsed}");
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.All.SendAsync("notify", $"Error: {ex.Message}");
            }           
        }

        private async Task<int> ProcessUrlAsync(string url)
        {
            byte[] content = await _client.GetByteArrayAsync(url);

            await _hubContext.Clients.All.SendAsync("notify", $"{url,-60} {content.Length,10:#,#}");

            return content.Length;
        }
    }
}
