using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace ModDotaHelper
{
    public class ModDotaHelper
    {
        /// <summary>
        /// This MRE is set when we want to close down. The various threads then signal
        /// that they're ready to close using the CountdownEvent workersactive.
        /// </summary>
        public static bool closedown = false;

        /// <summary>
        /// A counter for the number of workers active. Used when shutting down to ensure
        /// that we shut down cleanly.
        /// </summary>
        public static CountdownEvent workersactive = new CountdownEvent(0);

        /// <summary>
        /// Test mode; false in release builds.
        /// </summary>
        public static bool TestMode = false;

        /// <summary>
        /// The path to the folder containing dota.exe; this is read from the
        /// configuration file, and automagically figured out while doing the
        /// installation process.
        /// </summary>
        public static string DotaPath = null;

        /// <summary>
        /// The entry point. Note that we swap over to Run so that we can get
        /// slightly higher permissions. This may not be needed.
        /// </summary>
        static void Main()
        {
            Run();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            //get configuration data
            ReadConfig();
            //make sure we auto-start
            InitialSetup();
            //update check
            Updater.StartUpdaterThread();
        }

        /// <summary>
        /// Read the configuration file. May need to be made a bit more
        /// fail-safe,  there's a few potential exceptions not handled.
        /// </summary>
        public static void ReadConfig()
        {
            if(!File.Exists("config.txt"))
            {
                // Uh oh! We don't have a config file!
                // Quick, before they realize, generate a new config file
                KVLib.KeyValue root = new KVLib.KeyValue("config");
                KVLib.KeyValue dotadir = new KVLib.KeyValue("dotaDir");
                dotadir.Set(getDotaDir());
                root.AddChild(dotadir);
                string contents = root.ToString();
                using(StreamWriter writeto = new StreamWriter("config.txt"))
                {
                    writeto.Write(contents);
                }
                //ok, we've writen it, it's all good
            }
            using(StreamReader readfrom = new StreamReader("config.txt"))
            {
                string contents = readfrom.ReadToEnd();
                KVLib.KeyValue confignode = KVLib.KVParser.ParseKeyValueText(contents);
                foreach (KVLib.KeyValue child in confignode.Children)
                {
                    switch(child.Key) 
                    {
                        case "dotaDir":
                            DotaPath = child.GetString();
                            break;
                        default:
                            continue;
                    }
                }
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

        /// <summary>
        /// Do operations on the first time the program is run.
        /// </summary>
        public static void InitialSetup()
        {
            // Automatically start in the background
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("ModDotaHelper", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
        }
    }
   
}
