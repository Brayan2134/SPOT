/*
 * File: RequestModel.cs
 * Purpose: Defines a standardized structure for passing user input and metadata into backend services.
 * Role: Allows services to operate on a consistent data format regardless of source (Discord, web, CLI).
 * Interacts With:
 *   - CommandInterface.cs (used as input model)
 *   - Services/OpenAIService.cs, SpotifyService.cs (consume or return variants of this model)
 * Notes:
 *   - May be extended to include timestamp, locale, mood category, etc.
 *   - Ideal place for validation or preprocessing if needed
 */
namespace SPOT_Discord_Bot.Modules;

/**
 * SUMMARY:
 *          A simple data container for user input passed from Discord to backend services.
 *          Used across the entire bot pipeline to maintain a consistent data structure.
 * PRE CONDITIONS:
 *          - Query should contain a valid, non-empty string from the user.
 *          - Username is recommended for logging and analytics, but not strictly required.
 * POST CONDITIONS:
 *          - Will be passed to CommandInterface and services like OpenAIService for processing.
 *          - Fields may be extended in the future for richer context (e.g., mood tags, request time).
 */
public class RequestModel
{
    // The raw text query provided by the user, e.g., "summer beach", "study in the rain".
    // Used as input to the OpenAI prompt.
    public string Query { get; set; } = "";
    
    // The username of the Discord user who submitted the query.
    // Mainly used for logging and tracking who sent which prompt.
    public string Username { get; set; } = "";
}