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
    private readonly SpotifyService _spotifyService;
    private readonly ILogger<CommandInterface> _logger;

    /**
     * SUMMARY: Constructs a CommandInterface with injected service and logger.
     * PARAMS:
     *          - openAiService: Backend service that handles prompt expansion using GPT.
     *          - logger: Logger scoped to this interface for observability and debugging.
     */
    public CommandInterface(OpenAIService openAiService, SpotifyService spotifyService, ILogger<CommandInterface> logger)
    {
        _openAiService = openAiService;
        _spotifyService = spotifyService;
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
    public async Task<List<string>> HandleVibeAsync(RequestModel request)
    {
        _logger.LogInformation("Processing vibe request for user {User}", request.Username);

        string expanded = await _openAiService.ExpandPromptAsync(request);
        var artistSongs = ParseArtistSongMap(expanded);

        _spotifyService.LoadAccessTokenFromFile();

        var tracks = await _spotifyService.SearchTracksWithFallbackAsync(
            _spotifyService.AccessToken, artistSongs);

        
        _logger.LogInformation("Sending {Count} tracks to user {User}", tracks.Count, request.Username);
        _logger.LogDebug("Track list for user {User}:\n{Tracks}", request.Username, string.Join("\n", tracks));

        return tracks;
    }

    
    private Dictionary<string, List<string>> ParseArtistSongMap(string gptOutput)
    {
        var lines = gptOutput.Split('\n');
        var result = new Dictionary<string, List<string>>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim().TrimStart('-').Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            // Expecting format: Song Title - Artist Name
            var parts = trimmed.Split(" - ", 2);
            if (parts.Length == 2)
            {
                var song = parts[0].Trim();
                var artist = parts[1].Trim();

                if (!result.ContainsKey(artist))
                    result[artist] = new List<string>();

                result[artist].Add(song);
            }
        }

        return result;
    }
}