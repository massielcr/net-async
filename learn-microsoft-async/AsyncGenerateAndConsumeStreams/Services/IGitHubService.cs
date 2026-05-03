using Newtonsoft.Json.Linq;

namespace AsyncGenerateAndConsumeStreams.Services
{
    internal interface IGitHubService
    {
        Task<JArray> RunPagedQueryJObjectAsync(string repoName, CancellationToken cancel, IProgress<int> progress);
    }
}
