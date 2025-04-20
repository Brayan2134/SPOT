/*
 * File: OpenAIService.cs
 * Purpose: Handles all interaction with the OpenAI GPT API for prompt expansion and mood analysis.
 * Role: Converts raw user queries into refined genre/mood descriptors.
 * Interacts With:
 *   - CommandInterface.cs (receives prompt)
 *   - RequestModel.cs (input)
 * Notes:
 *   - Should be async and isolated from Discord dependencies
 *   - Future: can include prompt templating or fine-tuning integration
 */
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SPOT_Discord_Bot.Modules;

namespace SPOT_Discord_Bot.Services;

public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(ILogger<OpenAIService> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
    }

 public async Task<string> ExpandPromptAsync(RequestModel request)
    {
        try
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Missing OPENAI_API_KEY environment variable.");
                throw new InvalidOperationException("OpenAI API key not set.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            string systemPrompt = "You are a playlist assistant. Expand this user vibe into a list of moods, genres, or artist styles:";
            string fullPrompt = $"{systemPrompt} \"{request.Query}\"";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = fullPrompt }
                }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);
            _logger.LogInformation("Sending prompt to OpenAI for user {User}: {Prompt}", request.Username, fullPrompt);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

            string responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API returned non-success status {StatusCode}: {Body}", response.StatusCode, responseText);
                throw new HttpRequestException($"OpenAI error: {response.StatusCode}");
            }

            var json = JsonDocument.Parse(responseText);
            string? reply = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (reply == null)
            {
                _logger.LogWarning("OpenAI response parsed but no content was found.");
                return "OpenAI returned no reply.";
            }

            _logger.LogInformation("OpenAI reply for {User}: {Reply}", request.Username, reply.Trim());
            return reply.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calling OpenAI API for user {User}", request.Username);
            return "Failed to expand your vibe. Please try again later.";
        }
    }
}