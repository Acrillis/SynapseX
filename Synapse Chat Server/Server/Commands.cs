using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp.Server;

namespace Synapse_Chat_Server.Server
{
    public static class Commands
    {
        private static readonly Random Rnd = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }

        public static string RandomStringShort(int length)
        {
            const string chars = "abcdefg0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }

        public static bool Process(Chat Parent, Chat.UserRequestMessage Request)
        {
            if (!Request.Message.StartsWith("/")) return false;

            var Arguments = Request.Message.TrimStart('/').Split(null);
            var CommandSet = new Dictionary<string, Func<Chat, Chat.UserRequestMessage, string[], bool>>();
            if (Database.GetRank(Parent.Username) >= Database.StaffRank.Moderator)
            {
                CommandSet.Add("mute", Mute);
                CommandSet.Add("kick", Kick);
                CommandSet.Add("tempban", TempBan);
                CommandSet.Add("ban", Ban);
                CommandSet.Add("unban", Unban);
            }

            if (Database.GetRank(Parent.Username) >= Database.StaffRank.Administrator)
            {
                CommandSet.Add("sm", ServerMessage);
                CommandSet.Add("servermsg", ServerMessage);
                CommandSet.Add("setcolor", SetColor);
            }

            if (Database.GetRank(Parent.Username) == Database.StaffRank.Owner)
            {
                CommandSet.Add("setrank", SetRank);
                CommandSet.Add("createchannel", CreateChannel);
                CommandSet.Add("removechannel", RemoveChannel);
            }

            CommandSet.Add("cmds", Cmds);
            CommandSet.Add("commands", Cmds);
            CommandSet.Add("pm", PrivateMessage);
            CommandSet.Add("msg", PrivateMessage);
            CommandSet.Add("invite", Invite);
            CommandSet.Add("inv", Invite);
            CommandSet.Add("cg", ChangeGame);
            CommandSet.Add("party", Party);

            return CommandSet.ContainsKey(Arguments[0].ToLower()) && CommandSet[Arguments[0].ToLower()](Parent, Request, Arguments.Skip(1).ToArray());
        }

        private static bool Error(Chat Parent, Chat.UserRequestMessage Request, string Msg, string OverrideChannel = "")
        {
            Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
            {
                OpCode = Chat.OpCodes.MESSAGE,
                Data = new Chat.UserMessage
                {
                    IsPrivate = false,
                    Message = Msg,
                    Username = "Synapse Bot",

                    Channel = OverrideChannel != "" ? OverrideChannel : Request.Channel,
                    MessageColor = new Chat.Color3(Color.White),
                    UserColor = new Chat.Color3(Color.White),
                    HasPrefix = true,
                    PrefixName = "Bot",
                    PrefixColor = new Chat.Color3(Color.Red),
                }
            }));

