using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Players;


namespace PXServer.Source.Engine;


public class InventoryManager : Manager
{
    private readonly CrateManager crateManager;


    public InventoryManager(MongoDbContext database, CrateManager crateManager) : base(database)
    {
        this.crateManager = crateManager;
    }


    public async Task<Inventory> GetInventory(Player player)
    {
        var crates = await this.crateManager.GetInventoryCrates(player);
        return new Inventory(crates);
    }
}