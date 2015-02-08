using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModDotaInstaller
{
    static class Program
    {
        /// <summary>
        /// The path to the folder containing dota.exe
        /// </summary>
        static string _dotaPath = null;

        /// <summary>
        /// The x64 registry entry for the dota install location.
        /// </summary>
        const string DotaRegPath64 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570";

        /// <summary>
        /// The x86 registry entry for the dota install location.
        /// </summary>
        const string DotaRegPath86 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570";

        /// <summary>
        /// The main entry point for the application.
        /// The idea behind ModDotaInstaller is that it should handle first-time install operations,
        /// download ModDotaHelper (or unpack a version from internal storage), and make sure that a
        /// user looks over the default settings before we store them to a config file (which should
        /// solve the problem of finding a user's Dota directory in odd configurations)
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        static string GuessDotaDirectory()
        {
            _dotaPath = "C:/Program Files (x86)/Steam/steamapps/common/dota 2 beta";

            //Thanks to Jexah for the better path detection
            var registryPath = Registry.GetValue(Environment.Is64BitOperatingSystem ? DotaRegPath64 : DotaRegPath86, "InstallLocation", _dotaPath);

            if (registryPath != null && registryPath.ToString() != String.Empty)
            {
                _dotaPath = registryPath.ToString();
            }
            return _dotaPath;
        }
    }
}