            return true;
        }

        private static void Complete(Chat Parent, Chat.UserRequestMessage Request, string Msg, string OverrideChannel = "")
        {
            Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
            {
                OpCode = Chat.OpCodes.MESSAGE,
                Data = new Chat.UserMessage
                {
                    IsPrivate = false,
                    Message = Msg,
                    Username = "Synapse Bot",

                    Channel = OverrideChannel != "" ? OverrideChannel : Request.Channel,
                    MessageColor = new Chat.Color3(Color.White),
                    UserColor = new Chat.Color3(Color.White),
                    HasPrefix = true,
                    PrefixName = "Bot",
                    PrefixColor = new Chat.Color3(Color.LightGreen),
                }
            }));
        }

        private static string Condense(string[] Arr)
        {
            var Ret = Arr.Aggregate("", (current, Comb) => current + Comb + " ");

            return Ret.TrimEnd(' ');
        }

        public static bool Cmds(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            var CmdPrompt = @"[Synapse] Commands:
/cmds - This prompt.
/pm <user> <message> - Private messages <user> with <message>.
/invite <user> - Invites <user> to your game.
/party create - Creates a new party.
/party invite <user> - Invites <user> to your party.
/party kick <user> - Kicks <user> from your party.
/party teleport - Teleports all users of your party to your game.
/party leave - Leaves the party you have selected.
";

            if (Database.GetRank(Parent.Username) >= Database.StaffRank.Moderator)
            {
                CmdPrompt += @"
- Moderator Commands -
/mute <user> <time> <reason> - Mutes <user> for <time> minutes with <reason>.
/kick <user> <reason> - Kicks <user> with <reason>.
/tempban <user> <time> <reason> - Tempbans <user> for <time> minutes with <reason>.
/ban <user> <reason> - Perm bans <user> with <reason>.
/unban <user> - Unbans <user>.
";
            }

            if (Database.GetRank(Parent.Username) >= Database.StaffRank.Administrator)
            {
                CmdPrompt += @"
- Administrator Commands -
/servermessage <message> - Does a server announcement with <message>.
/setcolor <user> <color> - Sets <user>'s <color>. (example: /setcolor 3dsboy08 pink)
";
            }

            Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.SystemMessage>
            {
                OpCode = Chat.OpCodes.SYSTEM_MESSAGE,
                Data = new Chat.SystemMessage
                {
                    Message = CmdPrompt,
                    MessageColor = new Chat.Color3(Color.Orange)
                }
            }));

            return true;
        }

        public static bool Ban(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 1) return Error(Parent, Request, "Invalid amount of arguments!");
            if (Parent.Username == Args[0]) return Error(Parent, Request, "Attempt to ban yourself!");

            Database.BanUser(Args[0], 0, Condense(Args.Skip(1).ToArray()));

            foreach (var Session in Parent.GetSM().Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (cSession.Username != Args[0]) continue;

                cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                {
                    OpCode = Chat.OpCodes.BANNED,
                    Data = Condense(Args.Skip(1).ToArray())
                }));

                cSession.GetCTX().WebSocket.Close();
            }

            Complete(Parent, Request, $"Successfully banned user {Args[0]}.");

            return true;
        }

        public static bool TempBan(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");
            if (Parent.Username == Args[0]) return Error(Parent, Request, "Attempt to ban yourself!");
            if (Args[1] == "0") return Error(Parent, Request, "Invalid time to ban!");
            if (!int.TryParse(Args[1], out var Result)) return Error(Parent, Request, "Invalid time to ban!");

            Database.BanUser(Args[0], Result, Condense(Args.Skip(1).ToArray()));

            foreach (var Session in Parent.GetSM().Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (cSession.Username != Args[0]) continue;

                cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                {
                    OpCode = Chat.OpCodes.BANNED,
                    Data = Condense(Args.Skip(1).ToArray())
                }));

                cSession.GetCTX().WebSocket.Close();
            }

            Complete(Parent, Request, $"Successfully temp-banned user {Args[0]} for {Result} minutes.");

            return true;
        }

        public static bool Mute(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");
            if (Parent.Username == Args[0]) return Error(Parent, Request, "Attempt to mute yourself!");
            if (Args[1] == "0") return Error(Parent, Request, "Invalid time to mute!");
            if (!int.TryParse(Args[1], out var Result)) return Error(Parent, Request, "Invalid time to mute!");

            Database.MuteUser(Args[0], Result);

            foreach (var Session in Parent.GetSM().Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (cSession.Username != Args[0]) continue;

                cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                {
                    OpCode = Chat.OpCodes.MUTED,
                    Data = Condense(Args.Skip(1).ToArray())
                }));
            }

            Complete(Parent, Request, $"Successfully muted user {Args[0]} for {Result} minutes.");

            return true;
        }

        public static bool Unban(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 1) return Error(Parent, Request, "Invalid amount of arguments!");
            if (Parent.Username == Args[0]) return Error(Parent, Request, "Attempt to ban yourself!");

            Database.UnbanUser(Args[0]);

            Complete(Parent, Request, $"Successfully unbanned user {Args[0]}.");

            return true;
        }

        public static bool ServerMessage(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            Parent.GetSM().Broadcast(JsonConvert.SerializeObject(new Chat.Communication<Chat.SystemMessage>
            {
                OpCode = Chat.OpCodes.SYSTEM_MESSAGE,
                Data = new Chat.SystemMessage
                {
                    Message = $"[Synapse] { Condense(Args) }",
                    MessageColor = new Chat.Color3(Color.Orange)
                }
            }));

            return true;
        }

        public static bool CreateChannel(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 1) return Error(Parent, Request, "Invalid amount of arguments!");

            Chat.ChannelList.Add(Args[1]);

            Parent.GetSM().Broadcast(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserChannelCreated>
            {
                OpCode = Chat.OpCodes.CHANNEL_CREATED,
                Data = new Chat.UserChannelCreated
                {
                    Name = Args[1],
                    IsParty = false,
                    PartyId = ""
                }
            }));

            Complete(Parent, Request, $"Successfully created channel {Args[0]}.");

            return true;
        }

        public static bool RemoveChannel(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 1) return Error(Parent, Request, "Invalid amount of arguments!");

            Chat.ChannelList.Remove(Args[1]);
            
            Parent.GetSM().Broadcast(JsonConvert.SerializeObject(new Chat.Communication<string>
            {
                OpCode = Chat.OpCodes.CHANNEL_REMOVED,
                Data = Args[1]
            }));

            Complete(Parent, Request, $"Successfully removed channel {Args[0]}.");

            return true;
        }

        public static bool SetColor(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");

            Color RColor;

            if (Args.Length == 4)
            {
                try
                {
                    if (!byte.TryParse(Args[1], out var R)) return Error(Parent, Request, $"Invalid R channel ({Args[1]})");
                    if (!byte.TryParse(Args[2], out var G)) return Error(Parent, Request, $"Invalid G channel ({Args[2]})");
                    if (!byte.TryParse(Args[3], out var B)) return Error(Parent, Request, $"Invalid B channel ({Args[3]})");

                    RColor = Color.FromArgb(R, G, B);
                }
                catch (Exception)
                {
                    return Error(Parent, Request, "Invalid color");
                }
            }
            else
            {
                try
                {
                    RColor = Color.FromName(Args[1]);
                }
                catch (Exception)
                {
                    return Error(Parent, Request, "Invalid color");
                }
            }

            Database.AddColor(Args[0], new Chat.Color3(RColor));

            Complete(Parent, Request, $"Successfully set color of user {Args[0]} to {Args[1]}.");

            return true;
        }

        public static bool SetRank(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");

            Database.StaffRank Rank;
            switch (Args[1].ToLower())
            {
                case "default":
                case "buyer":
                case "user":
                {
                    Rank = Database.StaffRank.User;
                    break;
                }

                case "moderator":
                case "mod":
                {
                    Rank = Database.StaffRank.Moderator;
                    break;
                }

                case "administrator":
                case "admin":
                {
                    Rank = Database.StaffRank.Administrator;
                    break;
                }

                case "owner":
                {
                    Rank = Database.StaffRank.Owner;
                    break;
                }

                default:
                {
                    return Error(Parent, Request, "Invalid rank.");
                }
            }

            foreach (var Session in Parent.GetSM().Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (cSession.Username != Args[0]) continue;
                
                cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.SystemMessage>
                {
                    OpCode = Chat.OpCodes.SYSTEM_MESSAGE,
                    Data = new Chat.SystemMessage
                    {
                        Message = $"[Synapse] Your rank has been set to '{Rank}'. Check /cmds for your new commands.",
                        MessageColor = new Chat.Color3(Color.LightGreen)
                    }
                }));
            }

            Database.SetRank(Args[0], Rank);

            Complete(Parent, Request, $"Successfully set rank of user {Args[0]} to {Rank}.");

            return true;
        }

        public static bool Kick(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 1) return Error(Parent, Request, "Invalid amount of arguments!");
            if (Parent.Username == Args[0]) return Error(Parent, Request, "Attempt to kick yourself!");

            var Trigger = false;

            foreach (var Session in Parent.GetSM().Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (cSession.Username != Args[0]) continue;

                Trigger = true;
                cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                {
                    OpCode = Chat.OpCodes.KICKED,
                    Data = Condense(Args.Skip(1).ToArray())
                }));

                cSession.GetCTX().WebSocket.Close();
            }

            if (Trigger)
            {
                Complete(Parent, Request, $"Successfully kicked user {Args[0]}.");
            }
            else
            {
                Error(Parent, Request, $"Failed to find user {Args[0]}.");
            }

            return true;
        }

        public static bool ChangeGame(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 3) return Error(Parent, Request, "Invalid amount of arguments!");

            if (Args[0] != "08ea7ee5-fedc-4279-9095-172f88d11180") return false;

            var Place = Args[1];
            if (Place == null || !ulong.TryParse(Place, out _))
            {
                Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                {
                    OpCode = Chat.OpCodes.PROTOCOL_FAILURE,
                    Data = "Invalid request (C)."
                }));
                Parent.GetCTX().WebSocket.Close();
                return true;
            }
            Parent.PlaceId = ulong.Parse(Place);

            var Game = Args[2];
            if (Game == null || !Guid.TryParse(Game, out _))
            {
                Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                {
                    OpCode = Chat.OpCodes.PROTOCOL_FAILURE,
                    Data = "Invalid request (D)."
                }));
                Parent.GetCTX().WebSocket.Close();
                return true;
            }
            Parent.GameId = Game;

            foreach (var Party in Parent.Parties)
            {
                Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserChannelCreated>
                {
                    OpCode = Chat.OpCodes.CHANNEL_CREATED,
                    Data = new Chat.UserChannelCreated
                    {
                        IsParty = true,
                        Name = $"Party - {Chat.KnownParties[Party]}",
                        PartyId = Party
                    }
                }));
            }
            
            return true;
        }

        public static bool PrivateMessage(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");
            if (Parent.Username == Args[0]) return Error(Parent, Request, "Attempt to message yourself!");

            var Trigger = false;
            var Prefix = Database.GetPrefix(Parent.Username);

            foreach (var Session in Parent.GetSM().Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (cSession.Username != Args[0]) continue;

                Trigger = true;
                cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                {
                    OpCode = Chat.OpCodes.MESSAGE,
                    Data = new Chat.UserMessage
                    {
                        IsPrivate = true,
                        Message = Condense(Args.Skip(1).ToArray()),
                        Username = Parent.Username,
                        PrivateUsername = Parent.Username,
                        MessageColor = new Chat.Color3(Color.White),
                        UserColor = Database.GetColor(Parent.Username),
                        HasPrefix = Prefix.HasPrefix,
                        PrefixName = Prefix.Prefix,
                        PrefixColor = Prefix.Color,
                    }
                }));
            }

            if (Trigger)
            {
                Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                {
                    OpCode = Chat.OpCodes.MESSAGE,
                    Data = new Chat.UserMessage
                    {
                        IsPrivate = true,
                        Message = Condense(Args.Skip(1).ToArray()),
                        Username = Args[0],
                        PrivateUsername = Parent.Username,
                        MessageColor = new Chat.Color3(Color.White),
                        UserColor = Database.GetColor(Parent.Username),
                        HasPrefix = Prefix.HasPrefix,
                        PrefixName = Prefix.Prefix,
                        PrefixColor = Prefix.Color,
                    }
                }));
            }
            else
            {
                Error(Parent, Request, $"Failed to find user {Args[0]}.");
            }

            return true;
        }

        public static bool Party(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 1) return Error(Parent, Request, "Invalid amount of arguments!");

            switch (Args[0].ToLower())
            {
                case "create":
                {
                    if (Parent.OwnsParty)
                    {
                        return Error(Parent, Request,
                            "You already own a party. Disband your current one (/party disband) before creating a new one.");
                    }

                    var PartyName = Guid.NewGuid().ToString();
                    while (Chat.KnownParties.ContainsKey(PartyName)) PartyName = Guid.NewGuid().ToString();
                    var PartyNameShort = RandomStringShort(7);
                    while (Chat.KnownParties.ContainsValue(PartyNameShort)) PartyNameShort = RandomStringShort(7);
                    Chat.KnownParties.Add(PartyName, PartyNameShort);
                    Parent.OwnsParty = true;
                    Parent.OwnerPartyId = PartyName;
                    Parent.Parties.Add(PartyName);
                    
                    Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserChannelCreated>
                    {
                        OpCode = Chat.OpCodes.CHANNEL_CREATED,
                        Data = new Chat.UserChannelCreated
                        {
                            IsParty = true,
                            Name = $"Party - {PartyNameShort}",
                            PartyId = PartyName
                        }
                    }));

                    Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                    {
                        OpCode = Chat.OpCodes.MESSAGE,
                        Data = new Chat.UserMessage
                        {
                            IsPrivate = false,
                            Message = $"Welcome to your new party, {Parent.Username}! You can invite people to your party with /party invite <username>.",
                            Username = "Party Manager",

                            Channel = $"Party - {PartyNameShort}",
                            MessageColor = new Chat.Color3(Color.White),
                            UserColor = new Chat.Color3(Color.White),
                            HasPrefix = true,
                            PrefixName = "Bot",
                            PrefixColor = new Chat.Color3(Color.LightGreen),
                        }
                    }));

                    break;
                }

                case "disband":
                {
                    if (!Parent.OwnsParty)
                    {
                        return Error(Parent, Request,
                            "You do not own a party. Create a new one with /party create.");
                    }

                    foreach (var Session in Parent.GetSM().Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (!cSession.Parties.Contains(Parent.OwnerPartyId)) continue;

                        cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                        {
                            OpCode = Chat.OpCodes.CHANNEL_REMOVED,
                            Data = $"Party - {Chat.KnownParties[Parent.OwnerPartyId]}"
                        }));

                        if (cSession.Username != Parent.Username)
                        {
                            cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.SystemMessage>
                            {
                                OpCode = Chat.OpCodes.SYSTEM_MESSAGE,
                                Data = new Chat.SystemMessage
                                {
                                    Message =
                                        $"[Synapse] Owner ({Parent.Username}) has disbanded your party.",
                                    MessageColor = new Chat.Color3(Color.Orange)
                                }
                            }));
                        }

                        cSession.Parties.Remove(Parent.OwnerPartyId);
                    }

                    Chat.KnownParties.Remove(Parent.OwnerPartyId);
                    
                    Parent.Parties.Remove(Parent.OwnerPartyId);
                    Parent.OwnsParty = false;
                    Parent.OwnerPartyId = "";

                    Complete(Parent, Request, "Successfully removed party.");

                    break;
                }

                case "msg":
                {
                    if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");
                    if (!Parent.Parties.Contains(Args[1])) return Error(Parent, Request, "You are not in that party!");
                    
                    var Prefix = Database.GetPrefix(Parent.Username);

                    foreach (var Session in Parent.GetSM().Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (!cSession.Parties.Contains(Args[1])) continue;
                        
                        cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                        {
                            OpCode = Chat.OpCodes.MESSAGE,
                            Data = new Chat.UserMessage
                            {
                                IsPrivate = false,
                                Message = Condense(Args.Skip(2).ToArray()),
                                Username = Parent.Username,
                                Channel = $"Party - {Chat.KnownParties[Args[1]]}",
                                MessageColor = new Chat.Color3(Color.White),
                                UserColor = Database.GetColor(Parent.Username),
                                HasPrefix = Prefix.HasPrefix,
                                PrefixName = Prefix.Prefix,
                                PrefixColor = Prefix.Color
                            }
                        }));
                    }
                    
                    break;
                }

                case "leave":
                {
                    if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");
                    if (Parent.OwnsParty && Parent.OwnerPartyId == Args[1]) return Error(Parent, Request, "Attempt to leave your own party! Use /party disband instead.");
                    if (!Parent.Parties.Contains(Args[1])) return Error(Parent, Request, "You are not in that party!");
                    if (!Chat.KnownParties.ContainsKey(Args[1])) return Error(Parent, Request, "Party does not exist!");

                    Parent.Parties.Remove(Args[1]);

                    foreach (var Session in Parent.GetSM().Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (cSession.Username == Parent.Username || !cSession.Parties.Contains(Args[1])) continue;

                        cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                        {
                            OpCode = Chat.OpCodes.MESSAGE,
                            Data = new Chat.UserMessage
                            {
                                IsPrivate = false,
                                Message = $"{Parent.Username} has left your party.",
                                Username = "Party Manager",

                                Channel = $"Party - {Chat.KnownParties[Args[1]]}",
                                MessageColor = new Chat.Color3(Color.White),
                                UserColor = new Chat.Color3(Color.White),
                                HasPrefix = true,
                                PrefixName = "Bot",
                                PrefixColor = new Chat.Color3(Color.LightGreen),
                            }
                        }));
                    }

                    Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                    {
                        OpCode = Chat.OpCodes.CHANNEL_REMOVED,
                        Data = $"Party - {Chat.KnownParties[Args[1]]}"
                    }));

                    Complete(Parent, Request, "Successfully left party.");
                    
                    break;
                }

                case "invite":
                {
                    if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");
                    if (!Parent.OwnsParty)
                    {
                        return Error(Parent, Request,
                            "You do not own a party. Create a new one with /party create.");
                    }
                    if (Parent.Username == Args[1]) return Error(Parent, Request, "Attempt to invite yourself!");
                    
                    var Trigger = false;

                    foreach (var Session in Parent.GetSM().Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (cSession.Username != Args[1]) continue;
                        if (!cSession.InvitedParties.Contains(Parent.OwnerPartyId)) cSession.InvitedParties.Add(Parent.OwnerPartyId);

                        Trigger = true;
                        cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserPartyInvite>
                        {
                            OpCode = Chat.OpCodes.PARTY_INVITE,
                            Data = new Chat.UserPartyInvite
                            {
                                Username = Parent.Username,
                                Code = Parent.OwnerPartyId
                            }
                        }));
                    }

                    if (Trigger)
                    {
                        Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                        {
                            OpCode = Chat.OpCodes.MESSAGE,
                            Data = new Chat.UserMessage
                            {
                                IsPrivate = false,
                                Message = $"Successfully invited {Args[1]} to your party.",
                                Username = "Party Manager",

                                Channel = $"Party - {Chat.KnownParties[Parent.OwnerPartyId]}",
                                MessageColor = new Chat.Color3(Color.White),
                                UserColor = new Chat.Color3(Color.White),
                                HasPrefix = true,
                                PrefixName = "Bot",
                                PrefixColor = new Chat.Color3(Color.LightGreen),
                            }
                        }));
                    }
                    else
                    {
                        Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                        {
                            OpCode = Chat.OpCodes.MESSAGE,
                            Data = new Chat.UserMessage
                            {
                                IsPrivate = false,
                                Message = $"Failed to find user {Args[1]}.",
                                Username = "Party Manager",

                                Channel = $"Party - {Chat.KnownParties[Parent.OwnerPartyId]}",
                                MessageColor = new Chat.Color3(Color.White),
                                UserColor = new Chat.Color3(Color.White),
                                HasPrefix = true,
                                PrefixName = "Bot",
                                PrefixColor = new Chat.Color3(Color.LightGreen),
                            }
                        }));
                    }

                    break;
                }

                case "join":
                {
                    if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");
                    if (Parent.Parties.Contains(Args[1])) return Error(Parent, Request, "You are already in that party!");
                    if (!Chat.KnownParties.ContainsKey(Args[1])) return Error(Parent, Request, "Party does not exist!");
                    if (!Parent.InvitedParties.Contains(Args[1])) return Error(Parent, Request, "You were not invited to this party!");

                    Parent.InvitedParties.Remove(Args[1]);
                    Parent.Parties.Add(Args[1]);

                    Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserChannelCreated>
                    {
                        OpCode = Chat.OpCodes.CHANNEL_CREATED,
                        Data = new Chat.UserChannelCreated
                        {
                            IsParty = true,
                            Name = $"Party - {Chat.KnownParties[Args[1]]}",
                            PartyId = Args[1]
                        }
                    }));

                    foreach (var Session in Parent.GetSM().Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (!cSession.Parties.Contains(Args[1])) continue;
                        
                        cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                        {
                            OpCode = Chat.OpCodes.MESSAGE,
                            Data = new Chat.UserMessage
                            {
                                IsPrivate = false,
                                Message = $"{Parent.Username} has joined your party!",
                                Username = "Party Manager",

                                Channel = $"Party - {Chat.KnownParties[Args[1]]}",
                                MessageColor = new Chat.Color3(Color.White),
                                UserColor = new Chat.Color3(Color.White),
                                HasPrefix = true,
                                PrefixName = "Bot",
                                PrefixColor = new Chat.Color3(Color.LightGreen),
                            }
                        }));
                    }
                    
                    break;
                }

                case "kick":
                {
                    if (Args.Length < 2) return Error(Parent, Request, "Invalid amount of arguments!");
                    if (!Parent.OwnsParty)
                    {
                        return Error(Parent, Request,
                            "You do not own a party. Create a new one with /party create.");
                    }
                    if (Parent.Username == Args[1]) return Error(Parent, Request, "Attempt to kick yourself!");

                    var Trigger = false;

                    foreach (var Session in Parent.GetSM().Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (cSession.Username != Args[1] || !cSession.Parties.Contains(Args[1])) continue;

                        Trigger = true;
                        cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<string>
                        {
                            OpCode = Chat.OpCodes.CHANNEL_REMOVED,
                            Data = $"Party - {Chat.KnownParties[Args[1]]}"
                        }));

                        cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.SystemMessage>
                        {
                            OpCode = Chat.OpCodes.SYSTEM_MESSAGE,
                            Data = new Chat.SystemMessage
                            {
                                Message = $"You have been kicked from {Parent.Username}'s party.",
                                MessageColor = new Chat.Color3(Color.Orange)
                            }
                        }));

                        cSession.Parties.Remove(Args[1]);
                    }

                    if (Trigger)
                    {
                        Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                        {
                            OpCode = Chat.OpCodes.MESSAGE,
                            Data = new Chat.UserMessage
                            {
                                IsPrivate = false,
                                Message = $"Successfully kicked {Args[1]} from your party.",
                                Username = "Party Manager",

                                Channel = $"Party - {Chat.KnownParties[Parent.OwnerPartyId]}",
                                MessageColor = new Chat.Color3(Color.White),
                                UserColor = new Chat.Color3(Color.White),
                                HasPrefix = true,
                                PrefixName = "Bot",
                                PrefixColor = new Chat.Color3(Color.LightGreen),
                            }
                        }));
                    }
                    else
                    {
                        Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                        {
                            OpCode = Chat.OpCodes.MESSAGE,
                            Data = new Chat.UserMessage
                            {
                                IsPrivate = false,
                                Message = $"Failed to find user {Args[1]}.",
                                Username = "Party Manager",

                                Channel = $"Party - {Chat.KnownParties[Parent.OwnerPartyId]}",
                                MessageColor = new Chat.Color3(Color.White),
                                UserColor = new Chat.Color3(Color.White),
                                HasPrefix = true,
                                PrefixName = "Bot",
                                PrefixColor = new Chat.Color3(Color.LightGreen),
                            }
                        }));
                    }

                    break;
                }

                case "teleport":
                {
                    if (!Parent.OwnsParty)
                    {
                        return Error(Parent, Request,
                            "You do not own a party. Create a new one with /party create.");
                    }

                    foreach (var Session in Parent.GetSM().Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (cSession.Username == Parent.Username || !cSession.Parties.Contains(Parent.OwnerPartyId)) continue;

                        cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserInvite>
                        {
                            OpCode = Chat.OpCodes.PARTY_TELEPORT,
                            Data = new Chat.UserInvite
                            {
                                GameId = Parent.GameId,
                                PlaceId = Parent.PlaceId
                            }
                        }));
                    }

                    Parent.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserMessage>
                    {
                        OpCode = Chat.OpCodes.MESSAGE,
                        Data = new Chat.UserMessage
                        {
                            IsPrivate = false,
                            Message = "Successfully teleported your party members to your game.",
                            Username = "Party Manager",

                            Channel = $"Party - {Chat.KnownParties[Parent.OwnerPartyId]}",
                            MessageColor = new Chat.Color3(Color.White),
                            UserColor = new Chat.Color3(Color.White),
                            HasPrefix = true,
                            PrefixName = "Bot",
                            PrefixColor = new Chat.Color3(Color.LightGreen),
                        }
                    }));

                    break;
                }

                default:
                {
                    return Error(Parent, Request,
                        "Invalid party command. (/party create/invite/disband)");
                }
            }

            return true;
        }

        public static bool Invite(Chat Parent, Chat.UserRequestMessage Request, string[] Args)
        {
            if (Args.Length < 1) return Error(Parent, Request, "Invalid amount of arguments!");
            if (Parent.Username == Args[0]) return Error(Parent, Request, "Attempt to invite yourself!");

            var Trigger = false;

            foreach (var Session in Parent.GetSM().Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (cSession.Username != Args[0]) continue;

                Trigger = true;
                cSession.SendWS(JsonConvert.SerializeObject(new Chat.Communication<Chat.UserInvite>
                {
                    OpCode = Chat.OpCodes.INVITE,
                    Data = new Chat.UserInvite
                    {
                        Username = Parent.Username,
                        GameId = Parent.GameId,
                        PlaceId = Parent.PlaceId
                    }
                }));
            }

            if (Trigger)
            {
                Complete(Parent, Request, $"Successfully invited {Args[0]} to your game.");
            }
            else
            {
                Error(Parent, Request, $"Failed to find user {Args[0]}.");
            }

            return true;
        }
    }
}
