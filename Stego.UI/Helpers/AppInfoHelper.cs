using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;
using Stego.UI.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Stego.UI.Helpers
{
    public static class AppInfoHelper
    {
        public static string Name => "Stego";
        public static string Version {
            get
            {
                try
                {
                    PackageVersion v = Package.Current.Id.Version;
                    return $"{v.Major}.{v.Minor}.{v.Build}";
                }
                catch
                {
                    // unpackaged app, use assembly version
                    Version? asm = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    return asm != null ? $"{asm.Major}.{asm.Minor}.{asm.Build}" : "Unknown";
                }
            }
        }
        public static string Description => "A user-friendly interface for steganography operations.";
        public static string Author => "Zeke Zhang";
        public static string Website => "https://github.com/sean1832/stego";

        public static string WinAppSdkDetails =>
            $"Windows App SDK {ReleaseInfo.Major}.{ReleaseInfo.Minor}";
        public static string WinAppSdkRuntimeDetails => WinAppSdkDetails + $", Windows App Runtime {RuntimeInfo.AsString}";
    }
}
