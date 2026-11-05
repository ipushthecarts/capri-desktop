$ErrorActionPreference = 'Stop'
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class CapriTestDisplay {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Mode {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string device;
        public ushort spec, driver, size, extra;
        public uint fields;
        public int x, y;
        public uint orientation, fixedOutput;
        public short color, duplex, yResolution, ttOption, collate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string form;
        public ushort logPixels;
        public uint bits, width, height, flags, frequency, icmMethod, icmIntent, media, dither, reserved1, reserved2, panWidth, panHeight;
    }
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern bool EnumDisplaySettings(string device, int number, ref Mode mode);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int ChangeDisplaySettings(ref Mode mode, uint flags);
    public static string Prepare() {
        Mode current = new Mode();
        current.size = (ushort)Marshal.SizeOf(typeof(Mode));
        if (!EnumDisplaySettings(null, -1, ref current)) throw new Exception("Cannot read the test display mode.");
        if (current.width >= 2560 && current.height >= 1600) return current.width + "x" + current.height;
        for (int i = 0; ; i++) {
            Mode mode = new Mode(); mode.size = current.size;
            if (!EnumDisplaySettings(null, i, ref mode)) break;
            if (mode.width < 2560 || mode.height < 1600) continue;
            int result = ChangeDisplaySettings(ref mode, 0);
            if (result == 0) return mode.width + "x" + mode.height;
        }
        throw new Exception("The Windows smoke tests need a 2560x1600 virtual desktop; current mode is " + current.width + "x" + current.height + ".");
    }
}
'@
Write-Output ('Smoke-test desktop: ' + [CapriTestDisplay]::Prepare())
