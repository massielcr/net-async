using AsyncCancelListOfTasks.Hubs;
using AsyncCancelListOfTasks.Services;
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

cancelTaskApi.MapGet("/start", async (IMicrosoftService microsoftService, CancellationToken cancellationToken) =>
{
    try
    {
        await microsoftService.SumPageSizesAsync(cancellationToken);
        return Results.Ok();
    }
    catch (OperationCanceledException)
    {
        // Return 499 (Client Closed Request) or just a 204 No Content
        return Results.StatusCode(499);
    }
})
.WithName("SumPageSizesAsync");



app.Run();



[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
