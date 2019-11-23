using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace sxlib.Static
{
    public static class Utils
    {
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
        public static string Sha512Dll(string Input)
        {
            var bytes = File.ReadAllBytes(Input);
            bytes = bytes.SubArray(0, bytes.Length - 64);
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
        public static string CreateFileName(string Name)
        {
            var Sha = Sha512Bytes(Environment.MachineName + Name);
            var SubStr = Convert.ToBase64String(Sha).Substring(0, Convert.ToInt32(BitConverter.ToUInt32(Sha, 0) % 15) + 5);
            var FinalStr = new string(SubStr.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
            return FinalStr + Path.GetExtension(Name);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void AppendAllBytes(string Path, byte[] Bytes)
        {
            using (var FStream = new FileStream(Path, FileMode.Append))
            {
                FStream.Write(Bytes, 0, Bytes.Length);
            }
        }
    }
}
