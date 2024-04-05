using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PXResources.Shared.Resources;
using PXServer.Source.Database.Pixs;


namespace PXServer.Source.Database.Decks;


[Table("runtime_decks")]
public class RuntimeDeck
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = null!;

    public required string PlayerId { get; init; }
    public required string Name { get; init; }
    public required List<RuntimePix> Pixes { get; init; }


    public PublicDeck GetPublicDeck()
    {
        return new PublicDeck
        {
            Id = this.Id,
            Name = this.Name,
            Pixs = this.Pixes.Select(pix => pix.GetPublicPix()).ToList()
        };
    }
}