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
using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace SPOT_Discord_Bot.Modules;

public class VibeModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("vibe", "Send a vibe and get a reply!")]
    public async Task HandleVibeCommand([Summary(description: "What kind of vibe are you feeling?")] string query)
    {
        await RespondAsync($"🎵 Got your vibe: **{query}**");
    }
}