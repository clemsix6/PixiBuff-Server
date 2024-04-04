using MongoDB.Driver;
using PXServer.Source.Database.Crates;
using PXServer.Source.Database.Notifications;
using PXServer.Source.Database.Pixs;
using PXServer.Source.Database.Players;


namespace PXServer.Source.Database;


public class MongoDbContext
{
    private IMongoDatabase Database { get; }


    public MongoDbContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");

        var client = new MongoClient(connectionString);
        this.Database = client.GetDatabase(databaseName);
    }


    public IMongoCollection<PixPrefab> PixPrefabs =>
        this.Database.GetCollection<PixPrefab>("prefab_pixes");

    public IMongoCollection<PrefabAbility> AbilityPrefabs =>
        this.Database.GetCollection<PrefabAbility>("prefab_abilities");

    public IMongoCollection<CratePrefab> CratePrefabs =>
        this.Database.GetCollection<CratePrefab>("prefab_crates");

    public IMongoCollection<Player> RuntimePlayers =>
        this.Database.GetCollection<Player>("runtime_players");

    public IMongoCollection<RuntimeCrate> RuntimeCrates =>
        this.Database.GetCollection<RuntimeCrate>("runtime_inventory_crates");

    public IMongoCollection<RuntimeNotification> Notifications =>
        this.Database.GetCollection<RuntimeNotification>("runtime_player_notifications");

    public IMongoCollection<RuntimePix> RuntimePixes =>
        this.Database.GetCollection<RuntimePix>("runtime_inventory_pixs");
}