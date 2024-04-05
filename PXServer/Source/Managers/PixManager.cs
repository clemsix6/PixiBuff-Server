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

        // Get the starting abilities
        foreach (var abilityId in prefab.StartingAbilities) {
            var ability = await this.Database.AbilityPrefabs.Find(x => x.PrefabId == abilityId).FirstOrDefaultAsync();
            // Check if the ability exists in the database
            if (ability == null)
                throw new Exception($"Ability {abilityId} not found");
            // Add the ability to the list
            abilities.Add(ability);
        }
        return abilities;
    }


    public async Task<RuntimePix> CreatePix(RuntimePlayer runtimePlayer, PixPrefab prefab, int level)
    {
        // Check if the player exists in the database
        if (this.Database.RuntimePlayers.Find(x => x.Id == runtimePlayer.Id).FirstOrDefault() == null)
            throw new Exception("Player not found");
        // Check if the pix prefab exists in the database
        if (this.Database.PixPrefabs.Find(x => x.PrefabId == prefab.PrefabId).FirstOrDefault() == null)
            throw new Exception("PixPrefab not found");
        // Check if the level is valid
        if (level is <= 0 or > 100)
            throw new Exception("Invalid level");

        // Create the pix
        var pix = new RuntimePix
        {
            PlayerId = runtimePlayer.Id,
            Prefab = prefab,
            Level = level,
            Abilities = await GetStartingAbilities(prefab),
        };
        // Update the pix stats
        pix.UpdateStats();

        // Insert the pix into the database
        await this.Database.RuntimePixes.InsertOneAsync(pix);
        return pix;
    }


    public async Task<List<PublicPix>> GetPublicPixes(RuntimePlayer runtimePlayer)
    {
        // Get the pixes from the database
        var pixes = await this.Database.RuntimePixes.Find(p => p.PlayerId == runtimePlayer.Id).ToListAsync();
        // Return the public pixes
        return pixes.Select(x => x.GetPublicPix()).ToList();
    }
}