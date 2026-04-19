using AsyncProcessTasksAsTheyComplete.Hubs;
using AsyncProcessTasksAsTheyComplete.Services;
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

builder.Services.AddScoped<ILearnMicrosoftService, LearnMicrosoftService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseStaticFiles();
app.MapHub<NotificationHub>("/notificationHub");


var learnMicrosoftApi = app.MapGroup("/api");
learnMicrosoftApi.MapGet("/sumpagesizes", async Task (ILearnMicrosoftService service) =>
{
    await service.SumPageSizesAsync();
})
.WithName("SumPageSizes");

app.Run();



[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
