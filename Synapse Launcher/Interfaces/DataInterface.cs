using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Synapse_Launcher.Static;

namespace Synapse_Launcher.Interfaces
{
    public static class DataInterface
    {
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void Save<T>(string Name, T Data)
        {
            var Serial = JsonConvert.SerializeObject(Data);
            var Protected = ProtectedData.Protect(Encoding.UTF8.GetBytes(Serial),
                Encoding.UTF8.GetBytes(Utils.Sha512(Environment.MachineName + Name)), DataProtectionScope.LocalMachine);
            File.WriteAllText("auth\\" + Name + ".bin", Convert.ToBase64String(Protected));
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static T Read<T>(string Name)
        {
            var Unprotected = ProtectedData.Unprotect(Convert.FromBase64String(File.ReadAllText("auth\\" + Name + ".bin")),
                Encoding.UTF8.GetBytes(Utils.Sha512(Environment.MachineName + Name)), DataProtectionScope.LocalMachine);
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(Unprotected));
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static bool Exists(string Name)
        {
            return File.Exists("auth\\" + Name + ".bin");
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void Delete(string Name)
        {
            if (File.Exists("auth\\" + Name + ".bin")) File.Delete("auth\\" + Name + ".bin");
        }
    }
}
