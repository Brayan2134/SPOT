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

public class RequestModel
{
    public string Query { get; set; } = "";
    public string Username { get; set; } = "";
}