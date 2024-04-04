using MongoDB.Driver;
using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Pixs;
using PXServer.Source.Database.Players;


namespace PXServer.Source.Managers;


public class PixManager : Manager
{
    public PixManager(MongoDbContext database) : base(database)
    {
    }


    private async Task<List<PrefabAbility>> GetStartingAbilities(PixPrefab prefab)
    {
        var abilities = new List<PrefabAbility>();
        foreach (var abilityId in prefab.StartingAbilities) {
            var ability = await this.Database.AbilityPrefabs.Find(x => x.PrefabId == abilityId).FirstOrDefaultAsync();
            if (ability == null)
                throw new Exception($"Ability {abilityId} not found");
            abilities.Add(ability);
        }
        return abilities;
    }


    public async Task<RuntimePix> CreatePix(Player player, PixPrefab prefab, int level)
    {
        var pix = new RuntimePix
        {
            PlayerId = player.Id,
            Prefab = prefab,
            Level = level,
            Abilities = await GetStartingAbilities(prefab),
        };
        pix.UpdateStats();

        await this.Database.RuntimePixes.InsertOneAsync(pix);
        return pix;
    }


    public async Task<List<PublicPix>> GetPublicPixes(Player player)
    {
        var pixes = await this.Database.RuntimePixes.Find(p => p.PlayerId == player.Id).ToListAsync();
        return pixes.Select(x => x.GetPublicPix()).ToList();
    }
}