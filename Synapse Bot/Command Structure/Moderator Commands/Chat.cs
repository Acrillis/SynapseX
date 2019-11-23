using System.Security.Authentication;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Synapse_Bot.Utility;
using Synapse_Bot.Utility_Methods;

namespace Synapse_Bot.Command_Structure.Moderator_Commands
{
    /*
    *
    *	SYNAPSE BOT
    *	File.:	C_STRUCT/MOD/Chat.cs
    *	Desc.:	Chat-based commands for MODERATORS.
    *
    */

    public class ChatModerators : ModuleBase
    {
        [Command("ping"), Summary("Returns \"Pong!\" if the bot is alive.")]
        public async Task Ping()
        {
            if (!GlobalMethods.authenticateCaller(new Moderator(), Context.Message.Author)) throw new AuthenticationException("Requires Moderator or higher.");
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithDescription("Pong!");
            await ReplyAsync(null, false, embedBuilder);
        }
    }
}
