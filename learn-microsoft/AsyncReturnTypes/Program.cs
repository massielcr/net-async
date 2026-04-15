using AsyncReturnTypes.Hubs;
using AsyncReturnTypes.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateSlimBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHub<NotificationHub>("/notificationHub");
app.UseStaticFiles();


var asyncReturnTypesApi = app.MapGroup("/api");

asyncReturnTypesApi.MapGet("/task/", async Task (IHubContext<NotificationHub> hubContext) =>
{
    return;

}).WithName("Task");

asyncReturnTypesApi.MapGet("/tasktresult", async Task<int> () =>
{
    return 100;
})
.WithName("TaskTResult");

asyncReturnTypesApi.MapGet("/void", async void () =>
{
    return;
})
.WithName("Void");

asyncReturnTypesApi.MapGet("/valuetask", async ValueTask<int> () =>
{
    return 500;
})
.WithName("ValueTaskTResult");

asyncReturnTypesApi.MapGet("/streams", async (IHubContext<NotificationHub> hubContext) =>
{
    await foreach (var word in AsyncEnumerableStreams.ReadWordsFromStreamAsync())
    {
        await hubContext.Clients.All.SendAsync("notify", word);
    }
})
.WithName("Streams");





app.Run();
