using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using PXResources.Shared.Connection;
using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Notifications;
using PXServer.Source.Database.Players;
using PXServer.Source.Engine;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Controllers.Client;


[ApiController]
[Route("player")]
public class PlayerController : ControllerBase
{
    private readonly MongoDbContext database;
    private readonly PlayerManager playerManager;
    private readonly NotificationManager notificationManager;
    private readonly InventoryManager inventoryManager;


    public PlayerController(
        MongoDbContext database,
        PlayerManager playerManager,
        NotificationManager notificationManager,
        InventoryManager inventoryManager)
    {
        this.database = database;
        this.playerManager = playerManager;
        this.notificationManager = notificationManager;
        this.inventoryManager = inventoryManager;
    }


    private static string HashPassword(string password)
    {
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }


    private TokenInfo GenerateJwtToken(Player player)
    {
        var key = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrEmpty(key))
            throw new StartupException("JWT_SECRET environment variable is not set");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, player.Id),
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


    private async Task CheckUserExists(string name)
    {
        var userCursor = await this.database.RuntimePlayers.FindAsync(u => u.Name == name);
        var user = await userCursor.FirstOrDefaultAsync();
        if (user != null)
            throw new ServerException("User with this name already exists", StatusCodes.Status409Conflict);
    }


    private void CheckPasswordComplexity(string password)
    {
        var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$");
        if (!passwordRegex.IsMatch(password))
            throw new ServerException(
                "Password must contain at least 8 characters, one uppercase letter, one lowercase letter and one digit",
                StatusCodes.Status400BadRequest
            );
    }


    private async Task<Player> GetPlayer()
    {
        if (!this.User.Claims.Any())
            throw new ServerException("Invalid token", StatusCodes.Status401Unauthorized);
        var userId = this.User.Claims.First().Value;
        var userCursor = await this.database.RuntimePlayers.FindAsync(u => u.Id == userId);
        var player = await userCursor.FirstOrDefaultAsync();
        if (player == null)
            throw new ServerException("Player not found", StatusCodes.Status401Unauthorized);
        return player;
    }


    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Credentials>> Register([FromBody] Authentication authentication)
    {
        await CheckUserExists(authentication.Name);
        CheckPasswordComplexity(authentication.Password);
        var player = await this.playerManager.CreateUser(authentication.Name, HashPassword(authentication.Password));

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
        var userCursor = await this.database.RuntimePlayers.FindAsync(u => u.Name == authentication.Name);
        var user = await userCursor.FirstOrDefaultAsync();
        if (user == null || user.Password != HashPassword(authentication.Password))
            return Unauthorized();

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
        await GetPlayer();
        return NoContent();
    }


    [Authorize]
    [HttpGet("inventory")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Inventory>> GetInventory()
    {
        var player = await GetPlayer();
        var inventory = await this.inventoryManager.GetInventory(player);
        return Ok(inventory);
    }


    [Authorize]
    [HttpGet("notifications")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<RuntimeNotification>>> GetNotifications()
    {
        var player = await GetPlayer();
        var notifications = await this.notificationManager.GetNotifications(player);
        return Ok(notifications);
    }
}