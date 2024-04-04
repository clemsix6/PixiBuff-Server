namespace PXResources.Shared.Resources;


[Serializable]
public class PublicCrateLoot
{
    public required string PixPrefabId { get; init; }
    public required int Level { get; init; }
    public required float Chance { get; init; }
}



[Serializable]
public class PublicCrate
{
    public required string Id { get; init; }

    public required string Name { get; init; }
    public required string Description { get; init; }
    public required DateTime AddedAt { get; init; }

    public required List<PublicCrateLoot> Loot { get; init; }
}