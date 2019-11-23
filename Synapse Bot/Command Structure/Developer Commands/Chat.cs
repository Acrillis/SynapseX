using System.Security.Authentication;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Synapse_Bot.Utility;
using Synapse_Bot.Utility_Methods;

namespace Synapse_Bot.Command_Structure.Developer_Commands
{
    /*
    *
    *	SYNAPSE BOT
    *	File.:	C_STRUCT/DEV/Chat.cs
    *	Desc.:	Chat-based commands for DEVELOPERS.
    *
    */

    public class ChatDev : ModuleBase
    {
        [Command("ping"), Summary("Returns \"Pong!\" if the bot is alive.")]
        public async Task Ping()
        {
            if (!GlobalMethods.authenticateCaller(new Developer(), Context.Message.Author)) throw new AuthenticationException("Requires Developer or higher.");
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithDescription("Pong!");
            await ReplyAsync(null, false, embedBuilder);
        }
    }
}
