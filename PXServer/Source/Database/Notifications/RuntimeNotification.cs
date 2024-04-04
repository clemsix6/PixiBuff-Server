using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace PXServer.Source.Database.Notifications;


public enum NotificationType
{
    Info,
    Popup,
    Error
}



[Table("runtime_player_notifications")]
public class RuntimeNotification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string PlayerId { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required NotificationType Type { get; init; }
    public required DateTime CreatedAt { get; init; }
}