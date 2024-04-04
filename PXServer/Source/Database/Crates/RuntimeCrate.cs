using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PXResources.Shared.Resources;
using PXResources.Source.Exceptions;


namespace PXServer.Source.Database.Crates;


[Table("runtime_inventory_crates")]
public class RuntimeCrate
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string PlayerId { get; init; }
    public required CratePrefab Prefab { get; init; }
    public required DateTime AddedAt { get; init; }


    public PublicCrate GetPublicCrate()
    {
        var totalWeight = this.Prefab.Loot.Sum(loot => loot.Weight);
        var loots = this.Prefab.Loot
            .Select(
                loot => new PublicCrateLoot
                {
                    PixPrefabId = loot.PixPrefabId,
                    Level = loot.Level,
                    Chance = (float)Math.Round((float)loot.Weight / totalWeight * 100, 1)
                }
            );

        return new PublicCrate
        {
            Id = this.Id,
            Name = this.Prefab.Name,
            Description = this.Prefab.Description,
            AddedAt = this.AddedAt,
            Loot = loots.ToList()
        };
    }


    public CrateLootPrefab Open()
    {
        var totalWeight = this.Prefab.Loot.Sum(loot => loot.Weight);
        var roll = Random.Shared.Next(totalWeight);

        foreach (var loot in this.Prefab.Loot) {
            roll -= loot.Weight;
            if (roll <= 0)
                return loot;
        }
        throw new CrateRollException("Failed to roll a loot item");
    }
}