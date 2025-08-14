using Infrastructure.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Persistence;

public sealed class MongoClientFactory
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _db;
    private readonly HashSet<string> _allowed;

    public MongoClientFactory(IOptions<MongoOptions> options)
    {
        var o = options.Value;
        _client = new MongoClient(o.ConnectionString);
        _db = _client.GetDatabase(o.Database);
        _allowed = o.GetAllowed();
    }

    public IMongoDatabase GetDatabase() => _db;

    public IMongoCollection<MongoDB.Bson.BsonDocument> GetCollection(string name)
    {
        if (!_allowed.Contains(name))
            throw new InvalidOperationException($"Collection '{name}' is not allowed.");
        return _db.GetCollection<MongoDB.Bson.BsonDocument>(name);
    }

    public HashSet<string> AllowedCollections => _allowed;
}