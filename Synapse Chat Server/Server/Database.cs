using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Synapse_Chat_Server.Server
{
    public static class Database
    {
        public enum StaffRank
        {
            User,
            Moderator,
            Administrator,
            Owner
        }

        public class PrefixData
        {
            public bool HasPrefix;
            public string Prefix;
            public Chat.Color3 Color;
        }

        public class SynDB
        {
            public Dictionary<string, long> Bans;
            public Dictionary<string, long> Mutes;
            public Dictionary<string, string> BanMessages;
            public Dictionary<string, Chat.Color3> UserColors;
            public Dictionary<string, StaffRank> UserRanks;
        }

        private static SynDB CurrentDB;

        public static void LoadDb()
        {
            if (!File.Exists("db.json"))
            {
                CurrentDB = new SynDB { Bans = new Dictionary<string, long>(), Mutes = new Dictionary<string, long>(), BanMessages = new Dictionary<string, string>(), UserColors = new Dictionary<string, Chat.Color3>(), UserRanks = new Dictionary<string, StaffRank>()};
                File.WriteAllText("db.json", JsonConvert.SerializeObject(CurrentDB));
            }

            CurrentDB = JsonConvert.DeserializeObject<SynDB>(File.ReadAllText("db.json"));
        }

        public static void SaveDb()
        {
            File.WriteAllText("db.json", JsonConvert.SerializeObject(CurrentDB));
        }

        public static bool IsBanned(string Username)
        {
            if (!CurrentDB.Bans.ContainsKey(Username)) return false;

            if (CurrentDB.Bans[Username] == 1) return true;
            if (CurrentDB.Bans[Username] <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return true;
            CurrentDB.Bans.Remove(Username);
            CurrentDB.BanMessages.Remove(Username);
            SaveDb();
            return false;
        }

        public static bool IsMuted(string Username)
        {
            if (!CurrentDB.Mutes.ContainsKey(Username)) return false;
            
            if (CurrentDB.Mutes[Username] <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return true;
            CurrentDB.Mutes.Remove(Username);
            SaveDb();
            return false;
        }

        public static Chat.Color3 GetColor(string Username)
        {
            return !CurrentDB.UserColors.ContainsKey(Username) ? new Chat.Color3(Color.Orange) : CurrentDB.UserColors[Username];
        }

        public static StaffRank GetRank(string Username)
        {
            return !CurrentDB.UserRanks.ContainsKey(Username) ? StaffRank.User : CurrentDB.UserRanks[Username];
        }

        public static void SetRank(string Username, StaffRank Rank)
        {
            if (CurrentDB.UserRanks.ContainsKey(Username)) CurrentDB.UserRanks.Remove(Username);
            CurrentDB.UserRanks.Add(Username, Rank);
            SaveDb();
        }

        public static PrefixData GetPrefix(string Username)
        {
            if (!CurrentDB.UserRanks.ContainsKey(Username))
                return new PrefixData
                {
                    HasPrefix = false,
                    Prefix = "",
                    Color = new Chat.Color3(Color.Black)
                };

            switch (CurrentDB.UserRanks[Username])
            {
                case StaffRank.User:
                {
                    return new PrefixData
                    {
                        HasPrefix = false,
                        Prefix = "",
                        Color = new Chat.Color3(Color.Black)
                    };
                }

                case StaffRank.Moderator:
                {
                    return new PrefixData
                    {
                        HasPrefix = true,
                        Prefix = "Mod",
                        Color = new Chat.Color3(Color.SkyBlue)
                    };
                }

                case StaffRank.Administrator:
                {
                    return new PrefixData
                    {
                        HasPrefix = true,
                        Prefix = "Admin",
                        Color = new Chat.Color3(Color.IndianRed)
                    };
                }

                case StaffRank.Owner:
                {
                    return new PrefixData
                    {
                        HasPrefix = true,
                        Prefix = "Owner",
                        Color = new Chat.Color3(Color.FromArgb(214, 158, 255))
                    };
                }

                default:
                {
                    return new PrefixData
                    {
                        HasPrefix = false,
                        Prefix = "",
                        Color = new Chat.Color3(Color.Black)
                    };
                }
            }
        }

        public static void AddColor(string Username, Chat.Color3 NewColor)
        {
            if (CurrentDB.UserColors.ContainsKey(Username)) CurrentDB.UserColors.Remove(Username);
            CurrentDB.UserColors.Add(Username, NewColor);
            SaveDb();
        }

        public static string GetBanMessage(string Username)
        {
            return CurrentDB.BanMessages[Username];
        }

        public static void MuteUser(string Username, long Minutes)
        {
            if (CurrentDB.Mutes.ContainsKey(Username)) CurrentDB.Mutes.Remove(Username);
            CurrentDB.Mutes.Add(Username, DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Minutes * 60);

            SaveDb();
        }

        public static void BanUser(string Username, long Minutes, string Reason = "")
        {
            if (CurrentDB.Bans.ContainsKey(Username)) CurrentDB.Bans.Remove(Username);
            if (CurrentDB.BanMessages.ContainsKey(Username)) CurrentDB.BanMessages.Remove(Username);

            if (Minutes == 0)
            {
                CurrentDB.Bans.Add(Username, 1);
            }
            else
            {
                CurrentDB.Bans.Add(Username, DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Minutes * 60);
            }

            CurrentDB.BanMessages.Add(Username, Reason);
            SaveDb();
        }

        public static void UnbanUser(string Username)
        {
            if (CurrentDB.Bans.ContainsKey(Username)) CurrentDB.Bans.Remove(Username);
            if (CurrentDB.BanMessages.ContainsKey(Username)) CurrentDB.BanMessages.Remove(Username);
            SaveDb();
        }
    }
}
