using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using PXServer.Source.Database;
using PXServer.Source.Database.Notifications;
using PXServer.Source.Database.Pixs;
using PXServer.Source.Database.Players;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Managers;


public class PlayerManager : Manager
{
    private readonly NotificationManager notificationManager;
    private readonly CrateManager crateManager;
    private readonly DeckManager deckManager;
    private readonly MailManager mailManager;


    public PlayerManager(
        MongoDbContext database,
        NotificationManager notificationManager,
        CrateManager crateManager,
        DeckManager deckManager,
        MailManager mailManager) :
        base(database)
    {
        this.notificationManager = notificationManager;
        this.crateManager = crateManager;
        this.deckManager = deckManager;
        this.mailManager = mailManager;
    }


    private static string HashPassword(string password)
    {
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }


    private static string GenerateCode()
    {
        var code = new StringBuilder();
        var random = new Random();
        for (var i = 0; i < 6; i++)
            code.Append(random.Next(0, 10));
        return code.ToString();
    }


    private void CheckPassword(string password)
    {
        // Check if the password is between 8 and 64 characters long
        if (password.Length is < 8 or > 64)
            throw new ServerException(
                "Password must be between 8 and 64 characters long", StatusCodes.Status400BadRequest
            );
        // Check if the password contains at least one digit
        if (!password.Any(char.IsDigit))
            throw new ServerException("Password must contain at least one digit", StatusCodes.Status400BadRequest);
        // Check if the password contains at least one lowercase letter
        if (!password.Any(char.IsLower))
            throw new ServerException(
                "Password must contain at least one lowercase letter", StatusCodes.Status400BadRequest
            );
        // Check if the password contains at least one uppercase letter
        if (!password.Any(char.IsUpper))
            throw new ServerException(
                "Password must contain at least one uppercase letter", StatusCodes.Status400BadRequest
            );
        // Check if the password contains at least one special character
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
            throw new ServerException(
                "Password must contain at least one special character", StatusCodes.Status400BadRequest
            );
    }


