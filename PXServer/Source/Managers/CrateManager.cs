using MongoDB.Driver;
using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Crates;
using PXServer.Source.Database.Pixs;
using PXServer.Source.Database.Players;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Managers;


public class CrateManager : Manager
{
    private readonly PixManager pixManager;


    public CrateManager(MongoDbContext database, PixManager pixManager) : base(database)
    {
        this.pixManager = pixManager;
    }


    public async Task AddCrate(RuntimePlayer runtimePlayer, CratePrefab cratePrefab, int amount)
    {
        // Check if the player exists in the database
        if (this.Database.RuntimePlayers.Find(x => x.Id == runtimePlayer.Id).FirstOrDefault() == null)
            throw new ServerException("Player not found", StatusCodes.Status404NotFound);
        // Check if the crate prefab exists in the database
        if (this.Database.CratePrefabs.Find(x => x.PrefabId == cratePrefab.PrefabId).FirstOrDefault() == null)
            throw new ServerException("CratePrefab not found", StatusCodes.Status404NotFound);
        // Check if the amount is valid
        if (amount is <= 0 or > 50)
            throw new ServerException("Invalid amount", StatusCodes.Status400BadRequest);

        // Add the crates to the database
        for (var i = 0; i < amount; i++) {
            var crate = new RuntimeCrate
            {
                PlayerId = runtimePlayer.Id,
                Prefab = cratePrefab,
                AddedAt = DateTime.Now
            };
            // Insert the crate into the database
            await this.Database.RuntimeCrates.InsertOneAsync(crate);
        }
    }


    public async Task<List<RuntimeCrate>> GetCrates(RuntimePlayer runtimePlayer)
    {
        // Get the crates from the database
        return await this.Database.RuntimeCrates.Find(c => c.PlayerId == runtimePlayer.Id).ToListAsync();
    }


    public async Task<List<PublicCrate>> GetPublicCrates(RuntimePlayer runtimePlayer)
    {
        // Get the public crates
        var crates = await GetCrates(runtimePlayer);
        return crates.Select(x => x.GetPublicCrate()).ToList();
    }


    public async Task<RuntimePix> OpenCrate(RuntimePlayer runtimePlayer, string crateId)
    {
        // Check if the player exists in the database
        if (this.Database.RuntimePlayers.Find(x => x.Id == runtimePlayer.Id).FirstOrDefault() == null)
            throw new ServerException("Player not found", StatusCodes.Status404NotFound);

        // Get the crate from the database
        var crate = await this.Database.RuntimeCrates.Find(c => c.PlayerId == runtimePlayer.Id && c.Id == crateId)
            .FirstOrDefaultAsync();
        // Check if the crate exists
        if (crate == null)
            throw new ServerException("Crate not found", StatusCodes.Status404NotFound);

        // Open the crate
        var loot = crate.Open();
        // Get the pix prefab from the database
        var pixPrefab = await this.Database.PixPrefabs.Find(p => p.PrefabId == loot.PixPrefabId).FirstOrDefaultAsync();
        // Check if the pix prefab exists
        if (pixPrefab == null)
            throw new ServerException("PixPrefab not found", StatusCodes.Status404NotFound);

        // Create the pix
        var pix = await this.pixManager.CreatePix(runtimePlayer, pixPrefab, loot.Level);
        // Delete the crate from the database
        await this.Database.RuntimeCrates.DeleteOneAsync(c => c.Id == crateId);
        // Return the pix
        return pix;
    }
}