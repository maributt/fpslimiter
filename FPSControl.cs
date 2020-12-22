using Dalamud.Game;
using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Plugin;

namespace FPSLimiter
{
    public class FPSControl : IDisposable
    {

        private readonly DalamudPluginInterface pi;
        public IntPtr fpsDivider;
        private Int32 previousFpsCap;


        public FPSControl(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            var gKernalDevice = this.pi.TargetModuleScanner.GetStaticAddressFromSig("48 89 05 ?? ?? ?? ?? EB 07 48 89 3D ?? ?? ?? ?? BA");
            var fpsCapAddress = Marshal.ReadIntPtr(gKernalDevice) + 0x13C;
            this.fpsDivider = fpsCapAddress;
        }

        

        public string GetFpsDividerName(int divider = -1)
        {
            int n = Marshal.ReadInt32(this.fpsDivider);
            if (divider != -1) n = divider;
            
            switch (n) {
                case 0:
                    return "None.";
                case 1:
                case 2:
                case 3:
                case 4:
                    return $"Refresh Rate 1/{n}";
            }
            return "";
        }

        public Int32 getCurrentFpsCap()
        {
            return Marshal.ReadInt32(this.fpsDivider);
        }

        public Int32 getPreviousFpsCap()
        {
            return this.previousFpsCap;
        }

        public void writeFps(int newFpsCap)
        {
            
            if (newFpsCap == this.getCurrentFpsCap()) newFpsCap = this.previousFpsCap;

            // new          if = current, set to old
            // current      becomes old
            // previous     gets overwitten or retrieved by new

            this.previousFpsCap = this.getCurrentFpsCap();

            // this is for when things might go bad to prevent them from going too bad, hopefully
            VirtualProtect(this.fpsDivider, 1, Protection.PAGE_EXECUTE_READWRITE, out Protection oldProtection);
            //write the change
            Marshal.WriteByte(this.fpsDivider, (byte)newFpsCap);
            VirtualProtect(this.fpsDivider, 1, oldProtection, out _);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        // i stole this from bdth :) hehe
        #region Kernel32

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, Protection flNewProtect, out Protection lpflOldProtect);

        public enum Protection
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        #endregion
    }
}