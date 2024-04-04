using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PXResources.Shared.Resources;


namespace PXServer.Source.Database.Pixs;


[Table("prefab_pixes")]
public class PixPrefab
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;
    public required string PrefabId { get; init; }

    public required string Name { get; init; }
    public required string Description { get; init; }

    public required PublicPixStats BaseStats { get; init; }

    public required List<string> Types { get; init; }
    public required List<string> StartingAbilities { get; init; }
}