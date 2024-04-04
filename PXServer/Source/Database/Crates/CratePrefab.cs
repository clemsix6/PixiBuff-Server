using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PXResources.Source.Exceptions;


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


    public string Open()
    {
        var totalWeight = this.Loot.Sum(loot => loot.Weight);
        var roll = Random.Shared.Next(totalWeight);

        foreach (var loot in this.Loot) {
            roll -= loot.Weight;
            if (roll <= 0)
                return loot.PixPrefabId;
        }
        throw new CrateRollException("Failed to roll a loot item");
    }
}