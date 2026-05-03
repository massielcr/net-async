using Newtonsoft.Json.Linq;

namespace AsyncGenerateAndConsumeStreams.Services
{
    internal interface IGitHubService
    {
        Task<JArray> RunPagedQueryBeforeRefactoringAsync(string repoName, CancellationToken cancel, IProgress<int> progress);
    }
}
