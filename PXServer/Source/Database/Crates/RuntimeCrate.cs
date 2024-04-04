using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PXResources.Shared.Resources;


namespace PXServer.Source.Database.Crates;


[Table("runtime_inventory_crates")]
public class RuntimeCrate
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string PlayerId { get; init; }
    public required CratePrefab CratePrefab { get; init; }
    public required DateTime AddedAt { get; init; }


    public Crate GetInventoryCrate()
    {
        var totalWeight = this.CratePrefab.Loot.Sum(loot => loot.Weight);
        var loots = this.CratePrefab.Loot
            .Select(
                loot => new CrateLoot
                {
                    PixPrefabId = loot.PixPrefabId,
                    Level = loot.Level,
                    Chance = (float)Math.Round((float)loot.Weight / totalWeight * 100, 1)
                }
            );

        return new Crate
        {
            Id = this.Id,
            Name = this.CratePrefab.Name,
            Description = this.CratePrefab.Description,
            AddedAt = this.AddedAt,
            Loot = loots.ToList()
        };
    }
}