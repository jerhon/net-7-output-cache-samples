using Microsoft.AspNetCore.OutputCaching;

var builder = WebApplication.CreateBuilder(args);


var thirtySecondCachePolicy = "thirtySeconds";
var conditionalCache = "conditionalQueryString";
var taggedCache = "taggedCache";
var clearTaggedCache = "clearCache";

// Add services to the container.
builder.Services.AddOutputCache((opts) => {
    opts.AddPolicy(thirtySecondCachePolicy, (policy) => policy.Expire(TimeSpan.FromSeconds(30)));
    opts.AddPolicy(conditionalCache, (policy) => policy.With((ctx) => ctx.HttpContext.Request.Query["cache"] == "yes"));
    opts.AddPolicy(taggedCache, (policy) => policy.Tag("Tag"));
    opts.AddPolicy(clearTaggedCache, (policy) => policy.Tag("Tag").NoCache().Clear());
});

var app = builder.Build();

app.UseOutputCache();

var currentTime = () => TimeOnly.FromDateTime(DateTime.Now).ToLongTimeString();

// Return a cached time
app.MapGet("/time", currentTime)
    .CacheOutput(thirtySecondCachePolicy);

app.MapGet("/conditional_time", currentTime)
    .CacheOutput(conditionalCache);

app.MapGet("/person/{name}", (String name) => $"Hello, {name} @ {currentTime()}.")
    .CacheOutput();

app.MapGet("/tagged_cache", currentTime)
    .CacheOutput(taggedCache);

app.MapPost("/tagged_cache/clear", async (IOutputCacheStore cacheStorage, CancellationToken cancelToken) => {

    await cacheStorage.EvictByTagAsync("Tag", cancelToken);

    return $"Cleared Cache @ {currentTime()}";
});

app.Run();