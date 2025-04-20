/*
 * File: BotService.cs
 * Purpose: Manages DiscordSocketClient lifecycle, configuration, and command handling initialization.
 * Role: Handles Discord connection, login, client events, and sets up InteractionService.
 * Interacts With:
 *   - DiscordSocketClient (core Discord connection)
 *   - InteractionHandler (for slash command registration and execution)
 * Notes:
 *   - Contains logging hooks for Discord.NET events
 *   - Extensible to include dependency injection or service registration later
 */
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Discord.Interactions;
using SPOT_Discord_Bot.Modules;
using SPOT_Discord_Bot.Services;

namespace SPOT_Discord_Bot;

public class BotService
{
    // Main Discord client that handles connection, events, and messages
    private readonly DiscordSocketClient _client;
    
    // Structured logger instance scoped to this class
    private readonly ILogger<BotService> _logger;
    
    private readonly ILoggerFactory _loggerFactory;

    // Constructor for BotService. Sets up the Discord client and stores the injected logger.
    // Logger instance from Program.cs for structured output
    public BotService(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<BotService>();

        // Initialize the Discord client with selected gateway intents
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages
        });

        _logger.LogInformation("BotService instantiated with configured GatewayIntents.");
    }

    // Connects the bot to Discord, handles login, event hooks, and readiness.
    public async Task InitializeAsync()
    {
        _client.Log += HandleDiscordLog;

        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogError("DISCORD_TOKEN environment variable not found.");
            return;
        }

        var interactionService = new InteractionService(_client.Rest);
        var interactionHandler = new InteractionHandler(_client, interactionService, _logger);
        var vibeLogger = _loggerFactory.CreateLogger<VibeModule>();
        
        SPOT_Discord_Bot.Modules.VibeModule.Logger = vibeLogger;
        
        var openAiLogger = _loggerFactory.CreateLogger<OpenAIService>();
        var commandLogger = _loggerFactory.CreateLogger<CommandInterface>();
        var openAiService = new OpenAIService(openAiLogger);
        var commandInterface = new CommandInterface(openAiService, commandLogger);
        SPOT_Discord_Bot.Modules.VibeModule.CommandInterface = commandInterface;

        
        await interactionHandler.InitializeAsync();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.Ready += async () =>
        {
            _logger.LogInformation("Bot is connected and ready.");
            await interactionService.RegisterCommandsGloballyAsync();
        };
    }


    // Translates Discord.NET log messages into structured .NET logs.
    private Task HandleDiscordLog(LogMessage msg)
    {
        // Map Discord log severity to .NET log levels
        LogLevel level = msg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        // Write the log using the structured logger
        _logger.Log(level, msg.Exception, msg.Message);
        return Task.CompletedTask;
    }
}