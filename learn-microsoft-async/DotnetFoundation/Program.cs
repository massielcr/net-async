using DotnetFoundationAPI.Services;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();



builder.Services.AddScoped<IDotnetFoundation, DotnetFoundation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


#region minimal-api

app.MapGet("/api/iobound", async (IDotnetFoundation client) =>
{
    var response = await client.GetStringAsync();
    var result = Regex.Matches(response, @"\.NET").Count;

    return Results.Ok(result);
})
.WithName("GetIObound");

app.MapGet("/api/cpubound", async (IDotnetFoundation client) =>
{
    var result = await Task.Run(() =>
    {
        int total = 0;
        var rnd = new Random();

        for (int i = 0; i < 1000; i++)
        {
            total += rnd.Next(1, 7);
        }

        return total;
    });

    return Results.Ok(result);
})
.WithName("GetCPUbound");

#endregion

app.Run();