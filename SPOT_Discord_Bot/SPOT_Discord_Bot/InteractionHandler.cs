/*
 * File: InteractionHandler.cs
 * Purpose: Registers command modules and routes slash command executions.
 * Role: Serves as the command listener layer between DiscordSocketClient and InteractionService.
 * Interacts With:
 *   - DiscordSocketClient (for receiving interactions)
 *   - InteractionService (executes slash command logic)
 *   - Module files inside /Modules (e.g., PingModule, VibeModule)
 * Notes:
 *   - Keeps Discord routing logic separate from command definitions and services
 *   - Can be extended to include autocomplete, component handlers, etc.
 */
namespace SPOT_Discord_Bot;

public class InteractionHandler
{
    
}