namespace DotnetFoundationAPI.Services
{
    public class DotnetFoundation : IDotnetFoundation
    {
        private readonly HttpClient _httpClient = new();
        private const string _baseAddress = "https://dotnetfoundation.org";

        public async Task<string> GetStringAsync()
        {
            return await _httpClient.GetStringAsync(_baseAddress);
        }
    }
}
