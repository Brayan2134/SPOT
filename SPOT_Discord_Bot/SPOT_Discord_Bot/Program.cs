/*
 * File: Program.cs
 * Purpose: Application entrypoint and main orchestrator.
 * Role: Initializes and launches the core BotService, then blocks execution.
 * Interacts with:
 *  - BotService: Delegates all bot startup and Discord logic.
 * Notes:
 *  - Should remain minimal and purely orchestration-focused.
 *  - Never interacts directly with commands, services, or Discord API.
 */
using Microsoft.Extensions.Logging;
using SPOT_Discord_Bot;

public class Program
{
    public static async Task Main()
    {
        // STEP 1: Set up the logging factory
        // This configures how logs will be written and displayed (e.g., in the console).
        // - 'AddSimpleConsole' outputs logs in a simple format
        // - 'TimestampFormat' controls how timestamps look
        // - 'SetMinimumLevel' ensures only logs at Information level or above are shown
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;             // One-line logs for clean readability
                    options.TimestampFormat = "hh:mm:ss "; // Add a timestamp to each log
                })
                .SetMinimumLevel(LogLevel.Information);    // You can raise/lower this to Debug, Warning, etc.
        });

        // STEP 2: Create a logger for Program.cs
        // Logs written here will be tagged with the context "Program"
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("🔧 Logger initialized.");

        // STEP 3: Instantiate the core bot logic layer (BotService)
        // This class manages the Discord client, command handler, and service bootstrapping
        // We're injecting a logger specifically for BotService to keep logs organized
        var botService = new BotService(loggerFactory.CreateLogger<BotService>());
        await botService.InitializeAsync(); // Run startup logic (connects to Discord, registers commands, etc.)

        // STEP 4: Prevent the application from exiting
        // Task.Delay(-1) blocks the main thread indefinitely
        // This ensures the bot stays online and responsive to Discord events
        await Task.Delay(-1);
    }
}