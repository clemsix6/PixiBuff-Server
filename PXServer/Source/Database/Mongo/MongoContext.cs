using MongoDB.Driver;
using PXResources.Source.Pixs;
using PXResources.Source.Users;


namespace PXServer.Source.Database.Mongo;


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


    public IMongoCollection<PrefabPix> Pixes => this.Database.GetCollection<PrefabPix>("prefab_pixes");
    public IMongoCollection<PrefabAbility> Abilities => this.Database.GetCollection<PrefabAbility>("prefab_abilities");
    public IMongoCollection<User> Users => this.Database.GetCollection<User>("users");
}