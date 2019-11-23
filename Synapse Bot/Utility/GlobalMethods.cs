using System;
using Discord;
using Synapse_Bot.Utility;

namespace Synapse_Bot.Utility_Methods
{
    /*
    *
    *	SYNAPSE BOT
    *	File.:	UTIL/GlobalMethods.cs
    *	Desc.:	Common methods used in most/all classes.
    *
    */

    public class GlobalMethods
    {
        public static IGuild Guild = new Init().client.GetGuild(0);

        public static IRole ClientRole = Guild.GetRole(0);
        public static IRole ModeratorRole = Guild.GetRole(0);
        public static IRole AdministratorRole = Guild.GetRole(0);
        public static IRole DeveloperRole = Guild.GetRole(0);

        public static bool authenticateCaller(UserGroup authLevel, IUser messageAuthor)
        {
            foreach (ulong RoleID in Guild.GetUserAsync(messageAuthor.Id).Result.RoleIds)
            {
                if (authLevel == new User()) return true;
                if (authLevel == new Client())
                {
                    if (RoleID == ClientRole.Id || RoleID == ModeratorRole.Id || RoleID == AdministratorRole.Id || RoleID == DeveloperRole.Id) return true;
                }
                if (authLevel == new Moderator())
                {
                    if (RoleID == ModeratorRole.Id || RoleID == AdministratorRole.Id || RoleID == DeveloperRole.Id) return true;
                }
                if (authLevel == new Administrator())
                {
                    if (RoleID == AdministratorRole.Id || RoleID == DeveloperRole.Id) return true;
                }
                if (authLevel == new Developer())
                {
                    if (RoleID == DeveloperRole.Id) return true;
                }
            }

            return false;
        }

        public static EmbedBuilder buildEmbed(UserGroup userGroup, string commandTitle)
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle(String.Format("Synapse Phoenix - {0} - {1}", userGroup.Identifier, commandTitle));
            embedBuilder.WithColor(userGroup.Color);
            return embedBuilder;
        }
    }
}
