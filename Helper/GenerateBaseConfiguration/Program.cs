using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Permissions;

namespace GenerateBaseConfiguration
{
    /// <summary>
    /// This program generates the base config file used by ModDotaHelper. As we
    /// pull the dota location by looking over the registry, we need to actually
    /// get UAC permission to do so.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Generate the base config file. This may require UAC, and may require a visual interface,
        /// unlike the rest of the program, which is why it's partitioned off by itself.
        /// </summary>
        public static void Main()
        {
            // Force-generate a clean config file
            KV.KeyValue root = new KV.KeyValue("config");
            KV.KeyValue dotadir = new KV.KeyValue("dotaDir");
            dotadir.Set(getDotaDir());
            root.AddChild(dotadir);
            string contents = root.ToString();
            // This is system-wide
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            using (StreamWriter writeto = new StreamWriter(path + "/config.txt"))
            {
                writeto.Write(contents);
            }
        }
        /// <summary>
        /// The x64 registry entry for the dota install location.
        /// </summary>
        const string DotaRegPath64 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570";

        /// <summary>
        /// The x86 registry entry for the dota install location.
        /// </summary>
        const string DotaRegPath86 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570";

        /// <summary>
        /// Get the dota install directory
        /// </summary>
        /// <returns>The dota install directory</returns>
        [RegistryPermissionAttribute(SecurityAction.Demand, Read = DotaRegPath64 + ";" + DotaRegPath86)]
        public static string getDotaDir()
        {
            string dotaPath = "C:/Program Files (x86)/Steam/steamapps/common/dota 2 beta";

            //Thanks to Jexah for the better path detection
            var registryPath = Registry.GetValue(Environment.Is64BitOperatingSystem ? DotaRegPath64 : DotaRegPath86, "InstallLocation", dotaPath);

            if (registryPath != null && registryPath.ToString() != String.Empty)
            {
                dotaPath = registryPath.ToString();
            }
            return dotaPath;
        }
    }
}
