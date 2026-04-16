using AsyncFileAccess.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

var fileAccessApi = app.MapGroup("/api");

fileAccessApi.MapGet("/readtextfile", async (IFileService fileService) =>
{
    (bool success, string text) = await fileService.ReadTextFileAsync("myfiles/ReadTextFile.txt");

    if (success)
    {
        return Results.Ok(text);
    }
    else
    {
        return Results.Problem($"Error: {text}", statusCode: 400);
    }
})
.WithName("ReadTextFile");

fileAccessApi.MapGet("/readtextfilestream", async (IFileService fileService) =>
{
    (bool success, string text) = await fileService.ReadTextFileStreamAsync("myfiles/ReadTextFileStream.txt");

    if (success)
    {
        return Results.Ok(text);
    }
    else
    {
        return Results.Problem($"Error: {text}", statusCode: 400);
    }
})
.WithName("ReadTextFileStream");



fileAccessApi.MapGet("/writetextfile", async (IFileService fileService) =>
{
    await fileService.WriteTextFileAsync("myfiles/WriteTextFile.txt", "Hello, WriteTextFile World!");
})
.WithName("WriteTextFile");

fileAccessApi.MapGet("/writetextfilestream", async (IFileService fileService) =>
{
    await fileService.WriteTextFileAsync("myfiles/WriteTextFileStream.txt", "Hello, WriteTextFileStream World!");
})
.WithName("WriteTextFileStream");



fileAccessApi.MapGet("/writetextfileparallel", async (IFileService fileService) =>
{
    await fileService.WriteTextFileParallelAsync("myfiles/WriteTextFileParallel");
})
.WithName("WriteTextFileParallel");

fileAccessApi.MapGet("/writetextfilestreamparallel", async (IFileService fileService) =>
{
    await fileService.WriteTextFileStreamParallelAsync("myfiles/WriteTextFileStreamParallel");
})
.WithName("WriteTextFileStreamParallel");


app.Run();


[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
