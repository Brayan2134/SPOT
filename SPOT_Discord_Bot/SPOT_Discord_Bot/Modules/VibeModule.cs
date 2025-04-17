/*
 * File: VibeModule.cs
 * Purpose: Defines the /vibe slash command and sends structured requests to the CommandInterface.
 * Role: Acts as the user-facing command entry point for mood-based playlist generation.
 * Interacts With:
 *   - CommandInterface.cs (sends prompt, receives track list)
 *   - Discord.Interactions (slash command registration and response)
 * Notes:
 *   - Should remain purely UI-layer (Discord objects only)
 *   - Avoids direct service access — all logic goes through CommandInterface
 */
using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace SPOT_Discord_Bot.Modules;

public class VibeModule : InteractionModuleBase<SocketInteractionContext>
{
    // Static logger assigned at startup (for simplicity)
    public static ILogger<VibeModule>? Logger;
    
    [SlashCommand("vibe", "Send a vibe and get a reply!")]
    public async Task HandleVibeCommand([Summary(description: "What kind of vibe are you feeling?")] string query)
    {
        Logger?.LogInformation("/vibe called with query: \"{Query}\" by User: {User}", query, Context.User.Username);
        
        await RespondAsync($"Got your vibe: **{query}**");
    }
}