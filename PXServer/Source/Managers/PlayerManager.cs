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


    public PlayerManager(
        MongoDbContext database,
        NotificationManager notificationManager,
        CrateManager crateManager,
        DeckManager deckManager) :
        base(database)
    {
        this.notificationManager = notificationManager;
        this.crateManager = crateManager;
        this.deckManager = deckManager;
    }


    private static string HashPassword(string password)
    {
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
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


    public async Task<RuntimePlayer> CreateUser(string name, string password)
    {
        // Check if the name and password are valid
        CheckName(name);
        CheckPassword(password);

        // Hash the password
        var hashedPassword = HashPassword(password);
        // Create the user with the hashed password
        var user = new RuntimePlayer
        {
            Name = name,
            Password = hashedPassword
        };

        // Insert the user into the database
        await this.Database.RuntimePlayers.InsertOneAsync(user);
        // Call the OnUserCreated event
        await OnUserCreated(user);
        // Return the user
        return user;
    }


    public async Task<RuntimePlayer> CheckPassword(string username, string password)
    {
        // Get the user from the database
        var user = await this.Database.RuntimePlayers.Find(x => x.Name == username).FirstOrDefaultAsync();
        // Check if the user exists
        if (user == null)
            throw new ServerException("User not found", StatusCodes.Status404NotFound);

        // Hash the password
        var hashedPassword = HashPassword(password);
        // Check if the password is correct
        if (user.Password != hashedPassword)
            throw new ServerException("Incorrect password", StatusCodes.Status400BadRequest);

        // Return the user
        return user;
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