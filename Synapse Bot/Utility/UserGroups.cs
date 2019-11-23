using Discord;

namespace Synapse_Bot.Utility
{
    /*
    *
    *	SYNAPSE BOT
    *	File.:	UTIL/UserGroups.cs
    *	Desc.:	User group type init for command struct.
    *
    */

    public class UserGroup
    {
        public string Identifier { get; set; }
        public ulong RoleID { get; set; }
        public int RankStructure { get; set; }
        public Discord.Color Color { get; set; }
    }

    public class User : UserGroup
    {
        public new static readonly string Identifier = "User";
        public new static readonly ulong RoleID = 0;
        public new static readonly int RankStructure = 0;
        public new static readonly Discord.Color Color = Color.Default;
    }

    public class Client : UserGroup
    {
        public new static readonly string Identifier = "Client";
        public new static readonly ulong RoleID = 0;
        public new static readonly int RankStructure = 1;
        public new static readonly Discord.Color Color = Color.Default;
    }

    public class Moderator : UserGroup
    {
        public new static readonly string Identifier = "Moderator";
        public new static readonly ulong RoleID = 0;
        public new static readonly int RankStructure = 2;
        public new static readonly Discord.Color Color = Color.Default;
    }

    public class Administrator : UserGroup
    {
        public new static readonly string Identifier = "Client";
        public new static readonly ulong RoleID = 0;
        public new static readonly int RankStructure = 3;
        public new static readonly Discord.Color Color = Color.Default;
    }

    public class Developer : UserGroup
    {
        public new static readonly string Identifier = "Developer";
        public new static readonly ulong RoleID = 0;
        public new static readonly int RankStructure = 4;
        public new static readonly Discord.Color Color = Color.Default;
    }
}
