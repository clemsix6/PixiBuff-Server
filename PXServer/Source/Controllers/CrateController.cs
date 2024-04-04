using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PXResources.Shared.Resources;
using PXServer.Source.Managers;
using PXServer.Source.Services;



namespace PXServer.Source.Controllers;


[ApiController]
[Route("crate")]
public class CrateController : ControllerBase
{
    private readonly PlayerService playerService;
    private readonly CrateManager crateController;


    public CrateController(PlayerService playerService, CrateManager crateController)
    {
        this.playerService = playerService;
        this.crateController = crateController;
    }


    [Authorize]
    [HttpGet("crates")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<PublicCrate>>> GetCrates()
    {
        var player = await this.playerService.GetPlayer(this.User);
        var crates = await this.crateController.GetPublicCrates(player);
        return Ok(crates);
    }


    [Authorize]
    [HttpPost("open/{crateId}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicPix>> OpenCrate([FromRoute] string crateId)
    {
        var player = await this.playerService.GetPlayer(this.User);
        var pix = await this.crateController.OpenCrate(player, crateId);
        return Ok(pix.GetPublicPix());
    }

}