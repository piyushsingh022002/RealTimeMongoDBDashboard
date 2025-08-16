using MongoDB.Bson;
using MongoDB.Driver;
using RealTimeMongoDashboard.Application.Interfaces;
using RealTimeMongoDashboard.Application.Models;

namespace RealTimeMongoDashboard.Infrastructure.Services;

public sealed class CollectionService : ICollectionService
{
    private readonly IMongoDatabase _db;
    private readonly IAllowedCollections _allowed;

    public CollectionService(IMongoDatabase db, IAllowedCollections allowed)
    {
        _db = db;
        _allowed = allowed;
    }

    public async Task<PagedResult<object>> GetPageAsync(string collection, int page, int pageSize, string? filterJson = null, string? sortJson = null, CancellationToken ct = default)
    {
        var col = GetCollection(collection);
        var filter = ParseOrEmpty(filterJson);
        var sort = string.IsNullOrWhiteSpace(sortJson) ? Builders<BsonDocument>.Sort.Descending("_id") : BsonDocument.Parse(sortJson);

        var find = col.Find(filter);
        if (sort is not null) find = find.Sort(sort);

        var total = await find.CountDocumentsAsync(ct).ConfigureAwait(false);
        var docs = await find.Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync(ct).ConfigureAwait(false);

        return new PagedResult<object>
        {
            Items = docs.ConvertAll(ToPlainObject),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<object?> GetByIdAsync(string collection, string id, CancellationToken ct = default)
    {
        var col = GetCollection(collection);
        var filter = IdFilter(id);
        var doc = await col.Find(filter).FirstOrDefaultAsync(ct).ConfigureAwait(false);
        return doc is null ? null : ToPlainObject(doc);
    }

    public async Task<string> InsertAsync(string collection, object document, CancellationToken ct = default)
    {
        var col = GetCollection(collection);
        var doc = ToBsonDocument(document);
        await col.InsertOneAsync(doc, cancellationToken: ct).ConfigureAwait(false);
        var id = doc.GetValue("_id", BsonNull.Value);
        return id == BsonNull.Value ? string.Empty : id.ToString();
    }

    public async Task<bool> UpdateAsync(string collection, string id, object document, CancellationToken ct = default)
    {
        var col = GetCollection(collection);
        var doc = ToBsonDocument(document);
        doc["_id"] = ParseId(id);
        var result = await col.ReplaceOneAsync(IdFilter(id), doc, cancellationToken: ct).ConfigureAwait(false);
        return result.MatchedCount == 1;
    }

    public async Task<bool> DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        var col = GetCollection(collection);
        var result = await col.DeleteOneAsync(IdFilter(id), ct).ConfigureAwait(false);
        return result.DeletedCount == 1;
    }

    // ---- helpers ----
    private IMongoCollection<BsonDocument> GetCollection(string collection)
    {
        if (!_allowed.IsAllowed(collection))
            throw new InvalidOperationException($"Collection '{collection}' is not allowed.");
        return _db.GetCollection<BsonDocument>(collection);
    }

    private static BsonDocument ParseOrEmpty(string? json)
        => string.IsNullOrWhiteSpace(json) ? Builders<BsonDocument>.Filter.Empty.Render(BsonDocumentSerializer.Instance, BsonSerializer.SerializerRegistry) : BsonDocument.Parse(json!);

    private static FilterDefinition<BsonDocument> IdFilter(string id)
        => Builders<BsonDocument>.Filter.Eq("_id", ParseId(id));

    private static BsonValue ParseId(string id)
        => ObjectId.TryParse(id, out var oid) ? (BsonValue)oid : id;

    private static BsonDocument ToBsonDocument(object obj)
    {
        return obj switch
        {
            BsonDocument b => b,
            string s => BsonDocument.Parse(s),
            System.Text.Json.JsonElement j => BsonDocument.Parse(j.GetRawText()),
            _ => obj.ToBsonDocument() // works for Dictionary<string, object> and POCOs
        };
    }

    private static object ToPlainObject(BsonDocument doc)
    {
        // return clean JSON-friendly object (id as string)
        var clone = new BsonDocument(doc);
        if (clone.TryGetValue("_id", out var idVal))
            clone["_id"] = idVal.IsObjectId ? idVal.AsObjectId.ToString() : idVal.ToString();
        return MongoDB.Bson.Serialization.BsonSerializer.Deserialize<Dictionary<string, object>>(clone);
    }
}
