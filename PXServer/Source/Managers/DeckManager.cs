using MongoDB.Driver;
using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Decks;
using PXServer.Source.Database.Pixs;
using PXServer.Source.Database.Players;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Managers;


public class DeckManager : Manager
{
    public DeckManager(MongoDbContext database) : base(database)
    {
    }


    public RuntimeDeck CreateDeck(RuntimePlayer runtimePlayer, string name)
    {
        // Check if the player exists in the database
        if (this.Database.RuntimePlayers.Find(x => x.Id == runtimePlayer.Id).FirstOrDefault() == null)
            throw new ServerException("Player not found", StatusCodes.Status404NotFound);

        // Create the deck
        var deck = new RuntimeDeck
        {
            PlayerId = runtimePlayer.Id,
            Name = name,
            Pixes = []
        };

        // Insert the deck into the database
        this.Database.RuntimeDecks.InsertOne(deck);
        return deck;
    }


    public async Task AddPix(RuntimeDeck deck, RuntimePix pix, int position = -1)
    {
        // Check if deck exists in the database
        if (this.Database.RuntimeDecks.Find(x => x.Id == deck.Id).FirstOrDefault() == null)
            throw new ServerException("Deck not found", StatusCodes.Status404NotFound);
        // Check if the pix exists in the database
        if (this.Database.RuntimePixes.Find(x => x.Id == pix.Id).FirstOrDefault() == null)
            throw new ServerException("Pix not found", StatusCodes.Status404NotFound);
        // Check if the deck already contains the pix
        if (deck.Pixes.Any(x => x.Id == pix.Id))
            throw new ServerException("Deck already contains the pix", StatusCodes.Status400BadRequest);
        // Check if the pix belongs to the deck's player
        if (deck.PlayerId != pix.PlayerId)
            throw new ServerException("Pix does not belong to the deck's player", StatusCodes.Status400BadRequest);
        // Check if the player exists in the database
        if (this.Database.RuntimePlayers.Find(x => x.Id == deck.PlayerId).FirstOrDefault() == null)
            throw new ServerException("Player not found", StatusCodes.Status404NotFound);

        // Add the pix to the deck
        if (position == -1)
            deck.Pixes.Add(pix);
        else
            deck.Pixes.Insert(position, pix);

        // Update the deck in the database
        var update = Builders<RuntimeDeck>.Update.Set(x => x.Pixes, deck.Pixes);
        await this.Database.RuntimeDecks.UpdateOneAsync(x => x.Id == deck.Id, update);
    }


    public async Task<List<RuntimeDeck>> GetDecks(RuntimePlayer runtimePlayer)
    {
        // Get the decks from the database
        return await this.Database.RuntimeDecks.Find(d => d.PlayerId == runtimePlayer.Id).ToListAsync();
    }


    public async Task<List<PublicDeck>> GetPublicDecks(RuntimePlayer runtimePlayer)
    {
        // Get the public decks
        var decks = await GetDecks(runtimePlayer);
        return decks.Select(x => x.GetPublicDeck()).ToList();
    }
}