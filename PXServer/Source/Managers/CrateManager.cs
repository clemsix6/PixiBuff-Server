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


    public async Task AddCrate(Player player, CratePrefab cratePrefab, int amount)
    {
        for (var i = 0; i < amount; i++) {
            var crate = new RuntimeCrate
            {
                PlayerId = player.Id,
                Prefab = cratePrefab,
                AddedAt = DateTime.Now
            };
            await this.Database.RuntimeCrates.InsertOneAsync(crate);
        }
    }


    public async Task<List<RuntimeCrate>> GetCrates(Player player)
    {
        return await this.Database.RuntimeCrates.Find(c => c.PlayerId == player.Id).ToListAsync();
    }


    public async Task<List<PublicCrate>> GetPublicCrates(Player player)
    {
        var crates = await GetCrates(player);
        return crates.Select(x => x.GetPublicCrate()).ToList();
    }


    public async Task<RuntimePix> OpenCrate(Player player, string crateId)
    {
        var crate = await this.Database.RuntimeCrates.Find(c => c.PlayerId == player.Id && c.Id == crateId)
            .FirstOrDefaultAsync();
        if (crate == null)
            throw new ServerException("Crate not found", StatusCodes.Status404NotFound);
        var loot = crate.Open();
        var pixPrefab = await this.Database.PixPrefabs.Find(p => p.PrefabId == loot.PixPrefabId).FirstOrDefaultAsync();
        if (pixPrefab == null)
            throw new ServerException("PixPrefab not found", StatusCodes.Status404NotFound);
        var pix = await this.pixManager.CreatePix(player, pixPrefab, loot.Level);
        await this.Database.RuntimeCrates.DeleteOneAsync(c => c.Id == crateId);
        return pix;
    }
}