using System;
using System.IO;
using System.Runtime.InteropServices;

namespace JFN.Utilities
{
    public static class Paths
    {
        public static string GetAppDir(string appDir)
        {
            var path =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appDir)
                : Path.Combine("/var/lib/", appDir);
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
