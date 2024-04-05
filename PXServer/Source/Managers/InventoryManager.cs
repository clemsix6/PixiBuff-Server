using MongoDB.Driver;
using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Players;
using PXServer.Source.Exceptions;


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


    public async Task<PublicInventory> GetInventory(RuntimePlayer runtimePlayer)
    {
        // Get the crates and pixes from the database
        var crates = await this.crateManager.GetPublicCrates(runtimePlayer);
        var pixes = await this.pixManager.GetPublicPixes(runtimePlayer);

        // Return the inventory
        return new PublicInventory
        {
            Crates = crates,
            Pixes = pixes
        };
    }
}