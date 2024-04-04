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


    public async Task SendNotification(Player player, string title, string message, NotificationType type)
    {
        var notification = new RuntimeNotification
        {
            PlayerId = player.Id,
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = DateTime.Now
        };
        await this.Database.Notifications.InsertOneAsync(notification);
    }


    public async Task<List<RuntimeNotification>> GetNotifications(Player player)
    {
        return await this.Database.Notifications.Find(n => n.PlayerId == player.Id).ToListAsync();
    }


    public async Task DeleteNotification(Player player, string notificationId)
    {
        var notification = await this.Database.Notifications.Find(n => n.Id == notificationId).FirstOrDefaultAsync();
        if (notification == null)
            throw new ServerException("Notification not found", StatusCodes.Status404NotFound);
        if (notification.PlayerId != player.Id)
            throw new ServerException("Notification not found", StatusCodes.Status404NotFound);
        await this.Database.Notifications.DeleteOneAsync(n => n.Id == notificationId);
    }


    public async Task ClearNotifications(Player player)
    {
        await this.Database.Notifications.DeleteManyAsync(n => n.PlayerId == player.Id);
    }
}