using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace PXServer.Source.Database.Crates;


public class CrateLootPrefab
{
    public required string PixPrefabId { get; init; }
    public required int Weight { get; init; }
    public required int Level { get; init; }
}



[Table("prefab_crates")]
public class CratePrefab
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;
    public required string PrefabId { get; init; }

    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<CrateLootPrefab> Loot { get; init; }
}