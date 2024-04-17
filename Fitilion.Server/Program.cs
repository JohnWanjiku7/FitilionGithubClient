using Fitilion.Server.Models;
using Fitilion.Server.Persistense;
using Fitilion.Server.Services.Interfaces;
using Fitilion.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Fitilion.Server.Persistense.Interface;
using Fitilion.Server.Persistense.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Load AppSettings and configure named client
builder.Services.AddOptions<GitHubSettings>()
    .Bind(builder.Configuration.GetSection("GitHubSettings"))
    .ValidateDataAnnotations();

// Add and configure http clients
builder.Services.AddHttpClient("githubClient", (serviceProvider, httpClient) =>
{
    var gitHubSettings = serviceProvider.GetRequiredService<IOptions<GitHubSettings>>().Value;
    httpClient.DefaultRequestHeaders.Add("Authorization", gitHubSettings.AccessToken);
    httpClient.DefaultRequestHeaders.Add("APIVersion", gitHubSettings.APIVersion);
    httpClient.DefaultRequestHeaders.Add("User-Agent", "CommitsReader");
    httpClient.BaseAddress = new Uri(gitHubSettings.GitBaseHubUrl);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    };
})
.SetHandlerLifetime(TimeSpan.FromMinutes(20));

// Add scoped DbContext and configure SQL Server
builder.Services.AddDbContext<GitHubCommitsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IGitHubCommitRepository, GitHubCommitRepository>();

// Configure singleton GitHub commits service
builder.Services.AddScoped<IGitHubCommitsService, GitHubCommitsService>();

// Add Caching
builder.Services.AddMemoryCache();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
