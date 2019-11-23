using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Synapse_UI_WPF.Interfaces
{
    public static class CInterface
    {
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static uint InteractPtr;

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern uint LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("SynapseInjector.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private static extern void SynInject(uint Key, [MarshalAs(UnmanagedType.LPWStr)] string Dll, [MarshalAs(UnmanagedType.LPWStr)] string D3DComp, [MarshalAs(UnmanagedType.LPWStr)] string XInput, uint ProcId, bool AutoLaunch);

        [DllImport("SynapseInjector.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr SynHwidGrab(uint Key);

        [DllImport("SynapseInjector.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr SynSignRequest(uint Key, [MarshalAs(UnmanagedType.LPStr)] string Str);

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static uint GetKey(uint Func)
        {
            var Addr = InteractPtr;
            Addr ^= RotateRight(0x3b53904e, Convert.ToInt32(Addr % 16));
            return RotateLeft(Addr, Convert.ToInt32(Func % 16));
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void Init()
        {
            InteractPtr = LoadLibrary("bin\\SynapseInjector.dll");
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static void Inject(string Path, string D3DPath, string XInputPath, int Proc, bool AutoLaunch)
        {
            SynInject(GetKey(0x5156f544), Path, D3DPath, XInputPath, Convert.ToUInt32(Proc), AutoLaunch);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static string GetHwid()
        {
            var Str = SynHwidGrab(GetKey(0x74fbe312));
            return Marshal.PtrToStringAnsi(Str);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static string Sign(string Info)
        {
            var Str = SynSignRequest(GetKey(0x6f4a4a89), Info);
            return Marshal.PtrToStringAnsi(Str);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static uint RotateRight(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
    }
}
