using System.Threading.Tasks;
using Discord.Commands;
using Synapse_Bot.Utility;
using Synapse_Bot.Utility_Methods;

namespace Synapse_Bot.Command_Structure.User_Commands
{
    /*
    *
    *	SYNAPSE BOT
    *	File.:	C_STRUCT/USER/Chat.cs
    *	Desc.:	Chat-based commands for USERS.
    *
    */

    public class ChatUsers : ModuleBase
    {
        [Command("ping"), Summary("Returns \"Pong!\" if the bot is alive.")]
        public async Task Ping()
        {
            var embedBuilder = GlobalMethods.buildEmbed(new User(), "Ping");
            embedBuilder.WithDescription("Pong!");
            await ReplyAsync(null, false, embedBuilder);
        }
    }
}
