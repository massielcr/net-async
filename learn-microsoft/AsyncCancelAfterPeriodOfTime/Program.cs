using AsyncCancelAfterPeriodOfTime.Hubs;
using AsyncCancelAfterPeriodOfTime.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });

builder.Services.AddScoped<IMicrosoftService, MicrosoftService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

app.MapHub<NotificationHub>("/notificationHub");



var cancelTaskApi = app.MapGroup("/api");
cancelTaskApi.MapGet("/start", async (IMicrosoftService microsoftService, IHubContext<NotificationHub> hubContext, CancellationToken requestCancellationToken) =>
{
    await hubContext.Clients.All.SendAsync("notify", "Application started.");

    IResult result;

    CancellationTokenSource timeOutCancellationToken = new CancellationTokenSource();
    timeOutCancellationToken.CancelAfter(2500);

    using var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken, timeOutCancellationToken.Token);   

    try
    {
        await microsoftService.SumPageSizesAsync(linkedCancellationToken.Token);

        await hubContext.Clients.All.SendAsync("notify", "Application ending.");

        result = Results.Ok();
    }
    catch (OperationCanceledException)
    {
        if (requestCancellationToken.IsCancellationRequested)
        {
            await hubContext.Clients.All.SendAsync("notify", "\nTasks cancelled by the user.\n");
        }

        if (timeOutCancellationToken.IsCancellationRequested)
        {
            await hubContext.Clients.All.SendAsync("notify", "\nTasks cancelled: timed out.\n");
        }
        
        // Return 499 (Client Closed Request) or just a 204 No Content          
        result = Results.StatusCode(499);
    }   

    return result;
})
.WithName("SumPageSizesAsync");


app.Run();


[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
