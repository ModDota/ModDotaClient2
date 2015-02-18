using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using System.Reflection;
using System.Security.AccessControl;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;

namespace ModDotaHelper
{
    public class ModDotaHelper
    {
        /// <summary>
        /// This bool is true when we want to close down. The various threads
        /// then signal that they're ready to closem using the CountdownEvent
        /// workersactive so that we know when they're all done.
        /// </summary>
        public static bool closedown = false;

        /// <summary>
        /// A counter for the number of workers active. Used when shutting it
        /// down to ensure that we shut down cleanly.
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
        /// The mod manager - pretty much the meat of the application.
        /// </summary>
        public static ModManager modman = null;

        /// <summary>
        /// The entry point. Note that we swap over to Run so that we can get
        /// slightly higher permissions. This may not be needed.
        /// </summary>
        static void Main()
        {
            MessageBox.Show("Current path is " + Directory.GetCurrentDirectory());
            MessageBox.Show("Current path is " + Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            MessageBox.Show("Current path is " + Directory.GetCurrentDirectory());
            // Set the loggging up
            Console.SetOut(new StreamWriter(new FileStream("log.txt",FileMode.OpenOrCreate,FileAccess.Write)));
            Console.SetError(new StreamWriter(new FileStream("error.txt", FileMode.OpenOrCreate, FileAccess.Write)));
            Console.WriteLine("testing output");
            Console.Out.Flush();
            //get configuration data
            ReadConfig();
            //update check
            Updater.StartUpdaterThread();
            //Start the mod management
            modman = new ModManager(DotaPath, DotaPath + "/moddota/pak01");
            modman.CheckGameInfo();
            modman.ValidateInstalledMods();
            while (true) ;
        }
        /// <summary>
        /// Read the configuration file. May need to be made a bit more fail-
        /// safe,  there's a few potential exceptions not handled.
        /// </summary>
        public static void ReadConfig()
        {
            string configpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/config.txt";
            if (!File.Exists(configpath))
            {
                // Uh oh! We don't have a config file!
                // Quick, before they realize, generate a new config file!
                // By doing this approach, we can get the elevated privs needed to peek at the registry for dota' location.
                ProcessStartInfo ps = new ProcessStartInfo();
                ps.Verb = "runas";
                ps.FileName = "GenerateBaseConfiguration.exe";
                try
                {
                    Process generator = Process.Start(ps);
                    generator.WaitForExit();
                }
                catch (Win32Exception)
                {
                    //can't really do anything about it here
                }
                //ok, we've writen it, it's all good
            }
            // The file may still not exist if the generator failed
            if (File.Exists(configpath))
            {
                string contents;
                try
                {
                    using (StreamReader readfrom = new StreamReader(configpath))
                    {
                        contents = readfrom.ReadToEnd();
                    }
                }
                catch (Exception)
                {
                    // There's a lot of reasons why it might fail, so just handle it with defaults for now
                    Console.WriteLine("Failed to read config file, using default values...");
                    goto parsefailed;
                }
                KV.KeyValue confignode = null;
                KV.KeyValue[] confignodes;
                try
                {
                    confignodes = KV.KVParser.ParseAllKeyValues(contents);
                }
                catch (KV.KVParser.KeyValueParsingException)
                {
                    Console.WriteLine("Failed to parse config file, using default values...");
                    goto parsefailed;
                }
                foreach (KV.KeyValue kv in confignodes)
                {
                    if (kv.Key == "config")
                    {
                        confignode = kv;
                    }
                }
                if (confignode == null)
                {
                    Console.WriteLine("Couldn't find config node in configuration, using default values...");
                    goto parsefailed;
                }
                foreach (KV.KeyValue child in confignode.Children)
                {
                    switch (child.Key)
                    {
                        case "dotaDir":
                            DotaPath = child.GetString();
                            break;
                        default:
                            // We haven't defined anything else yet.
                            continue;
                    }
                }
                // Check that required values are set
                if (DotaPath == null)
                {
                    Console.WriteLine("Couldn't find required values in config file, using default values...");
                    goto parsefailed;
                }
            }
            else
            {
                goto parsefailed;
            }
            return;
        // PARSE FAILURE HANDLING
        parsefailed:
            // can't read the configuration, and can't generate a new one. Oh well, use defaults.
            // Default dota install dir
            DotaPath = "C:/Program Files (x86)/Steam/steamapps/common/dota 2 beta";
            return;
        }
    }
}
