using System.Net;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Model;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();


// this will trigger the dbInitializer after the application is fully running.
// otherwise the builder will keep waiting in the try block and application will not run if db not initialized
app.Lifetime.ApplicationStarted.Register(async () => 
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        
        System.Console.WriteLine(e);
    }
});

// try
// {
//     await DbInitializer.InitDb(app);
// }
// catch (Exception e)
// {
    
//     System.Console.WriteLine(e);
// }

app.Run();

// called when search service donot find auction service for the http request to get the data
// Do something if we get an exception
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
