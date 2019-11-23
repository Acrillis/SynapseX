using System.Security.Authentication;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Synapse_Bot.Utility;
using Synapse_Bot.Utility_Methods;

namespace Synapse_Bot.Command_Structure.Administrator_Commands
{
    /*
    *
    *	SYNAPSE BOT
    *	File.:	C_STRUCT/ADMIN/Chat.cs
    *	Desc.:	Chat-based commands for ADMINISTRATORS.
    *
    */

    public class ChatAdministrator : ModuleBase
    {
        [Command("ping"), Summary("Returns \"Pong!\" if the bot is alive.")]
        public async Task Ping()
        {
            if (!GlobalMethods.authenticateCaller(new Administrator(), Context.Message.Author)) throw new AuthenticationException("Requires Administrator or higher.");
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithDescription("Pong!");
            await ReplyAsync(null, false, embedBuilder);
        }
    }
}
