using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

await Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration))
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<MongoOptions>(ctx.Configuration.GetSection("Mongo"));
        services.Configure<ApiOptions>(ctx.Configuration.GetSection("Api"));
        services.AddSingleton<IMongoClient>(sp => new MongoClient(sp.GetRequiredService<IOptions<MongoOptions>>().Value.ConnectionString));
        services.AddHostedService<WatcherService>();
        services.AddHttpClient("api");
    })
    .RunConsoleAsync();

public sealed class MongoOptions { public string ConnectionString { get; init; } = default!; public string Database { get; init; } = default!; public string[] Collections { get; init; } = Array.Empty<string>(); }
public sealed class ApiOptions { public string BaseUrl { get; init; } = default!; public string InternalKey { get; init; } = default!; }

public sealed class WatcherService : BackgroundService
{
    private readonly ILogger _log = Log.ForContext<WatcherService>();
    private readonly IMongoClient _client;
    private readonly MongoOptions _mongo;
    private readonly ApiOptions _api;
    private readonly IHttpClientFactory _httpFactory;

    public WatcherService(IMongoClient client, IOptions<MongoOptions> mongo, IOptions<ApiOptions> api, IHttpClientFactory httpFactory)
    {
        _client = client;
        _mongo = mongo.Value;
        _api = api.Value;
        _httpFactory = httpFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _client.GetDatabase(_mongo.Database);
        var tasks = _mongo.Collections.Select(c => WatchCollection(db, c, stoppingToken)).ToArray();
        await Task.WhenAll(tasks);
    }

    private async Task WatchCollection(IMongoDatabase db, string collection, CancellationToken ct)
    {
        var col = db.GetCollection<BsonDocument>(collection);
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
            .Match(x => x.OperationType == ChangeStreamOperationType.Insert
                     || x.OperationType == ChangeStreamOperationType.Update
                     || x.OperationType == ChangeStreamOperationType.Replace
                     || x.OperationType == ChangeStreamOperationType.Delete);

        using var cursor = await col.WatchAsync(pipeline, cancellationToken: ct);
        _log.Information("Watching {Collection}", collection);

        var http = _httpFactory.CreateClient("api");
        http.BaseAddress = new Uri(_api.BaseUrl);
        var token = await GetInternalTokenAsync(http, ct);

        while (await cursor.MoveNextAsync(ct))
        {
            foreach (var change in cursor.Current)
            {
                var msg = BuildMessage(collection, change);
                var content = new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json");
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = await http.PostAsync("/api/internal/broadcast", content, ct);
                if (!res.IsSuccessStatusCode)
                    _log.Warning("Broadcast failed: {Status} {Reason}", (int)res.StatusCode, res.ReasonPhrase);
            }
        }
    }

    private static object BuildMessage(string collection, ChangeStreamDocument<BsonDocument> change)
    {
        string op = change.OperationType.ToString().ToLowerInvariant();
        string id = change.DocumentKey.GetValue("_id", BsonNull.Value) switch
        {
            var v when v.IsObjectId => v.AsObjectId.ToString(),
            var v when v != BsonNull.Value => v.ToString(),
            _ => string.Empty
        };

        object? payload = null;
        if (change.FullDocument is not null)
        {
            var clone = new BsonDocument(change.FullDocument);
            if (clone.TryGetValue("_id", out var idVal))
                clone["_id"] = idVal.IsObjectId ? idVal.AsObjectId.ToString() : idVal.ToString();
            payload = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<Dictionary<string, object>>(clone);
        }

        return new
        {
            Collection = collection,
            Operation = op,
            Id = id,
            Timestamp = DateTime.UtcNow,
            Payload = payload
        };
    }

    private async Task<string> GetInternalTokenAsync(HttpClient http, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/service-token");
        req.Headers.Add("x-internal-key", _api.InternalKey);
        var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        using var s = await res.Content.ReadAsStreamAsync(ct);
        var dto = await JsonSerializer.DeserializeAsync<TokenDto>(s, cancellationToken: ct);
        return dto!.access_token;
    }

    private sealed class TokenDto { public string access_token { get; init; } = default!; }
}
