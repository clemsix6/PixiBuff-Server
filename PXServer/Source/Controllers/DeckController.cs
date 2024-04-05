using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PXResources.Shared.Resources;
using PXServer.Source.Managers;
using PXServer.Source.Services;


namespace PXServer.Source.Controllers;

[ApiController]
[Route("deck")]
public class DeckController : ControllerBase
{
    private readonly PlayerService playerService;
    private readonly DeckManager deckManager;


    public DeckController(PlayerService playerService, DeckManager deckManager)
    {
        this.playerService = playerService;
        this.deckManager = deckManager;
    }



    [Authorize]
    [HttpGet("decks")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<PublicDeck>>> GetDecks()
    {
        var player = await this.playerService.GetPlayer(this.User);
        var decks = await this.deckManager.GetDecks(player);
        return Ok(decks);
    }
}