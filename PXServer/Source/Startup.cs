using System.Text;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using PXResources.Shared.Resources;
using PXServer.Source.Database;
using PXServer.Source.Database.Crates;
using PXServer.Source.Database.Pixs;
using PXServer.Source.Exceptions;
using PXServer.Source.Managers;
using PXServer.Source.Middlewares;
using PXServer.Source.Services;
using Swashbuckle.AspNetCore.SwaggerUI;


namespace PXServer.Source;


public class Startup
{
    private static readonly Logger Logger =
        LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();


    private static void AddJwtAuthentication(IHostApplicationBuilder builder)
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrEmpty(secretKey))
            throw new StartupException("JWT_SECRET environment variable is not set");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = "app_back",
                        ValidateAudience = true,
                        ValidAudience = "app_front",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                }
            );
        builder.Services.AddAuthorization();
    }


    private static void AddSwagger(IHostApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(
            c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Area API", Version = "v1" });

                c.AddSecurityDefinition(
                    "Bearer", new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    }
                );

                c.OperationFilter<AuthorizeCheckOperationFilter>();
            }
        );
    }


    private static void AddJsonOptions(IHostApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .AddJsonOptions(
                options => { options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy(); }
            );
    }


    private static void SetupLogging(WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews();
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();
    }


    private static void AddManagers(IHostApplicationBuilder builder)
    {
        var database = new MongoDbContext();
        builder.Services.AddSingleton(database);

        var mailManager = new MailManager(database);

        var playerService = new PlayerService(database);
        builder.Services.AddSingleton(playerService);

        var notificationManager = new NotificationManager(database);
        builder.Services.AddSingleton(notificationManager);

        var pixManager = new PixManager(database);
        builder.Services.AddSingleton(pixManager);

        var deckManager = new DeckManager(database);
        builder.Services.AddSingleton(deckManager);

        var crateManager = new CrateManager(database, pixManager);
        builder.Services.AddSingleton(crateManager);

        var inventoryManager = new InventoryManager(database, crateManager, pixManager);
        builder.Services.AddSingleton(inventoryManager);

        var playerManager = new PlayerManager(database, notificationManager, crateManager, deckManager, mailManager);
        builder.Services.AddSingleton(playerManager);
    }


    private static WebApplicationBuilder SetupBuilder(string[] args)
    {
        Logger.Info("Building application...");
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        SetupLogging(builder);
        AddManagers(builder);
        AddJsonOptions(builder);
        AddJwtAuthentication(builder);
        AddSwagger(builder);
        return builder;
    }


    private static void RunApp(WebApplication app)
    {
        Logger.Info("Running application...");
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<LoggingMiddleware>();

        app.MapControllers();
        app.UseSwaggerUI(
            c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PixiBuff API");
                c.DocExpansion(DocExpansion.None);
                c.OAuthUsePkce();
            }
        );
        app.UseSwagger();
        app.UseAuthentication();
        app.UseAuthorization();

        Logger.Info("Application started");
        app.Run();
    }


    public static void Main(string[] args)
    {
        DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { "auth.env", "db.env", "email.env" }));
        var builder = SetupBuilder(args);
        var app = builder.Build();
        RunApp(app);
    }


    private static void AddElements()
    {
        var ctx = new MongoDbContext();
        var pix = new PixPrefab
        {
            PrefabId = "anola",

            Name = "Anola",
            Description = "An electric blue bird.",

            BaseStats = new PublicPixStats(79, 112, 31, 16),

            Types = new List<string> { "electric" },
            StartingAbilities = new List<string> { "electric_strike", "charge" }
        };
        ctx.PixPrefabs.InsertOne(pix);

        var electricStrike = new PrefabAbility
        {
            PrefabId = "electric_strike",
            Name = "Electric Strike",
            Description = "A powerful electric attack.",
            Type = "electric",
            BasePower = 20,
            BaseAccuracy = 100,
            Category = "offensive"
        };
        ctx.AbilityPrefabs.InsertOne(electricStrike);

        var charge = new PrefabAbility
        {
            PrefabId = "charge",
            Name = "Charge",
            Description = "The pix charges up its energy and then releases it in a powerful attack.",
            Type = "electric",
            BasePower = 35,
            BaseAccuracy = 100,
            Category = "offensive"
        };
        ctx.AbilityPrefabs.InsertOne(charge);

        var starterCrate = new CratePrefab
        {
            PrefabId = "starter_crate",
            Name = "Starter Crate",
            Description = "A crate that contains a random starter pix.",
            Loot =
            [
                new CrateLootPrefab { PixPrefabId = "anola", Weight = 100, Level = 1 }
            ]
        };
        ctx.CratePrefabs.InsertOne(starterCrate);
    }
}