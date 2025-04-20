/*
 * File: SpotifyService.cs
 * Purpose: Interfaces with the Spotify API to search tracks and create playlists based on expanded queries.
 * Role: Converts genre/mood keywords into a curated song list or playlist link.
 * Interacts With:
 *   - CommandInterface.cs
 *   - OpenAIService.cs (chained output)
 * Notes:
 *   - Handles auth and token refresh
 *   - Can support search-based or user-authenticated playlist creation
 */
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SPOT_Discord_Bot.Services;

public class SpotifyService
{
    private readonly ILogger<SpotifyService> _logger;
    private readonly HttpClient _http;
    private string? _accessToken;
    private string? _refreshToken;
    
    public string AccessToken => _accessToken!;

    public SpotifyService(ILogger<SpotifyService> logger)
    {
        _http = new HttpClient();
        _logger = logger;
        
    }
    
    public void LoadAccessTokenFromFile(string path = "../../../etc/spotify_token.json")
    {
        _logger.LogInformation($"Loading token from: {Path.GetFullPath("../../../etc/spotify_token.json")}");

        if (!File.Exists(path))
        {
            _logger.LogError("Token file not found.");
            return;
        }

        var json = File.ReadAllText(path);
        var doc = JsonDocument.Parse(json);

        _accessToken = doc.RootElement.GetProperty("access_token").GetString();
        _refreshToken = doc.RootElement.GetProperty("refresh_token").GetString();

        _logger.LogInformation("Loaded access and refresh token from file.");
    }
    
    public async Task<string> GetAccessTokenAsync(string code)
    {
        var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
        var redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URL");

        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri }
        };

        var response = await client.PostAsync(
            "https://accounts.spotify.com/api/token",
            new FormUrlEncodedContent(requestBody)
        );

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Token exchange failed: {json}");
            return null!;
        }

        var doc = JsonDocument.Parse(json);
        string accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
        string refreshToken = doc.RootElement.GetProperty("refresh_token").GetString()!;
        int expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();

        _logger.LogInformation($"Access token: {accessToken}");
        _logger.LogInformation($"Refresh token: {refreshToken}");
        _logger.LogInformation($"⏳ Expires in: {expiresIn} seconds");

        return accessToken;
    }
    
    public async Task<bool> RefreshAccessTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_refreshToken))
        {
            _logger.LogError("No refresh token available.");
            return false;
        }

        var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", _refreshToken! }
        });

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to refresh access token: {Response}", json);
            return false;
        }

        var doc = JsonDocument.Parse(json);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString();

        if (doc.RootElement.TryGetProperty("refresh_token", out var newRefreshToken))
        {
            _refreshToken = newRefreshToken.GetString();
        }

        _logger.LogInformation("✅ Successfully refreshed access token.");
        SaveTokensToFile("../../../etc/spotify_token.json");
        return true;
    }
    
    private void SaveTokensToFile(string path)
    {
        var payload = new
        {
            access_token = _accessToken,
            refresh_token = _refreshToken
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);

        _logger.LogInformation("💾 Saved refreshed tokens to file.");
    }
    
    public async Task<List<string>> SearchTracksWithFallbackAsync(string accessToken, Dictionary<string, List<string>> artistSongMap)
    {
        var results = new List<string>();

        try
        {
            foreach (var kvp in artistSongMap)
            {
                string artist = kvp.Key;
                foreach (string song in kvp.Value)
                {
                    string url = await TrySearchAsync(_accessToken!, song, artist);
                    if (!string.IsNullOrEmpty(url))
                    {
                        results.Add(url);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Spotify token might be expired. Attempting refresh...");

            bool success = await RefreshAccessTokenAsync();
            if (!success)
            {
                _logger.LogError("Unable to recover from expired token. Shutting down.");
                Environment.Exit(1); // 👈 Force exit
            }

            // Retry once with new token
            return await SearchTracksWithFallbackAsync(_accessToken!, artistSongMap);
        }

        return results;
    }


    private async Task<string> TrySearchAsync(string token, string song, string artist)
    {
        // 1. Try with both track and artist
        string combinedQuery = $"track:{song} artist:{artist}";
        string? url = await RunSearchQuery(token, combinedQuery);
        if (url != null) return url;

        // 2. Fallback: search just track name
        url = await RunSearchQuery(token, $"track:{song}");
        if (url != null) return url;

        // 3. Fallback: search just artist and return top track
        return await RunSearchQuery(token, $"artist:{artist}");
    }

    private async Task<string?> RunSearchQuery(string token, string query)
    {
        string searchUrl = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=1";

        using var request = new HttpRequestMessage(HttpMethod.Get, searchUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Search error: {json}");
            return null;
        }

        var doc = JsonDocument.Parse(json);
        var tracks = doc.RootElement.GetProperty("tracks").GetProperty("items");
        if (tracks.GetArrayLength() == 0) return null;

        var track = tracks[0];
        return track.GetProperty("external_urls").GetProperty("spotify").GetString();
    }
}