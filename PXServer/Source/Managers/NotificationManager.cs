using MongoDB.Driver;
using PXServer.Source.Database;
using PXServer.Source.Database.Notifications;
using PXServer.Source.Database.Players;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Managers;


public class NotificationManager : Manager
{
    public NotificationManager(MongoDbContext database) : base(database)
    {
    }


    public async Task SendNotification(RuntimePlayer runtimePlayer, string title, string message, NotificationType type)
    {
        // Check if the player exists in the database
        if (this.Database.RuntimePlayers.Find(x => x.Id == runtimePlayer.Id).FirstOrDefault() == null)
            throw new ServerException("Player not found", StatusCodes.Status404NotFound);

        // Create the notification
        var notification = new RuntimeNotification
        {
            PlayerId = runtimePlayer.Id,
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = DateTime.Now
        };
        // Insert the notification into the database
        await this.Database.Notifications.InsertOneAsync(notification);
    }


    public async Task<List<RuntimeNotification>> GetNotifications(RuntimePlayer runtimePlayer)
    {
        // Get the notifications from the database
        return await this.Database.Notifications.Find(n => n.PlayerId == runtimePlayer.Id).ToListAsync();
    }


    public async Task DeleteNotification(RuntimePlayer runtimePlayer, string notificationId)
    {
        var notification = await this.Database.Notifications.Find(n => n.Id == notificationId).FirstOrDefaultAsync();
        if (notification == null)
            throw new ServerException("Notification not found", StatusCodes.Status404NotFound);
        if (notification.PlayerId != runtimePlayer.Id)
            throw new ServerException("Notification not found", StatusCodes.Status404NotFound);
        await this.Database.Notifications.DeleteOneAsync(n => n.Id == notificationId);
    }


    public async Task ClearNotifications(RuntimePlayer runtimePlayer)
    {
        await this.Database.Notifications.DeleteManyAsync(n => n.PlayerId == runtimePlayer.Id);
    }
}