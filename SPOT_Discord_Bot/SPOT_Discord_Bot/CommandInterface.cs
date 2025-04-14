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
namespace SPOT_Discord_Bot;

public class CommandInterface
{
    
}