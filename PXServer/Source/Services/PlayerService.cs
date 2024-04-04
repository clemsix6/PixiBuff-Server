using System.Security.Claims;
using MongoDB.Driver;
using PXServer.Source.Database;
using PXServer.Source.Database.Players;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Services;


public class PlayerService
{
    private readonly MongoDbContext database;


    public PlayerService(MongoDbContext database)
    {
        this.database = database;
    }


    public async Task<Player> GetPlayer(ClaimsPrincipal user)
    {
        if (!user.Claims.Any())
            throw new ServerException("Invalid token", StatusCodes.Status401Unauthorized);
        var userId = user.Claims.First().Value;
        var filter = Builders<Player>.Filter.Eq(p => p.Id, userId);
        var userCursor = await this.database.RuntimePlayers.FindAsync(filter);
        var player = await userCursor.FirstOrDefaultAsync();
        if (player == null)
            throw new ServerException("Player not found", StatusCodes.Status401Unauthorized);
        return player;
    }
}