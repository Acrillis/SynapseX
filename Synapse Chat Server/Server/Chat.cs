using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RestSharp;
using WebSocketSharp;
using WebSocketSharp.Net.WebSockets;
using WebSocketSharp.Server;

namespace Synapse_Chat_Server.Server
{
    public class Chat : WebSocketBehavior
    {
        public string Username;
        public string GameId;
        public ulong PlaceId;
        public bool Authenticated;
        public bool OwnsParty;
        public string OwnerPartyId;
        public List<string> InvitedParties = new List<string>();
        public List<string> Parties = new List<string>();

        public static Dictionary<string, string> KnownParties = new Dictionary<string, string>();
        public static List<string> ChannelList = new List<string>
        {
            "General",
            "Bots",
            "Per-Game",
            "Per-Server"
        };

        public enum OpCodes
        {
            //Message OpCodes
            MESSAGE,
            SYSTEM_MESSAGE,

            //Action OpCodes
            INVITE,
            PARTY_INVITE,
            PARTY_TELEPORT,
            CHANNEL_CREATED,
            CHANNEL_REMOVED,

            //Auth OpCodes
            AUTH_SUCCESS,
            AUTH_FAILURE,

            //Moderation OpCodes
            MUTED,
            KICKED,
            BANNED,

            //Protocol OpCodes
            PROTOCOL_FAILURE
        }

        [Serializable]
        public class Color3
        {
            public byte R;
            public byte G;
            public byte B;

            public Color3(Color Color)
            {
                R = Color.R;
                G = Color.G;
                B = Color.B;
            }
        }

        [Serializable]
        public class UserJoin
        {
            public string Username;
            public string MOTD;
        }

        [Serializable]
        public class UserInvite
        {
            public string Username;

            public ulong PlaceId;
            public string GameId;
        }

        [Serializable]
        public class UserPartyInvite
        {
            public string Username;
            public string Code;
        }

        [Serializable]
        public class UserChannelCreated
        {
            public string Name;

            public bool IsParty;
            public string PartyId;
        }

        [Serializable]
        public class UserMessage
        {
            public bool IsPrivate;
            public string PrivateUsername;

            public bool HasPrefix;
            public string PrefixName;
            public Color3 PrefixColor;

            public string Username;
            public string Channel;
            public string Message;

            public Color3 MessageColor;
            public Color3 UserColor;
        }

        [Serializable]
        public class SystemMessage
        {
            public string Message;
            public Color3 MessageColor;
        }

        [Serializable]
        public class UserRequestMessage
        {
            public string Channel;
            public string Message;
        }

        [Serializable]
        public class Communication<T>
        {
            public OpCodes OpCode;
            public T Data;
        }

