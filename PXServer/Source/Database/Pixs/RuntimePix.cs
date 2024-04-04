using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PXResources.Shared.Resources;


namespace PXServer.Source.Database.Pixs;


[Table("runtime_inventory_pixs")]
public class RuntimePix
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string PlayerId { get; init; }
    public required PixPrefab Prefab { get; init; }
    public required int Level { get; init; }
    public required List<PrefabAbility> Abilities { get; init; }
    public int Experience { get; init; } = 0;

    public PublicPixStats IvStats { get; init; } = new(64);
    public PublicPixStats Stats { get; init; } = new();

    public int Total { get; set; }
    public int Cp { get; set; }
    public int Gold { get; set; }


    public void UpdateStats()
    {
        this.Stats.Hp = (2 * this.Prefab.BaseStats.Hp + this.IvStats.Hp) * (this.Level + 5) / 100 + this.Level + 5;
        this.Stats.Attack = (2 * this.Prefab.BaseStats.Attack + this.IvStats.Attack) * (this.Level + 5) / 100 + 2;
        this.Stats.Defense = (2 * this.Prefab.BaseStats.Defense + this.IvStats.Defense) * (this.Level + 5) / 100 + 2;
        this.Stats.Speed = (2 * this.Prefab.BaseStats.Speed + this.IvStats.Speed) * (this.Level + 5) / 100 + 2;
        this.Total = this.Stats.Hp + this.Stats.Attack + this.Stats.Defense + this.Stats.Speed;

        this.Cp = (int)Math.Pow(
                      Math.Pow(this.Stats.Attack, 0.65) *
                      Math.Pow(this.Stats.Defense, 0.6) *
                      Math.Pow(this.Stats.Hp, 0.6) *
                      Math.Pow(this.Stats.Speed, 0.3) *
                      Math.Pow(this.Level, 0.1) *
                      (1 + Math.Pow(this.Abilities.Sum(x => x.BasePower), 0.5)), 0.75
                  ) /
                  10;
        this.Gold = (int)Math.Pow((float)this.Cp / 10 + 1, 2);
    }


    public PublicPix GetPublicPix()
    {
        return new PublicPix
        {
            Id = this.Id,
            PrefabId = this.Prefab.PrefabId,

            Name = this.Prefab.Name,
            Description = this.Prefab.Description,

            Types = this.Prefab.Types,
            Abilities = this.Abilities.Select(x => x.PrefabId).ToList(),

            Level = this.Level,
            Experience = this.Experience,

            Stats = this.Stats,
            Total = this.Total,
            Cp = this.Cp,
            Gold = this.Gold
        };
    }
}