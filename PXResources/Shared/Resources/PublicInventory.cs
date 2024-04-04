namespace PXResources.Shared.Resources;


[Serializable]
public class PublicInventory
{
    public required List<PublicCrate> Crates { get; init; }
    public required List<PublicPix> Pixes { get; init; }
}