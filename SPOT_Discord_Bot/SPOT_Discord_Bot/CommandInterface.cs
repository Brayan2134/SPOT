/*
 * File: CommandInterface.cs
 * Purpose: Acts as the intermediary between command input and backend services.
 * Role: Receives structured user requests and delegates them to appropriate APIs (OpenAI, Spotify).
 * Interacts With:
 *   - RequestModel.cs (input formatting)
 *   - OpenAIService.cs (prompt expansion)
 *   - SpotifyService.cs (track/playlist generation)
 *   - Modules (call this as their backend interface)
 * Notes:
 *   - Standardizes all backend input/output formats
 *   - Designed to be frontend-agnostic for potential web or CLI interfaces
 */
using SPOT_Discord_Bot.Modules;
using SPOT_Discord_Bot.Services;
using Microsoft.Extensions.Logging;

namespace SPOT_Discord_Bot;

public class CommandInterface
{
    private readonly OpenAIService _openAiService;
    private readonly ILogger<CommandInterface> _logger;

    /**
     * SUMMARY: Constructs a CommandInterface with injected service and logger.
     * PARAMS:
     *          - openAiService: Backend service that handles prompt expansion using GPT.
     *          - logger: Logger scoped to this interface for observability and debugging.
     */
    public CommandInterface(OpenAIService openAiService, ILogger<CommandInterface> logger)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    /**
     * SUMMARY: Expands a user’s vibe query using the OpenAIService and returns a refined genre/mood string.
     * PRE CONDITIONS:
     *          - The input <paramref name="request"/> must contain a valid Query and Username.
     *          - The OpenAIService must be functional and reachable via API key.
     * POST CONDITIONS:
     *          - Returns a non-null string (either GPT reply or a fallback message).
     *          - Logs the beginning and completion of the request lifecycle.
     * PARAMS:
     *          - request: A RequestModel containing the user's Discord username and their original input query.
     *                     Typically created in VibeModule.cs and passed down through the bot pipeline.
     * RETURN:
     *          - str: A GPT-generated string expanding the input "vibe" into a list of moods, genres, or artists.
     */
    public async Task<string> HandleVibeAsync(RequestModel request)
    {
        _logger.LogInformation("Processing vibe request for user {User}", request.Username);
        var expandedPrompt = await _openAiService.ExpandPromptAsync(request);
        return expandedPrompt;
    }
}