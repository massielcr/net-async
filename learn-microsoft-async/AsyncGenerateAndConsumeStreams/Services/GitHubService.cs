using AsyncGenerateAndConsumeStreams.Requests;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Octokit;

namespace AsyncGenerateAndConsumeStreams.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;

        private const string PagedIssueQuery =
                            @"query ($repo_name: String!,  $start_cursor:String) {
                              repository(owner: ""dotnet"", name: $repo_name) {
                                issues(last: 25, before: $start_cursor)
                                 {
                                    totalCount
                                    pageInfo {
                                      hasPreviousPage
                                      startCursor
                                    }
                                    nodes {
                                      title
                                      number
                                      createdAt
                                    }
                                  }
                                }
                              }
                            ";

        public GitHubService(IConfiguration configuration)
        {
            var key = configuration["GitHubKey"]
                ?? throw new InvalidOperationException("You must store your GitHub key in User Secrets or an environment variable named 'GitHubKey'.");

            _client = new GitHubClient(new ProductHeaderValue("IssueQueryDemo"))
            {
                Credentials = new Credentials(key)
            };
        }


        public async Task<JArray> RunPagedQueryJObjectAsync(string repoName, CancellationToken cancel, IProgress<int> progress)
        {
            if (string.IsNullOrWhiteSpace(repoName))
            {
                throw new ArgumentException("You must provide a repo name", nameof(repoName));
            }

            var issueAndPRQuery = new GraphQLRequest
            {
                Query = PagedIssueQuery
            };

            issueAndPRQuery.Variables["repo_name"] = repoName;

            JArray finalResults = [];

            bool hasMorePages = true;
            int pagesReturned = 0;
            int issuesReturned = 0;

            // Stop with 10 pages, because these are large repos:
            while (hasMorePages && (pagesReturned++ < 10))
            {
                var postBody = issueAndPRQuery.ToJsonText();

                var response = await _client.Connection.Post<string>(new Uri("https://api.github.com/graphql"),
                                                                            postBody, 
                                                                            "application/json", 
                                                                            "application/json");

                JObject results = JObject.Parse(response.HttpResponse.Body.ToString()!);

                int totalCount = (int)issues(results)["totalCount"]!;
                hasMorePages = (bool)pageInfo(results)["hasPreviousPage"]!;

                issueAndPRQuery.Variables["start_cursor"] = pageInfo(results)["startCursor"]!.ToString();
                issuesReturned += issues(results)["nodes"]!.Count();
                finalResults.Merge(issues(results)["nodes"]!);

                progress?.Report(issuesReturned);
                cancel.ThrowIfCancellationRequested();
            }

            return finalResults;

            JObject issues(JObject result) => (JObject)result["data"]!["repository"]!["issues"]!;
            JObject pageInfo(JObject result) => (JObject)issues(result)["pageInfo"]!;
        }

    }
}
