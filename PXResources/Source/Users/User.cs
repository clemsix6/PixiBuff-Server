using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace PXResources.Source.Users;


[Table("users")]
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string Name { get; set; }
    public required string Password { get; init; }

    [DefaultValue(1)]
    public required int Level { get; set; }
}