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

using Discord;
using Discord.WebSocket;

public class Program
{
    private static DiscordSocketClient _client;

    public static async Task Main()
    {
        _client = new DiscordSocketClient();

        _client.Log += Log;

        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1); // Keeps the bot alive
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}