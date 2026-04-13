using AsyncProgrammingScenarios.Models;
using System.Text.RegularExpressions;

namespace AsyncProgrammingScenarios.Services
{
    public class DotNetAPIService : IDotNetAPIService
    {
        private static readonly HttpClient _httpClient = new();

        public async Task<int> GetDotNetCountAsync(string URL)
        {
            // Suspends GetDotNetCountAsync() to allow the caller (the web server)
            // to accept another request, rather than blocking on this one.
            var html = await _httpClient.GetStringAsync(URL);
            return Regex.Matches(html, @"\.NET").Count;
        }
    }
}
