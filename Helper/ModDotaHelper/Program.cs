using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Threading;

namespace ModDotaHelper
{
    public class Programs
    {
        /// <summary>
        /// path is the file path to "dota 2 beta", with no trailing slash
        /// </summary>

        private static CountdownEvent Closer = new CountdownEvent(1);

        private static string _dotaPath = null;


        const string DotaRegPath64 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570";
        const string DotaRegPath86 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570";


        public static bool TestMode = false;

        public static string DotaPath
        {
            get
            {
                if(_dotaPath == null)
                {
                    _dotaPath = "C:/Program Files (x86)/Steam/steamapps/common/dota 2 beta";                    

                    //Thanks to Jexah for the better path detection
                    var registryPath = Registry.GetValue(Environment.Is64BitOperatingSystem ? DotaRegPath64 : DotaRegPath86, "InstallLocation", _dotaPath);                     
                   
                    if (registryPath != null && registryPath.ToString() != String.Empty)
                    {
                        _dotaPath = registryPath.ToString();
                    }
                }

                return _dotaPath;                
            }
            set { _dotaPath = value; }
        }

        static void Main(string[] args)
        {
            Run();
        }



        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            //update check
            CheckAndUpdate();

            //Stay open without busywaiting
            Closer.Wait();
        }

        public static void CheckAndUpdate()
        {
            
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
