using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using PXResources.Shared.Connection;
using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Players;
using PXServer.Source.Exceptions;
using PXServer.Source.Managers;
using PXServer.Source.Services;


namespace PXServer.Source.Controllers;


[ApiController]
[Route("player")]
public class PlayerController : ControllerBase
{
    private readonly MongoDbContext database;
    private readonly PlayerService playerService;
    private readonly PlayerManager playerManager;
    private readonly InventoryManager inventoryManager;


    public PlayerController(
        MongoDbContext database,
        PlayerService playerService,
        PlayerManager playerManager,
        InventoryManager inventoryManager)
    {
        this.database = database;
        this.playerService = playerService;
        this.playerManager = playerManager;
        this.inventoryManager = inventoryManager;
    }


    private TokenInfo GenerateJwtToken(RuntimePlayer runtimePlayer)
    {
        var key = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrEmpty(key))
            throw new StartupException("JWT_SECRET environment variable is not set");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, runtimePlayer.Id),
        };

        var expires = DateTime.Now.AddMonths(1);
        var token = new JwtSecurityToken(
            issuer: "app_back",
            audience: "app_front",
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);
        return new TokenInfo
        {
            Token = tokenString,
            Expiration = expires
        };
    }


    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Credentials>> Register([FromBody] Authentication authentication)
    {
        var player = await this.playerManager.CreateUser(authentication.Name, authentication.Password);

        var credentials = new Credentials
        {
            Id = player.Id,
            Name = player.Name,
            TokenInfo = GenerateJwtToken(player)
        };
        return CreatedAtAction(nameof(Register), credentials);
    }


    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Credentials>> Login([FromBody] Authentication authentication)
    {
        var user = await this.playerManager.CheckPassword(authentication.Name, authentication.Password);

        var credentials = new Credentials
        {
            Id = user.Id,
            Name = user.Name,
            TokenInfo = GenerateJwtToken(user)
        };
        return Ok(credentials);
    }


    [Authorize]
    [HttpGet("check-token")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TestToken()
    {
        await this.playerService.GetPlayer(this.User);
        return NoContent();
    }


    [Authorize]
    [HttpGet("inventory")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PublicInventory>> GetInventory()
    {
        var player = await this.playerService.GetPlayer(this.User);
        var inventory = await this.inventoryManager.GetInventory(player);
        return Ok(inventory);
    }
}