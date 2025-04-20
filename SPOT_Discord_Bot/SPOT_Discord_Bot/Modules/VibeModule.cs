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
    
    // Static CommandInterface to use the OPENAI and Spotify Services
    public static CommandInterface? CommandInterface;
    
    // The 'actual' /vibe command
    // This allows the users to actually search for it on their Discord server
    // And this is the logic of how the query should be input into the server.
    // Additionally, this begins the backend process of the data processing
    // (pining OpenAI and then Spotify Web API)!
    [SlashCommand("vibe", "Send a vibe and get a reply!")]
    public async Task HandleVibeCommand([Summary(description: "What kind of vibe are you feeling?")] string query)
    {
        Logger?.LogInformation("/vibe called with query: \"{Query}\" by User: {User}", query, Context.User.Username);

        var request = new RequestModel
        {
            Query = query,
            Username = Context.User.Username
        };

        try
        {
            string reply = await CommandInterface!.HandleVibeAsync(request);
            await RespondAsync($"Here's your expanded vibe:\n{reply}");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error while processing /vibe for user {User}", Context.User.Username);

            // Avoid timeout by always responding
            await RespondAsync("Sorry! Something went wrong while processing your vibe 🥲", ephemeral: true);
        }
    }
}