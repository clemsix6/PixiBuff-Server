using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Players;


namespace PXServer.Source.Managers;


public class InventoryManager : Manager
{
    private readonly CrateManager crateManager;
    private readonly PixManager pixManager;


    public InventoryManager(MongoDbContext database, CrateManager crateManager, PixManager pixManager) : base(database)
    {
        this.crateManager = crateManager;
        this.pixManager = pixManager;
    }


    public async Task<PublicInventory> GetInventory(Player player)
    {
        var crates = await this.crateManager.GetPublicCrates(player);
        var pixes = await this.pixManager.GetPublicPixes(player);

        return new PublicInventory
        {
            Crates = crates,
            Pixes = pixes
        };
    }
}