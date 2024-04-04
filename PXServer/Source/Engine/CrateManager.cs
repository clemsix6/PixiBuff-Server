using MongoDB.Driver;
using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Crates;
using PXServer.Source.Database.Players;


namespace PXServer.Source.Engine;


public class CrateManager : Manager
{
    public CrateManager(MongoDbContext database) : base(database)
    {
    }


    public async Task AddCrate(Player player, CratePrefab cratePrefab, int amount)
    {
        for (var i = 0; i < amount; i++) {
            var crate = new RuntimeCrate
            {
                PlayerId = player.Id,
                CratePrefab = cratePrefab,
                AddedAt = DateTime.Now
            };
            await this.Database.RuntimeCrates.InsertOneAsync(crate);
        }
    }


    public async Task<List<RuntimeCrate>> GetCrates(Player player)
    {
        return await this.Database.RuntimeCrates.Find(c => c.PlayerId == player.Id).ToListAsync();
    }


    public async Task<List<Crate>> GetInventoryCrates(Player player)
    {
        var crates = await GetCrates(player);
        return crates.Select(x => x.GetInventoryCrate()).ToList();
    }
}