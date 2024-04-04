namespace PXResources.Shared.Resources;


[Serializable]
public class Inventory
{
    public List<Crate> Crates { get; init; }


    public Inventory(List<Crate> crates)
    {
        this.Crates = crates;
    }
}