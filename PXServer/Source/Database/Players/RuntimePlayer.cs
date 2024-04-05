using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace PXServer.Source.Database.Players;


[Table("runtime_players")]
public class RuntimePlayer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string Name { get; init; }
    public required string Password { get; init; }
}