        protected override void OnOpen()
        {
            var HWID = Context.QueryString["hwid"];
            if (HWID == null || HWID.Length != 30 || !HWID.All(char.IsLetterOrDigit))
            {
                Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.PROTOCOL_FAILURE,
                    Data = "Invalid HWID."
                }));
                Context.WebSocket.Close();
                return;
            }

            var Place = Context.QueryString["pid"];
            if (Place == null || !ulong.TryParse(Place, out _))
            {
                Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.PROTOCOL_FAILURE,
                    Data = "Invalid Place ID."
                }));
                Context.WebSocket.Close();
                return;
            }
            PlaceId = ulong.Parse(Place);

            var Game = Context.QueryString["gid"];
            if (Game == null || !Guid.TryParse(Game, out _))
            {
                Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.PROTOCOL_FAILURE,
                    Data = "Invalid Game ID."
                }));
                Context.WebSocket.Close();
                return;
            }
            GameId = Game;

            Log.Info($"{HWID} connecting with IP address {Context.UserEndPoint.Address}, authenticating...");

            var RClient = new RestClient("https://synapse.to");
            var RRequest = new RestRequest("whitelist/getusernamefromhwid", Method.GET);
            RRequest.AddParameter("a", HWID);

            var Response = RClient.Execute(RRequest);

            if (!Response.IsSuccessful)
            {
                Log.Warn($"Failed to authenticate {HWID}, response not successful.");

                Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.AUTH_FAILURE,
                    Data = "Failed to authenticate with server."
                }));

                Context.WebSocket.Close();
                return;
            }

            var Split = Response.Content.Split('|');
            if (Split.Length != 2 || Split[0] != "YES")
            {
                Log.Warn($"Failed to authenticate {HWID}, invalid HWID.");

                Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.AUTH_FAILURE,
                    Data = "Failed to authenticate with server."
                }));

                Context.WebSocket.Close();
                return;
            }

            Username = Regex.Replace(Split[1], @"\s+", "");
            if (Username == "3dsboy08")
            {
                Database.SetRank(Username, Database.StaffRank.Owner);
            }

            foreach (var Session in Sessions.Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (cSession.Username != Username || cSession == this) continue;

                cSession.Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.PROTOCOL_FAILURE,
                    Data = "Logged in from a different location."
                }));

                cSession.GetCTX().WebSocket.Close();
            }

            Authenticated = true;

            if (Database.IsBanned(Username))
            {
                Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.BANNED,
                    Data = Database.GetBanMessage(Username)
                }));

                Context.WebSocket.Close();
                return;
            }

            Send(JsonConvert.SerializeObject(new Communication<UserJoin>
            {
                OpCode = OpCodes.AUTH_SUCCESS,
                Data = new UserJoin
                {
                    Username = Username,
                    MOTD = "Hello, gamers!"
                }
            }));

            Log.Info($"{HWID} ({Username}) successfully authenticated! (Game Id: {GameId})");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (!OwnsParty) return;

            foreach (var Session in Sessions.Sessions)
            {
                if (!(Session is Chat cSession)) continue;
                if (!cSession.Parties.Contains(OwnerPartyId)) continue;

                cSession.SendWS(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.CHANNEL_REMOVED,
                    Data = $"Party - {KnownParties[OwnerPartyId]}"
                }));

                cSession.SendWS(JsonConvert.SerializeObject(new Communication<SystemMessage>
                {
                    OpCode = OpCodes.SYSTEM_MESSAGE,
                    Data = new SystemMessage
                    {
                        Message =
                            $"[Synapse] Owner ({Username}) has disbanded your party. (Disconnected)",
                        MessageColor = new Color3(Color.Orange)
                    }
                }));

                cSession.Parties.Remove(OwnerPartyId);
            }

            KnownParties.Remove(OwnerPartyId);

            Parties.Remove(OwnerPartyId);
            OwnsParty = false;
            OwnerPartyId = "";
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (!Authenticated) return;
            if (Username == null) return;

            UserRequestMessage Request;
            try
            {
                Request = JsonConvert.DeserializeObject<UserRequestMessage>(e.Data);
            }
            catch (Exception)
            {
                Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.PROTOCOL_FAILURE,
                    Data = "Invalid request. (A)"
                }));

                Context.WebSocket.Close();
                return;
            }

            if (Request.Channel.Length > 50) return;
            if (Request.Message.Length > 200) return;

            if (!ChannelList.Contains(Request.Channel))
            {
                Send(JsonConvert.SerializeObject(new Communication<string>
                {
                    OpCode = OpCodes.PROTOCOL_FAILURE,
                    Data = "Invalid request. (B)"
                }));

                Context.WebSocket.Close();
                return;
            }

            if (string.IsNullOrWhiteSpace(Request.Message))
                return;

            if (Database.IsMuted(Username))
            {
                Send(JsonConvert.SerializeObject(new Communication<SystemMessage>
                {
                    OpCode = OpCodes.SYSTEM_MESSAGE,
                    Data = new SystemMessage
                    {
                        Message = "[Synapse] You are currently muted from the Synapse X chat.",
                        MessageColor = new Color3(Color.Red)
                    }
                }));

                return;
            }

            if (Database.GetRank(Username) == Database.StaffRank.User && Filter.Process(this, Request.Message))
            {
                Send(JsonConvert.SerializeObject(new Communication<SystemMessage>
                {
                    OpCode = OpCodes.SYSTEM_MESSAGE,
                    Data = new SystemMessage
                    {
                        Message = "[Synapse] Please do not try to spam the chat.",
                        MessageColor = new Color3(Color.Red)
                    }
                }));

                return;
            }

            if (Commands.Process(this, Request))
                return;

            var Prefix = Database.GetPrefix(Username);

            switch (Request.Channel)
            {
                case "Per-Game":
                {
                    foreach (var Session in Sessions.Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (cSession.PlaceId != PlaceId) continue;

                        cSession.SendWS(JsonConvert.SerializeObject(new Communication<UserMessage>
                        {
                            OpCode = OpCodes.MESSAGE,
                            Data = new UserMessage
                            {
                                IsPrivate = false,
                                Message = Request.Message,
                                Channel = Request.Channel,
                                Username = Username,
                                MessageColor = new Color3(Color.White),
                                UserColor = Database.GetColor(Username),
                                HasPrefix = Prefix.HasPrefix,
                                PrefixName = Prefix.Prefix,
                                PrefixColor = Prefix.Color
                            }
                        }));
                    }

                    return;
                }
                case "Per-Server":
                {
                    foreach (var Session in Sessions.Sessions)
                    {
                        if (!(Session is Chat cSession)) continue;
                        if (cSession.GameId != GameId) continue;
                    
                        cSession.SendWS(JsonConvert.SerializeObject(new Communication<UserMessage>
                        {
                            OpCode = OpCodes.MESSAGE,
                            Data = new UserMessage
                            {
                                IsPrivate = false,
                                Message = Request.Message,
                                Channel = Request.Channel,
                                Username = Username,
                                MessageColor = new Color3(Color.White),
                                UserColor = Database.GetColor(Username),
                                HasPrefix = Prefix.HasPrefix,
                                PrefixName = Prefix.Prefix,
                                PrefixColor = Prefix.Color
                            }
                        }));
                    }

                    return;
                }
                default:
                    Sessions.Broadcast(JsonConvert.SerializeObject(new Communication<UserMessage>
                    {
                        OpCode = OpCodes.MESSAGE,
                        Data = new UserMessage
                        {
                            IsPrivate = false,
                            Message = Request.Message,
                            Channel = Request.Channel,
                            Username = Username,
                            MessageColor = new Color3(Color.White),
                            UserColor = Database.GetColor(Username),
                            HasPrefix = Prefix.HasPrefix,
                            PrefixName = Prefix.Prefix,
                            PrefixColor = Prefix.Color
                        }
                    }));
                    break;
            }
        }

        //Lazy way to get around protected methods.
        public void SendWS(string Data)
        {
            Send(Data);
        }

        public WebSocketSessionManager GetSM()
        {
            return Sessions;
        }

        public WebSocketContext GetCTX()
        {
            return Context;
        }
    }
}
