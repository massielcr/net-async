using AsyncCancelListOfTasks.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace AsyncCancelListOfTasks.Services
{
    public class MicrosoftService(IHubContext<NotificationHub> hubContext) : IMicrosoftService
    {
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
        private static HttpClient _httpClient = new();

        private static readonly IEnumerable<string> _urlList =
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

        public async Task SumPageSizesAsync(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            int total = 0;
            foreach (string url in _urlList)
            {
                int contentLength = await ProcessUrlAsync(url, _httpClient, cancellationToken);
                total += contentLength;
            }

            stopwatch.Stop();

            await _hubContext.Clients.All.SendAsync("notify", $"\nTotal bytes returned:  {total:#,#}", cancellationToken: cancellationToken);
            await _hubContext.Clients.All.SendAsync("notify", $"Elapsed time: {stopwatch.Elapsed}\n", cancellationToken: cancellationToken);
        }

        private async Task<int> ProcessUrlAsync(string url, HttpClient httpClient, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            byte[] content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            await _hubContext.Clients.All.SendAsync("notify", $"{url,-60} {content.Length,10:#,#}", cancellationToken: cancellationToken);

            return content.Length;
        }
    }
}
