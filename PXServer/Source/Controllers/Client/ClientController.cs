using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using PXResources.Source.NetworkShared.Client;
using PXResources.Source.Users;
using PXServer.Source.Database.Mongo;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Controllers.Client;


[ApiController]
[Route("client")]
public class ClientController(MongoDbContext db) : ControllerBase
{
    private static string HashPassword(string password)
    {
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }


    private TokenInfoDTO GenerateJwtToken(User user)
    {
        var key = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrEmpty(key))
            throw new StartupException("JWT_SECRET environment variable is not set");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
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
        return new TokenInfoDTO
        {
            Token = tokenString,
            Expiration = expires
        };
    }


    private async Task CheckUserExists(string name)
    {
        var userCursor = await db.Users.FindAsync(u => u.Name == name);
        var user = await userCursor.FirstOrDefaultAsync();
        if (user != null)
            throw new RouteException("User with this name already exists", StatusCodes.Status409Conflict);
    }


    private void CheckPasswordComplexity(string password)
    {
        var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$");
        if (!passwordRegex.IsMatch(password))
            throw new RouteException(
                "Password must contain at least 8 characters, one uppercase letter, one lowercase letter and one digit",
                StatusCodes.Status400BadRequest
            );
    }


    private User CreateUser(RegisterDTO registerDto)
    {
        var user = new User
        {
            Name = registerDto.Name,
            Password = HashPassword(registerDto.Password),
            Level = 1
        };
        return user;
    }


    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientCredentialsDTO>> Register([FromBody] RegisterDTO registerDto)
    {
        await CheckUserExists(registerDto.Name);
        CheckPasswordComplexity(registerDto.Password);
        var user = CreateUser(registerDto);
        await db.Users.InsertOneAsync(user);

        var credentialsDto = new ClientCredentialsDTO
        {
            Id = user.Id,
            Name = user.Name,
            TokenInfo = GenerateJwtToken(user)
        };
        return CreatedAtAction(nameof(Register), credentialsDto);
    }


    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientCredentialsDTO>> Login([FromBody] RegisterDTO loginDto)
    {
        var userCursor = await db.Users.FindAsync(u => u.Name == loginDto.Name);
        var user = await userCursor.FirstOrDefaultAsync();
        if (user == null || user.Password != HashPassword(loginDto.Password))
            return Unauthorized();

        var credentialsDto = new ClientCredentialsDTO
        {
            Id = user.Id,
            Name = user.Name,
            TokenInfo = GenerateJwtToken(user)
        };
        return Ok(credentialsDto);
    }


    [Authorize]
    [HttpGet("check-token")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult TestToken()
    {
        return NoContent();
    }
}