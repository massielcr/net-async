using AsyncReturnTypes.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AsyncReturnTypes.Services
{
    public class AsyncTask(IHubContext<NotificationHub> hubContext) : IAsyncTask
    {
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
        public async Task DisplayCurrentInfoAsync()
        {
            await WaitAndApologizeAsync();

            await _hubContext.Clients.All.SendAsync("notify", $"Today is {DateTime.Now:D}");
            await _hubContext.Clients.All.SendAsync("notify", $"The current time is {DateTime.Now.TimeOfDay:t}");
            await _hubContext.Clients.All.SendAsync("notify", "The current temperature is 76 degrees.");
        }

        async Task WaitAndApologizeAsync()
        {
            await Task.Delay(2000);            
        }
    }
}
