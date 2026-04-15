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

builder.Services.AddScoped<IAsyncTask, AsyncTask>();
builder.Services.AddScoped<IAsyncValueTask, AsyncValueTask>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHub<NotificationHub>("/notificationHub");
app.UseStaticFiles();


var asyncReturnTypesApi = app.MapGroup("/api");

asyncReturnTypesApi.MapGet("/task/", async Task (IHubContext<NotificationHub> hubContext, IAsyncTask _asyncTask) =>
{
    await hubContext.Clients.All.SendAsync("notify", "Sorry for the delay...");

    await _asyncTask.DisplayCurrentInfoAsync();

}).WithName("Task");

asyncReturnTypesApi.MapGet("/tasktresult", async Task<int> () =>
{
    return await AsyncTaskTResult.GetLeisureHoursAsync();
})
.WithName("TaskTResult");

asyncReturnTypesApi.MapGet("/void", async void () =>
{

})
.WithName("Void");

asyncReturnTypesApi.MapGet("/valuetask", async ValueTask<int> (IHubContext<NotificationHub> hubContext, IAsyncValueTask valueTask) =>
{
    await hubContext.Clients.All.SendAsync("notify", "Shaking dice...");

    int roll1 = await valueTask.RollAsync();
    int roll2 = await valueTask.RollAsync();

    return roll1 + roll2;
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
