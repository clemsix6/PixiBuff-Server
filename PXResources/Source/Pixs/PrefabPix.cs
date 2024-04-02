using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace PXResources.Source.Pixs;


[Table("prefab_pixes")]
public class PrefabPix
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string Name { get; init; }
    public required string Description { get; init; }

    public required int BaseHp { get; init; }
    public required int BaseAtk { get; init; }
    public required int BaseDef { get; init; }

    public required ICollection<string> Types { get; init; }
    public required ICollection<PrefabStartingAbility> StartingAbilities { get; init; }
}
