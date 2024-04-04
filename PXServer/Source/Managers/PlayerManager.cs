using MongoDB.Driver;
using PXServer.Source.Database;
using PXServer.Source.Database.Notifications;
using PXServer.Source.Database.Players;


namespace PXServer.Source.Managers;


public class PlayerManager : Manager
{
    private readonly NotificationManager notificationManager;
    private readonly CrateManager crateManager;


    public PlayerManager(
        MongoDbContext database,
        NotificationManager notificationManager,
        CrateManager crateManager) :
        base(database)
    {
        this.notificationManager = notificationManager;
        this.crateManager = crateManager;
    }


    public async Task<Player> CreateUser(string name, string password)
    {
        var user = new Player
        {
            Name = name,
            Password = password
        };
        await this.Database.RuntimePlayers.InsertOneAsync(user);
        await OnUserCreated(user);
        return user;
    }


    private async Task OnUserCreated(Player player)
    {
        this.Logger.Info($"[+] {player.Name}");
        await this.notificationManager.SendNotification(
            player, "Welcome", "Welcome to the server", NotificationType.Info
        );
        var cratePrefab = this.Database.CratePrefabs.Find(c => c.Name == "Starter Crate").FirstOrDefault();
        await this.crateManager.AddCrate(player, cratePrefab, 5);
    }
}