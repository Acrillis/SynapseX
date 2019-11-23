using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using Synapse_UI_WPF.Static;

namespace Synapse_UI_WPF.Interfaces
{
    public static class WebInterface
    {
        public enum WhitelistCheckResult
        {
            OK,
            NO_RESULTS,
            UNAUTHORIZED_HWID,
            EXPIRED_LICENCE,
            INVALID_REQUEST,
            UNKNOWN
        }

        public enum MigrationResult
        {
            OK,
            INVALID_USER_PASS,
            ALREADY_EXISTING_ACCOUNT,
            INVALID_REQUEST,
            UNKNOWN
        }

        public enum RegisterResult
        {
            OK,
            ALPHA_NUMERIC_ONLY,
            USERNAME_TAKEN,
            ALREADY_EXISTING_ACCOUNT,
            INVALID_KEY,
            INVALID_REQUEST,
            UNKNOWN
        }

        public enum LoginResult
        {
            OK,
            NOT_MIGRATED,
            INVALID_USER_PASS,
            INVALID_REQUEST,
            UNKNOWN
        }

        public enum RedeemResult
        {
            OK,
            ALREADY_UNLIMITED,
            INVALID_USERNAME,
            INVALID_KEY,
            INVALID_REQUEST,
            UNKNOWN
        }

        public enum GetUsernameResult
        {
            OK,
            INVALID_HWID,
            INVALID_REQUEST,
            UNKNOWN
        }

        public enum ChangeResult
        {
            OK,
            INVALID_TOKEN,
            EXPIRED_TOKEN,
            ALREADY_EXISTING_HWID,
            NOT_ENOUGH_TIME,
            INVALID_REQUEST,
            UNKNOWN
        }

        public enum ResetEmailResult
        {
            OK,
            NOT_ENOUGH_TIME,
            ACCOUNT_DOES_NOT_EXIST,
            INVALID_REQUEST,
            UNKNOWN
        }

        public enum ResetPasswordResult
        {
            OK,
            INVALID_KEY,
            KEY_EXPIRED,
            ACCOUNT_DOES_NOT_EXIST,
            INVALID_REQUEST,
            UNKNOWN
        }

        public class SynResetResponse
        {
            public ResetPasswordResult Result;
            public string Username;

            public SynResetResponse(ResetPasswordResult _Result, string _Username = "")
            {
                Result = _Result;
                Username = _Username;
            }
        }

        public class SynGetUsernameResponse
        {
            public GetUsernameResult Result;
            public string Username;

            public SynGetUsernameResponse(GetUsernameResult _Result, string _Username = "")
            {
                Result = _Result;
                Username = _Username;
            }
        }

        public class SynRegisterResponse
        {
            public RegisterResult Result;
            public string Token;

            public SynRegisterResponse(RegisterResult _Result, string _Token = "")
            {
                Result = _Result;
                Token = _Token;
            }
        }
        public class SynMigrationResponse
        {
            public MigrationResult Result;
            public string Token;

            public SynMigrationResponse(MigrationResult _Result, string _Token = "")
            {
                Result = _Result;
                Token = _Token;
            }
        }

        public class SynLoginResponse
        {
            public LoginResult Result;
            public string Token;

            public SynLoginResponse(LoginResult _Result, string _Token = "")
            {
                Result = _Result;
                Token = _Token;
            }
        }

        private static string Hwid;

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static string Decrypt(string Key, string IV, string Cipher)
        {
            var CipherBytes = Convert.FromBase64String(Cipher);
            var KeyBytes = Encoding.ASCII.GetBytes(Key);
            var IVBytes = Encoding.ASCII.GetBytes(IV);
            string Clear;

            if (KeyBytes.Length != 32 || IVBytes.Length != 16)
	            throw new ArgumentException("Invalid arguments");

			using (var encryptor = Aes.Create())
            {
                encryptor.Padding = PaddingMode.PKCS7;
                encryptor.Mode = CipherMode.CBC;
                encryptor.KeySize = 256;
                encryptor.Key = KeyBytes;
                encryptor.IV = IVBytes;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(CipherBytes, 0, CipherBytes.Length);
                        cs.Close();
                    }
                    Clear = Encoding.ASCII.GetString(ms.ToArray());
                }
            }

