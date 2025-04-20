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
    
    /**
     *  SUMMARY: Discord slash command handler for "/vibe". Takes a user-provided vibe (e.g., "summer beach")
     *  and returns an AI-expanded list of genres or moods.
     *  PRE CONDITIONS:
     *       - This command must be registered and the bot must be online and connected to Discord.
     *       - CommandInterface must be initialized before this command is used.
     *       - A valid query string must be provided by the user through the slash command.
     * POST CONDITIONS:
     *       - Sends a response message to the user within Discord (always responds, even on error).
     *       - Logs the original query and the expanded result or failure.
     *       - Calls the GPT-based OpenAIService via CommandInterface to expand the input.
     * PARAMS:
     *       - query (str): The user's input string describing a mood, theme, or vibe — such as "rainy study session" or "summer road trip".
     *                      This value is passed automatically by Discord when the slash command is invoked.
     * NOTES:
     *       - This method is UI-facing and represents the entry point from Discord to backend services.
     *       - It should remain lean, with all business logic delegated to backend service layers.
     */
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
            // Backend request to expand the user query using OpenAI API
            string reply = await CommandInterface!.HandleVibeAsync(request);
            
            // Always respond to avoid a Discord timeout!!
            await RespondAsync($"Here's your expanded vibe:\n{reply}");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error while processing /vibe for user {User}", Context.User.Username);

            // Fallback response to ensure a graceful user experience
            await RespondAsync("Sorry! Something went wrong while processing your vibe 🥲", ephemeral: true);
        }
    }
}