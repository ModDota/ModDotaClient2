using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModDotaUpdater
{
    class ModDotaUpdater
    {
        /// <summary>
        /// This is a light updater for ModDotaHelper. It may get a little bit
        /// less light in the future; ModDotaHelper can, in turn, update these
        /// functions to add functionality (for example, if the need arises to
        /// send out zip files instead of just exes)
        /// </summary>
        static void Main()
        {
            // Wait for ModDotaHelper to close down properly
            Thread.Sleep(5000);
            // All this does is check ModDotaHelper for updates, then exits
            // This is due to the inability to update your own executable cleanly in a cross-platform manner
            Version sug = GetSuggestedVersion();
            Version ins = GetInstalledVersion();
            if (ins < sug)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://moddota.com/mdc/ModDotaHelper"+sug.ToString()+".exe", "ModDotaHelper.exe");
                }
            }
            // Start the helper back up
            Process.Start("ModDotaHelper.exe");
            // So long and thanks for all the fish
            Process.GetCurrentProcess().Kill();
        }
        /// <summary>
        /// Get the version of the Helper suggested by ModDota
        /// </summary>
        /// <returns>A Version object pulled from the version page</returns>
        static Version GetSuggestedVersion()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string s = client.DownloadString("https://moddota.com/mdc/ModDotaHelper.version");
                    return new Version(s);
                }
            } catch (Exception)
            {
                return new Version();
            }
        }
        /// <summary>
        /// Get the version of the currently installed version of the Helper
        /// </summary>
        /// <returns>The Version contained inside the assembly of ModDotaHelper</returns>
        static Version GetInstalledVersion()
        {
            try
            {
                return AssemblyName.GetAssemblyName("ModDotaHelper.exe").Version;
            } catch (Exception)
            {
                // can't open it, so force update
                return new Version();
            }
        }
    }
}
