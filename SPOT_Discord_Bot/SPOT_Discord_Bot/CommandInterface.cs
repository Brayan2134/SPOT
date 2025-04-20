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

    public CommandInterface(OpenAIService openAiService, ILogger<CommandInterface> logger)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    public async Task<string> HandleVibeAsync(RequestModel request)
    {
        _logger.LogInformation("Processing vibe request for user {User}", request.Username);
        var expandedPrompt = await _openAiService.ExpandPromptAsync(request);
        return expandedPrompt;
    }
}