    private void CheckName(string name)
    {
        // Check if the name is between 4 and 16 characters long
        if (name.Length is < 4 or > 16)
            throw new ServerException("Name must be between 4 and 16 characters long", StatusCodes.Status400BadRequest);
        // Check if the name contains one digits
        if (name.Any(c => !char.IsLetterOrDigit(c)))
            throw new ServerException("Name must not contain any special characters", StatusCodes.Status400BadRequest);
        // Check if the name contains at least 3 letters
        if (name.Count(char.IsLetter) < 3)
            throw new ServerException("Name must contain at least 3 letters", StatusCodes.Status400BadRequest);
        // Check if name contains a special character
        if (Regex.IsMatch(name, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
            throw new ServerException("Name must not contain any special characters", StatusCodes.Status400BadRequest);
        // Check if the name is already taken
        if (this.Database.RuntimePlayers.Find(x => x.Name == name).FirstOrDefault() != null)
            throw new ServerException("Name is already taken", StatusCodes.Status409Conflict);
    }


    private void CheckEmail(string email)
    {
        // Check if the email is valid
        if (!Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            throw new ServerException("Invalid email", StatusCodes.Status400BadRequest);
        // Check if the email is already taken
        if (this.Database.RuntimePlayers.Find(x => x.Email == email).FirstOrDefault() != null)
            throw new ServerException("Email is already taken", StatusCodes.Status409Conflict);
    }


    private void SendCode(string email, string code)
    {
        // Send the code to the email
        this.mailManager.SendMail(
            email,
            "Verification code",
            $"Your verification code is: {code}"
        );
    }


    public async Task<WaitingPlayer> CreateWaitingPlayer(string name, string email, string password)
    {
        // Check if the name, email and password are valid
        CheckName(name);
        CheckEmail(email);
        CheckPassword(password);

        // Hash the password
        var hashedPassword = HashPassword(password);
        // Create the waiting player with the hashed password
        var waitingPlayer = new WaitingPlayer
        {
            Name = name,
            Password = hashedPassword,
            Email = email,
            Code = GenerateCode(),
            Expiration = DateTime.UtcNow.AddMinutes(5),
            TryCount = 0
        };

        // Insert the waiting player into the database
        await this.Database.WaitingPlayers.InsertOneAsync(waitingPlayer);
        // Send the code to the email
        SendCode(email, waitingPlayer.Code);
        // Return the waiting player
        return waitingPlayer;
    }


    private async Task CheckCode(WaitingPlayer player, string code)
    {
        // Check if the code is correct
        if (player.Code == code)
            return;
        // If try count is more than 3, delete the player
        if (player.TryCount >= 3) {
            await this.Database.WaitingPlayers.DeleteOneAsync(x => x.Id == player.Id);
            throw new ServerException("Try count exceeded", StatusCodes.Status400BadRequest);
        }
        // Increment the try count
        player.TryCount++;
        // Update the player in the database
        await this.Database.WaitingPlayers.ReplaceOneAsync(x => x.Name == player.Name, player);
        // Throw an exception
        throw new ServerException("Incorrect code", StatusCodes.Status400BadRequest);
    }


    public async Task<RuntimePlayer> CheckPassword(string identifier, string password)
    {
        // Get the player from the database
        var player = await this.Database.RuntimePlayers.Find(x => x.Name == identifier).FirstOrDefaultAsync();
        // Check if the player exists
        if (player == null) {
            player = await this.Database.RuntimePlayers.Find(x => x.Email == identifier).FirstOrDefaultAsync();
            if (player == null)
                throw new ServerException("User not found", StatusCodes.Status404NotFound);
        }

        // Hash the password
        var hashedPassword = HashPassword(password);
        // Check if the password is correct
        if (player.Password != hashedPassword)
            throw new ServerException("Incorrect password", StatusCodes.Status400BadRequest);

        // Return the player
        return player;
    }


    public async Task<RuntimePlayer> ValidatePlayer(string identifier, string code)
    {
        // Get the player from the database
        var player = await this.Database.WaitingPlayers.Find(x => x.Name == identifier).FirstOrDefaultAsync();
        if (player == null) {
            player = await this.Database.WaitingPlayers.Find(x => x.Email == identifier).FirstOrDefaultAsync();
            if (player == null)
                throw new ServerException("Player not found or deleted", StatusCodes.Status404NotFound);
        }
        // Check if the waiting player exists
        if (player == null)
            throw new ServerException("Waiting player not found", StatusCodes.Status404NotFound);
        // Check if the waiting player is expired
        if (player.Expiration < DateTime.UtcNow) {
            await this.Database.WaitingPlayers.DeleteOneAsync(x => x.Id == player.Id);
            throw new ServerException("Waiting player is expired", StatusCodes.Status400BadRequest);
        }

        // Create the runtime player
        var runtimePlayer = new RuntimePlayer
        {
            Name = player.Name,
            Password = player.Password,
            Email = player.Email
        };

        // Insert the runtime player into the database
        await this.Database.RuntimePlayers.InsertOneAsync(runtimePlayer);
        // Delete the waiting player from the database
        await this.Database.WaitingPlayers.DeleteOneAsync(x => x.Id == player.Id);
        // Call the OnUserCreated event
        await OnUserCreated(runtimePlayer);
        // Return the runtime player
        return runtimePlayer;
    }


    private async Task OnUserCreated(RuntimePlayer runtimePlayer)
    {
        // Log the user creation
        this.Logger.Info($"[+] {runtimePlayer.Name}");

        // Send a welcome notification
        await this.notificationManager.SendNotification(
            runtimePlayer,
            "Welcome",
            "Welcome to the game!",
            NotificationType.Info
        );

        // Add a starter crate
        var cratePrefab = this.Database.CratePrefabs.Find(c => c.PrefabId == "starter_crate").FirstOrDefault();
        await this.crateManager.AddCrate(runtimePlayer, cratePrefab, 5);

        // Add a main deck
        this.deckManager.CreateDeck(runtimePlayer, "main");
    }
}