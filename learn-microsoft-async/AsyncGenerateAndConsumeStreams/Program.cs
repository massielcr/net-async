using AsyncGenerateAndConsumeStreams.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octokit;

// 1. Initialize the Host Builder
Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// 2. Register your services for DI
builder.Services.AddSingleton<IGitHubService, GitHubService>();

// 3. Build the host
using IHost host = builder.Build();

// 4. Resolve the service
IGitHubService gitHubService = host.Services.GetRequiredService<IGitHubService>();


CancellationTokenSource cancellationSource = new();

#region Before Refactoring

//IProgress<int> progressReporter = new Progress<int>((num) =>
//{
//    Console.WriteLine($"Received {num} issues in total");
//});

//try
//{
//    var results = await gitHubService.RunPagedQueryBeforeRefactoringAsync("docs", cancellationSource.Token, progressReporter);

//    Console.WriteLine();
//    Console.WriteLine(" Results: ");
//    Console.WriteLine();

//    foreach (var issue in results)
//        Console.WriteLine(issue);
//}
//catch (OperationCanceledException)
//{
//    Console.WriteLine("Work has been cancelled");
//}
//catch (Exception ex)
//{
//    Console.WriteLine(ex.Message);
//}

#endregion


#region Using async streams

int num = 0;
await foreach (var issue in gitHubService.RunPagedQueryAsync("docs")
    .WithCancellation(cancellationSource.Token))
{
    Console.WriteLine(issue);
    Console.WriteLine($"Received {++num} issues in total");
}

#endregion


Console.ReadLine();