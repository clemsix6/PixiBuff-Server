using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace PXServer.Source.Database.Players;


[Table("waiting_players")]
public class WaitingPlayer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string Name { get; init; }
    public required string Password { get; init; }
    public required string Email { get; init; }

    public required string Code { get; init; }
    public required DateTime Expiration { get; init; }
    public required int TryCount { get; set; }
}