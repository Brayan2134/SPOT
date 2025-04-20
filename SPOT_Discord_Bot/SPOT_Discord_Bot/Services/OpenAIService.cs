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

 /**
  * SUMMARY: Expands a user's "vibe" query using OpenAI GPT API to generate a list of musical genres, moods, or artists.
  * PRE CONDITIONS:
  *     - The OPENAI_API_KEY environment variable must be set and valid.
  *     - The input <paramref name="request"/> must contain a non-empty Query string and Username for logging.
  *     - This method should be called from a high-level service like CommandInterface.cs (never from UI directly).
  * POST CONDITIONS:
  *     - Returns a trimmed string containing the expanded vibe content from OpenAI.
  *     - Logs all major events (query, outbound prompt, OpenAI response, and errors if any).
  *     - Always returns a non-null string, even in error cases (fallback string provided).
  * PARAMS:
  *     - RequestModel request: holds the original user query and their Discord username.
  *                             This is typically constructed inside a command handler (e.g., VibeModule.cs)
  *                             and passed through CommandInterface.cs.
  * RETURN:
  *     - str: A string containing OpenAI's response: an expanded version of the user's vibe query.
  */
 public async Task<string> ExpandPromptAsync(RequestModel request)
    {
        try
        {
            // STEP 1: Retrieve the OpenAI API key from environment variables
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Missing OPENAI_API_KEY environment variable.");
                throw new InvalidOperationException("OpenAI API key not set.");
            }

            // STEP 2: Set up the authorization header for the OpenAI request
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // STEP 3: Build the full user prompt
            // The prompt includes a base system instruction and the user’s original query
            string systemPrompt = "You are a playlist assistant. Expand this user vibe into a list of moods, genres, or artist styles:";
            string fullPrompt = $"{systemPrompt} \"{request.Query}\"";

            // STEP 4: Format the request body in the structure required by OpenAI's Chat API
            var requestBody = new
            {
                model = "gpt-3.5-turbo", // NOTE: You can upgrade to "gpt-4" if needed
                messages = new[]
                {
                    new { role = "user", content = fullPrompt }
                }
            };

            // STEP 5: Serialize the body to JSON for the HTTP request
            string jsonPayload = JsonSerializer.Serialize(requestBody);
            
            // STEP 6: Log the outgoing request for observability
            _logger.LogInformation("Sending prompt to OpenAI for user {User}: {Prompt}", request.Username, fullPrompt);

            // STEP 7: Send the POST request to OpenAI
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

            // STEP 8: Read the response as a raw string for logging and debugging
            string responseText = await response.Content.ReadAsStringAsync();

            // STEP 9: If the response failed (non-200), log the full body and return an error message
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API returned non-success status {StatusCode}: {Body}", response.StatusCode, responseText);
                throw new HttpRequestException($"OpenAI error: {response.StatusCode}");
            }

            // STEP 10: Parse the JSON response to extract the chatbot reply
            var json = JsonDocument.Parse(responseText);
            string? reply = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // STEP 11: Handle cases where the API returns a valid response but no content
            if (reply == null)
            {
                _logger.LogWarning("OpenAI response parsed but no content was found.");
                return "OpenAI returned no reply.";
            }

            // STEP 12: Log and return the final expanded result
            _logger.LogInformation("OpenAI reply for {User}: {Reply}", request.Username, reply.Trim());
            return reply.Trim();
        }
        catch (Exception ex)
        {
            // STEP 13: Catch any unexpected errors and log them with full details
            _logger.LogError(ex, "Error while calling OpenAI API for user {User}", request.Username);
            return "Failed to expand your vibe. Please try again later.";
        }
    }
}