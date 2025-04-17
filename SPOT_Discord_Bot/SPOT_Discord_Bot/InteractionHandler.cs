/*
 * File: InteractionHandler.cs
 * Purpose: Registers command modules and routes slash command executions.
 * Role: Serves as the command listener layer between DiscordSocketClient and InteractionService.
 * Interacts With:
 *   - DiscordSocketClient (for receiving interactions)
 *   - InteractionService (executes slash command logic)
 *   - Module files inside /Modules (e.g., PingModule, VibeModule)
 * Notes:
 *   - Keeps Discord routing logic separate from command definitions and services
 *   - Can be extended to include autocomplete, component handlers, etc.
 */
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace SPOT_Discord_Bot;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly ILogger _logger;

    public InteractionHandler(DiscordSocketClient client, InteractionService commands, ILogger logger)
    {
        _client = client;
        _commands = commands;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

        _client.InteractionCreated += async interaction =>
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            var result = await _commands.ExecuteCommandAsync(ctx, null);

            if (!result.IsSuccess)
            {
                _logger.LogWarning($"Command failed: {result.ErrorReason}");
                await interaction.RespondAsync("Something went wrong: ", ephemeral: true);
            }
        };
    }
}