            return Clear;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static bool VerifyData(string Message, string Signature, RSAParameters publicKey)
        {
            var Success = false;
            using (var RSA = new RSACryptoServiceProvider())
            {
                var Encoder = new ASCIIEncoding();
                var BytesToVerify = Encoder.GetBytes(Message);
                var SignedBytes = Convert.FromBase64String(Signature);
                try
                {
                    RSA.ImportParameters(publicKey);
                    Success = RSA.VerifyData(BytesToVerify, CryptoConfig.MapNameToOID("SHA512"), SignedBytes);
                }
                catch (CryptographicException)
                {
                    MessageBox.Show("Failed to verify UI data. Please check your anti-virus software.",
                        "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }
                finally
                {
                    RSA.PersistKeyInCsp = false;
                }
            }
            return Success;
        }

        public static readonly Random Rnd = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void InitHwid()
        {
            Hwid = CInterface.GetHwid();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static WhitelistCheckResult Check()
        {
            var Mappings = new Dictionary<string, WhitelistCheckResult>
            {
                { "YES", WhitelistCheckResult.OK },
                { "NO1", WhitelistCheckResult.INVALID_REQUEST },
                { "NO2", WhitelistCheckResult.NO_RESULTS },
                { "NO3", WhitelistCheckResult.UNAUTHORIZED_HWID },
                { "NO4", WhitelistCheckResult.EXPIRED_LICENCE }
            };

            using (var WC = new WebClient { Proxy = null })
            {
                var Result = WC.DownloadString("https://synapse.to/whitelist/check?a=" + Hwid);
                return Mappings.ContainsKey(Result) ? Mappings[Result] : WhitelistCheckResult.UNKNOWN;
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static SynMigrationResponse Migrate(string Username, string Password)
        {
            var Mappings = new Dictionary<string, MigrationResult>
            {
                { "OK", MigrationResult.OK },
                { "ERR1", MigrationResult.INVALID_REQUEST },
                { "ERR2", MigrationResult.INVALID_USER_PASS },
                { "ERR3", MigrationResult.ALREADY_EXISTING_ACCOUNT }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/migrate", Method.POST);

            Request.AddHeader("R", CInterface.Sign(Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)) + Hwid));
            Request.AddParameter("a", Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)));
            Request.AddParameter("b", Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)));
            Request.AddParameter("c", Hwid);

            var Result = Client.Execute(Request).Content;

            return Result.Contains("OK") ? new SynMigrationResponse(MigrationResult.OK, Result.Split('|')[1]) : new SynMigrationResponse(Mappings.ContainsKey(Result) ? Mappings[Result] : MigrationResult.UNKNOWN);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static SynRegisterResponse Register(string Username, string Password, string Email, string SerialKey)
        {
            var Mappings = new Dictionary<string, RegisterResult>
            {
                { "OK", RegisterResult.OK },
                { "ERR1", RegisterResult.INVALID_REQUEST },
                { "ERR2", RegisterResult.INVALID_REQUEST },
                { "ERR3", RegisterResult.ALPHA_NUMERIC_ONLY },
                { "ERR4", RegisterResult.USERNAME_TAKEN },
                { "ERR5", RegisterResult.ALREADY_EXISTING_ACCOUNT },
                { "ERR6", RegisterResult.INVALID_KEY }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/create", Method.POST);

            Request.AddHeader("R", CInterface.Sign(Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)) + Hwid + SerialKey));
            Request.AddParameter("a", Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)));
            Request.AddParameter("b", Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)));
            Request.AddParameter("c", Hwid);
            Request.AddParameter("d", Email);
            Request.AddParameter("e", SerialKey);

            var Result = Client.Execute(Request).Content;

            return Result.Contains("OK") ? new SynRegisterResponse(RegisterResult.OK, Result.Split('|')[1]) : new SynRegisterResponse(Mappings.ContainsKey(Result) ? Mappings[Result] : RegisterResult.UNKNOWN);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static RedeemResult Redeem(string Username, string SerialKey)
        {
            var Mappings = new Dictionary<string, RedeemResult>
            {
                { "OK", RedeemResult.OK },
                { "ERR1", RedeemResult.INVALID_REQUEST },
                { "ERR2", RedeemResult.INVALID_USERNAME },
                { "ERR3", RedeemResult.INVALID_KEY },
                { "ERR4", RedeemResult.ALREADY_UNLIMITED }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/redeem", Method.POST);

            Request.AddHeader("R", CInterface.Sign(Username + SerialKey));
            Request.AddParameter("a", Username);
            Request.AddParameter("b", SerialKey);

            var Result = Client.Execute(Request).Content;

            if (Result.StartsWith("OK"))
                return RedeemResult.OK;

            return Mappings.ContainsKey(Result) ? Mappings[Result] : RedeemResult.UNKNOWN;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static SynGetUsernameResponse GetUsername()
        {
            var Mappings = new Dictionary<string, GetUsernameResult>
            {
                { "YES", GetUsernameResult.OK },
                { "NO1", GetUsernameResult.INVALID_REQUEST },
                { "NO2", GetUsernameResult.INVALID_HWID }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/getusernamefromhwid");

            Request.AddParameter("a", Hwid);

            var Result = Client.Execute(Request).Content;

            return Result.Contains("YES") ? new SynGetUsernameResponse(GetUsernameResult.OK, Result.Split('|')[1]) : new SynGetUsernameResponse(Mappings.ContainsKey(Result) ? Mappings[Result] : GetUsernameResult.UNKNOWN);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static SynLoginResponse Login(string Username, string Password)
        {
            var Mappings = new Dictionary<string, LoginResult>
            {
                { "OK", LoginResult.OK },
                { "ERR1", LoginResult.INVALID_REQUEST },
                { "ERR2", LoginResult.NOT_MIGRATED },
                { "ERR3", LoginResult.INVALID_USER_PASS }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/login", Method.POST);

            var PCUser = WindowsIdentity.GetCurrent().Name;
            var MachineUser = Environment.MachineName;

            Request.AddHeader("R", CInterface.Sign(Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)) + Convert.ToBase64String(Encoding.UTF8.GetBytes(PCUser)) + Hwid));
            Request.AddParameter("a", Convert.ToBase64String(Encoding.UTF8.GetBytes(Username)));
            Request.AddParameter("b", Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)));
            Request.AddParameter("c", Convert.ToBase64String(Encoding.UTF8.GetBytes(PCUser)));
            Request.AddParameter("d", Convert.ToBase64String(Encoding.UTF8.GetBytes(MachineUser)));
            Request.AddParameter("e", Hwid);

            var Result = Client.Execute(Request).Content;

            return Result.Contains("OK") ? new SynLoginResponse(LoginResult.OK, Result.Split('|')[1]) : new SynLoginResponse(Mappings.ContainsKey(Result) ? Mappings[Result] : LoginResult.UNKNOWN);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static ChangeResult Change(string Token)
        {
            var Mappings = new Dictionary<string, ChangeResult>
            {
                { "OK", ChangeResult.OK },
                { "ERR1", ChangeResult.INVALID_REQUEST },
                { "ERR2", ChangeResult.INVALID_TOKEN },
                { "ERR3", ChangeResult.EXPIRED_TOKEN },
                { "ERR4", ChangeResult.ALREADY_EXISTING_HWID },
                { "ERR5", ChangeResult.NOT_ENOUGH_TIME }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/change", Method.POST);

            var PCUser = WindowsIdentity.GetCurrent().Name;
            var MachineUser = Environment.MachineName;

            Request.AddHeader("R", CInterface.Sign(Convert.ToBase64String(Encoding.UTF8.GetBytes(Token)) + Hwid));
            Request.AddParameter("a", Convert.ToBase64String(Encoding.UTF8.GetBytes(Token)));
            Request.AddParameter("b", Convert.ToBase64String(Encoding.UTF8.GetBytes(PCUser)));
            Request.AddParameter("c", Convert.ToBase64String(Encoding.UTF8.GetBytes(MachineUser)));
            Request.AddParameter("d", Hwid);

            var Result = Client.Execute(Request).Content;

            return Mappings.ContainsKey(Result) ? Mappings[Result] : ChangeResult.UNKNOWN;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static ChangeResult ChangeDiscord(string Token, string DiscordId)
        {
            var Mappings = new Dictionary<string, ChangeResult>
            {
                { "OK", ChangeResult.OK },
                { "ERR1", ChangeResult.INVALID_REQUEST },
                { "ERR2", ChangeResult.INVALID_TOKEN },
                { "ERR3", ChangeResult.EXPIRED_TOKEN },
                { "ERR4", ChangeResult.NOT_ENOUGH_TIME }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/changediscord", Method.POST);

            var PCUser = WindowsIdentity.GetCurrent().Name;
            var MachineUser = Environment.MachineName;

            Request.AddHeader("R", CInterface.Sign(Convert.ToBase64String(Encoding.UTF8.GetBytes(Token)) + DiscordId));
            Request.AddParameter("a", Convert.ToBase64String(Encoding.UTF8.GetBytes(Token)));
            Request.AddParameter("b", Convert.ToBase64String(Encoding.UTF8.GetBytes(PCUser)));
            Request.AddParameter("c", Convert.ToBase64String(Encoding.UTF8.GetBytes(MachineUser)));
            Request.AddParameter("d", DiscordId);

            var Result = Client.Execute(Request).Content;

            return Mappings.ContainsKey(Result) ? Mappings[Result] : ChangeResult.UNKNOWN;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static ResetEmailResult SendResetEmail(string UsernameEmail)
        {
            var Mappings = new Dictionary<string, ResetEmailResult>
            {
                { "OK", ResetEmailResult.OK },
                { "ERR1", ResetEmailResult.INVALID_REQUEST },
                { "ERR2", ResetEmailResult.INVALID_REQUEST },
                { "ERR3", ResetEmailResult.NOT_ENOUGH_TIME },
                { "ERR4", ResetEmailResult.ACCOUNT_DOES_NOT_EXIST }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/passwordreset", Method.POST);

            Request.AddHeader("R", CInterface.Sign(Convert.ToBase64String(Encoding.UTF8.GetBytes(UsernameEmail)) + Hwid));
            Request.AddParameter("a", Convert.ToBase64String(Encoding.UTF8.GetBytes(UsernameEmail)));
            Request.AddParameter("b", Hwid);

            var Result = Client.Execute(Request).Content;

            return Mappings.ContainsKey(Result) ? Mappings[Result] : ResetEmailResult.UNKNOWN;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static SynResetResponse ResetPassword(string ResetToken, string NewPassword)
        {
            var Mappings = new Dictionary<string, ResetPasswordResult>
            {
                { "OK", ResetPasswordResult.OK },
                { "ERR1", ResetPasswordResult.INVALID_REQUEST },
                { "ERR2", ResetPasswordResult.INVALID_KEY },
                { "ERR3", ResetPasswordResult.KEY_EXPIRED },
                { "ERR4", ResetPasswordResult.ACCOUNT_DOES_NOT_EXIST }
            };

            var Client = new RestClient("https://synapse.to") { Proxy = null };
            var Request = new RestRequest("whitelist/passwordresetkey", Method.POST);

            Request.AddHeader("R", CInterface.Sign(Convert.ToBase64String(Encoding.UTF8.GetBytes(NewPassword)) + ResetToken));
            Request.AddParameter("a", Convert.ToBase64String(Encoding.UTF8.GetBytes(NewPassword)));
            Request.AddParameter("b", ResetToken);

            var Result = Client.Execute(Request).Content;

            if (Result.Contains("OK"))
            {
                return new SynResetResponse(ResetPasswordResult.OK, Result.Split('|')[1]);
            }

            return Mappings.ContainsKey(Result) ? new SynResetResponse(Mappings[Result]) : new SynResetResponse(ResetPasswordResult.UNKNOWN);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static string GetVersion()
        {
            using (var WC = new WebClient { Proxy = null })
            {
                return WC.DownloadString("https://synapse.to/whitelist/version");
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static string GetDiscordId()
        {
            using (var WC = new WebClient { Proxy = null })
            {
                return WC.DownloadString("https://synapse.to/whitelist/getdiscord?a=" + Hwid);
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static Data.WebSocketHolder GetWhitelistedDomains()
        {
            using (var WC = new WebClient { Proxy = null })
            {
                return JsonConvert.DeserializeObject<Data.WebSocketHolder>(
                    WC.DownloadString("https://cdn.synapse.to/synapsedistro/distro/websocket.json"));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Data.UIData GetUiData(Data.VerifiedContents<Data.UIData> Data)
        {
            /* stupid hack as msil encryption doesn't support generic get's (congrats babel) */
            return Data.Contents;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetUiSignature(Data.VerifiedContents<Data.UIData> Data)
        {
            /* stupid hack x2 as msil encryption doesn't support generic get's (congrats babel) */
            return Data.Signature;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static Data.UIData GetData()
        {
            var Rand = RandomString(16);
            string WebDl;

            using (var WC = new WebClient { Proxy = null })
            {
                WC.Headers.Add("R", CInterface.Sign(Hwid + Rand));
                WebDl = WC.DownloadString("https://synapse.to/whitelist/getuidata?a=" + Hwid + "&b=" + Rand);
            }

            var Dec = Decrypt("frA97aZtW1FiqMV7" + Rand, "RVXX7YzDUbTg2hFf", WebDl);
            var VerfContents = JsonConvert.DeserializeObject<Data.VerifiedContents<Data.UIData>>(Dec);

            RSAParameters RsaParams;
            using (var Rsa = new RSACryptoServiceProvider())
            {
                Rsa.FromXmlString("<RSAKeyValue><Modulus>tHc4tAP48V2bQ1ovz7LKWzoBVM7Ukb/R/cCzAlINa3yOS8I++0rBwYUBv0qdP5yWKGbAQmkINgruJKMC6EUgwz7RftCiKq8SEU0mVvuVFk99IABAYOO156aORISID+SBsSs28FYZKxHA4j1Ykt7YODj1wYeBSdqS0+e+V+vAabAE7Qsnh8VA9pPN6iPtKW9Zs6n2eGQpM1E+C8POMYIMnSrTIiVCBGMQXEgP0JUmiVlXG2CrqlECBpWR56ur8F1UFR3wcQU+Fix8l3Q1fiPIifFQkIHR8WQXa0JBLfNtQUJVYCtsR5zsoYZ+bqFmKqsjMMjqMlSbcr5XRwv3OX7/iQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
                RsaParams = Rsa.ExportParameters(false);
            }

            if (VerifyData(JsonConvert.SerializeObject(GetUiData(VerfContents)), GetUiSignature(VerfContents), RsaParams))
                return GetUiData(VerfContents);

            MessageBox.Show("Failed to verify UI data. Please check your anti-virus software.",
                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);

            return new Data.UIData();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Data.ScriptHubHolder GetScriptHubData(Data.VerifiedContents<Data.ScriptHubHolder> Data)
        {
            /* stupid hack as msil encryption doesn't support generic get's (congrats babel) */
            return Data.Contents;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetScriptHubSignature(Data.VerifiedContents<Data.ScriptHubHolder> Data)
        {
            /* stupid hack x2 as msil encryption doesn't support generic get's (congrats babel) */
            return Data.Signature;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static Data.ScriptHubHolder GetScriptHubData()
        {
            var Rand = RandomString(16);
            string WebDl;

            using (var WC = new WebClient { Proxy = null })
            {
                WC.Headers.Add("R", CInterface.Sign(Hwid + Rand));
                WebDl = WC.DownloadString("https://synapse.to/whitelist/getscripthubdata?a=" + Hwid + "&b=" + Rand);
            }

            var Dec = Decrypt("s6nnBBMt9e7Jm88Y" + Rand, "v9WGGhv1w6EolYQP", WebDl);
            var VerfContents = JsonConvert.DeserializeObject<Data.VerifiedContents<Data.ScriptHubHolder>>(Dec);

            RSAParameters RsaParams;
            using (var Rsa = new RSACryptoServiceProvider())
            {
                Rsa.FromXmlString("<RSAKeyValue><Modulus>tHc4tAP48V2bQ1ovz7LKWzoBVM7Ukb/R/cCzAlINa3yOS8I++0rBwYUBv0qdP5yWKGbAQmkINgruJKMC6EUgwz7RftCiKq8SEU0mVvuVFk99IABAYOO156aORISID+SBsSs28FYZKxHA4j1Ykt7YODj1wYeBSdqS0+e+V+vAabAE7Qsnh8VA9pPN6iPtKW9Zs6n2eGQpM1E+C8POMYIMnSrTIiVCBGMQXEgP0JUmiVlXG2CrqlECBpWR56ur8F1UFR3wcQU+Fix8l3Q1fiPIifFQkIHR8WQXa0JBLfNtQUJVYCtsR5zsoYZ+bqFmKqsjMMjqMlSbcr5XRwv3OX7/iQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
                RsaParams = Rsa.ExportParameters(false);
            }

            if (VerifyData(JsonConvert.SerializeObject(GetScriptHubData(VerfContents)), GetScriptHubSignature(VerfContents), RsaParams))
                return GetScriptHubData(VerfContents);

            MessageBox.Show("Failed to verify Script Hub data. Please check your anti-virus software.",
                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);

            return new Data.ScriptHubHolder();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static string VerifyWebsiteWithVersion(Window This)
        {
            try
            {
                var Request = (HttpWebRequest) WebRequest.Create("https://synapse.to/whitelist/version");

                if (Request.Proxy.GetProxy(new Uri("https://synapse.to/whitelist/version")).ToString() !=
                    "https://synapse.to/whitelist/version")
                {
                    This.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to verify Synapse website authenticity. Please make sure you do not have any web interception programs open and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                }

                Request.UserAgent = "synx/ui";
                var Response = (HttpWebResponse) Request.GetResponse();
                var ResponseStr = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                Response.Close();
                var Cert = Request.ServicePoint.Certificate;
                var CertX509 = new X509Certificate2(Cert);

                var CN = CertX509.Issuer;
                if (!CN.Contains("CloudFlare") && !CN.Contains("COMODO"))
                {
                    This.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to verify Synapse website authenticity. Please make sure you do not have any web interception programs open and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                }

                return ResponseStr;
            }
            catch (Exception)
            {
                return "";
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void VerifyWebsite(Window This)
        {
            try
            {
                var Request = (HttpWebRequest) WebRequest.Create("https://synapse.to/whitelist/version");

                if (Request.Proxy.GetProxy(new Uri("https://synapse.to/whitelist/version")).ToString() !=
                    "https://synapse.to/whitelist/version")
                {
                    This.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to verify Synapse website authenticity. Please make sure you do not have any web interception programs open and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                }

                Request.UserAgent = "synx/ui";
                var Response = (HttpWebResponse) Request.GetResponse();
                Response.Close();
                var Cert = Request.ServicePoint.Certificate;
                var CertX509 = new X509Certificate2(Cert);

                var CN = CertX509.Issuer;
                if (!CN.Contains("CloudFlare") && !CN.Contains("COMODO"))
                {
                    This.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Failed to verify Synapse website authenticity. Please make sure you do not have any web interception programs open and try again.",
                            "Synapse X", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(0);
                    });
                }
            }
            catch (Exception) { }
        }
    }
}
