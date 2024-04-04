using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace PXServer.Source.Database.Pixs;


[Table("prefab_abilities")]
public class PrefabAbility
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;
    public required string PrefabId { get; init; }

    public required string Name { get; init; }
    public required string Description { get; init; }

    public required int BasePower { get; init; }
    public required int BaseAccuracy { get; init; }

    public required string Type { get; init; }
    public required string Category { get; init; }
}