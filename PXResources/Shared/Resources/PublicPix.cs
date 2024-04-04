namespace PXResources.Shared.Resources;


public class PublicPix
{
    public required string Id { get; init; }
    public required string PrefabId { get; init; }

    public required string Name { get; init; }
    public required string Description { get; init; }

    public required List<string> Types { get; init; }
    public required List<string> Abilities { get; init; }

    public required int Level { get; init; }
    public required int Experience { get; init; }

    public required PublicPixStats Stats { get; init; }
    public required int Total { get; set; }
    public required int Cp { get; set; }
    public required int Gold { get; set; }
}