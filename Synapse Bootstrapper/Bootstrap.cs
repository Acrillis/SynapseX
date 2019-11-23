#define USE_UPDATE_CHECKS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

[assembly: Obfuscation(Feature = "type renaming pattern 'SYNX'.*", Exclude = false)]
[assembly: Obfuscation(Feature = "encrypt symbol names with password BFM5yQBHku8Ar1wfZc3TU5zfwoBGyj0z", Exclude = false)]
[assembly: Obfuscation(Feature = "string encryption", Exclude = true)]

namespace Synapse_Bootstrapper
{
    public class Bootstrap
    {
        public static List<string> Directories = new List<string>
        {
            "autoexec",
            "bin",
            "scripts",
            "workspace",
            "auth"
        };

        [Serializable]
        public class SynBootstrapperData
        {
            public string UiDownload;
            public string UiHash;
            public string InjectorDownload;
            public string InjectorHash;
            public string BootstrapperVersion;
        }

        [Serializable]
        public class SynVerifiedContents<T>
        {
            public T Contents;
            public string Signature;
        }

        public const string BootstrapVersion = "3";

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private string Decrypt(string Key, string IV, string Cipher)
        {
            var CipherBytes = Convert.FromBase64String(Cipher);
            var KeyBytes = Encoding.ASCII.GetBytes(Key);
            var IVBytes = Encoding.ASCII.GetBytes(IV);
            string Clear;

            if (KeyBytes.Length != 32 || IVBytes.Length != 16)
            {
                throw new ArgumentException("Invalid arguments");
            }

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
        public static string Sha512(string Input, bool IsFile = false)
        {
            var bytes = IsFile ? File.ReadAllBytes(Input) : Encoding.ASCII.GetBytes(Input);
            using (var hash = SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);
                var hashedInputStringBuilder = new StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static byte[] Sha512Bytes(string Input, bool IsFile = false)
        {
            var bytes = IsFile ? File.ReadAllBytes(Input) : Encoding.ASCII.GetBytes(Input);
            using (var hash = SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);
                return hashedInputBytes;
            }
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public string CreateFileName(string Name)
        {
            var Sha = Sha512Bytes(Environment.MachineName + Name);
            var SubStr = Convert.ToBase64String(Sha).Substring(0, Convert.ToInt32(BitConverter.ToUInt32(Sha, 0) % 15) + 5);
            var FinalStr = new string(SubStr.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
            return FinalStr + Path.GetExtension(Name);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static bool VerifyData(string Message, string Signature, RSAParameters publicKey)
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
                    MessageBox.Show("Failed to verify bootstrapper data. Please check your anti-virus software.",
                        "Synapse X", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                finally
                {
                    RSA.PersistKeyInCsp = false;
                }
            }
            return Success;
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public void Start()
        {
            if (!Directory.Exists("autoexec") || !Directory.Exists("bin") || !Directory.Exists("scripts") || !Directory.Exists("workspace") || !Directory.Exists("auth"))
            {
                /* check with user */
                var Result = MessageBox.Show(
                    "Hi! It seems to be your first time using Synapse X. Synapse X will now download its files into the directory this executable is in.\n\nAre you sure you want to download Synapse X in this directory?",
                    "Synapse X", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (Result != DialogResult.OK) Environment.Exit(0);

                /* create directories */
                foreach (var Dir in Directories)
                {
                    if (Dir == "bin" && Directory.Exists(Dir)) Directory.Delete(Dir, true);
                    if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
                }
            }

            /* download contents */
            var WebData = "";
            using (var WC = new WebClient())
            {
                try
                {
                    WebData = WC.DownloadString("https://synapse.to/whitelist/getbootstrapdata");
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to download bootstrapper data. Please check your anti-virus software.",
                        "Synapse X", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }

            /* decrypt data */
            var Data = JsonConvert.DeserializeObject<SynVerifiedContents<SynBootstrapperData>>(
                Decrypt("U653zwLsno6GIwYhNpU1g2Hf4sYcTQp8", "1210w2EHaNLvLJiA", WebData));

            /* setup Rsa */
            RSAParameters RsaParams;
            using (var Rsa = new RSACryptoServiceProvider())
            {
                Rsa.FromXmlString("<RSAKeyValue><Modulus>tHc4tAP48V2bQ1ovz7LKWzoBVM7Ukb/R/cCzAlINa3yOS8I++0rBwYUBv0qdP5yWKGbAQmkINgruJKMC6EUgwz7RftCiKq8SEU0mVvuVFk99IABAYOO156aORISID+SBsSs28FYZKxHA4j1Ykt7YODj1wYeBSdqS0+e+V+vAabAE7Qsnh8VA9pPN6iPtKW9Zs6n2eGQpM1E+C8POMYIMnSrTIiVCBGMQXEgP0JUmiVlXG2CrqlECBpWR56ur8F1UFR3wcQU+Fix8l3Q1fiPIifFQkIHR8WQXa0JBLfNtQUJVYCtsR5zsoYZ+bqFmKqsjMMjqMlSbcr5XRwv3OX7/iQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
                RsaParams = Rsa.ExportParameters(false);
            }

            /* verify data */
            if (!VerifyData(JsonConvert.SerializeObject(Data.Contents), Data.Signature, RsaParams))
            {
                MessageBox.Show("Failed to verify bootstrapper data. Please check your anti-virus software. (1)",
                    "Synapse X", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            /* check bootstrapper version */
            if (Data.Contents.BootstrapperVersion != BootstrapVersion)
            {
                MessageBox.Show("Outdated bootstrapper! Please redownload Synapse.",
                    "Synapse X", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            /* verify hashes & download */
            const string InjectorName = "bin\\SynapseInjector.dll";
            var UiName = "bin\\" + CreateFileName("Synapse.bin");

            /* kill lingering processes */
            foreach (var Proc in Process.GetProcessesByName(CreateFileName("Synapse.bin")))
            {
                try
                {
                    Proc.Kill();
                }
                catch (Exception) { }
            }

#if USE_UPDATE_CHECKS
            using (var WC = new WebClient())
            {
                try
                {
                    if (!File.Exists(InjectorName)) WC.DownloadFile(Data.Contents.InjectorDownload, InjectorName);
                    if (!File.Exists(UiName)) WC.DownloadFile(Data.Contents.UiDownload, UiName);

                    if (Sha512(InjectorName, true) != Data.Contents.InjectorHash)
                    {
                        File.Delete(InjectorName);
                        WC.DownloadFile(Data.Contents.InjectorDownload, InjectorName);

                        if (Sha512(InjectorName, true) != Data.Contents.InjectorHash)
                        {
                            File.Delete(InjectorName);
                            MessageBox.Show("Failed to verify bootstrapper files. Please check your anti-virus software. (2)",
                                "Synapse X", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(0);
                        }
                    }

                    if (Sha512(UiName, true) != Data.Contents.UiHash)
                    {
                        File.Delete(UiName);
                        WC.DownloadFile(Data.Contents.UiDownload, UiName);

                        if (Sha512(UiName, true) != Data.Contents.UiHash)
                        {
                            File.Delete(UiName);
                            MessageBox.Show("Failed to verify bootstrapper files. Please check your anti-virus software. (3)",
                                "Synapse X", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(0);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to download bootstrapper files. Please check your anti-virus software. (4)",
                        "Synapse X", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
#endif

            /* launch Synapse UI */
            var ProcInfo = new ProcessStartInfo(UiName)
            {
                WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath),
                UseShellExecute = false
            };
            Process.Start(ProcInfo);

            /* exit */
            Environment.Exit(0);
        }

        [STAThread]
        public static void Main(string[] args) => new Bootstrap().Start();
    }
}
