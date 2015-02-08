using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModDotaHelper
{
    static class Updater
    {
        static private Timer UpdaterThreadTimer;
        static private object temp;
        static public void StartUpdaterThread()
        {
            UpdaterThreadTimer = new Timer(UpdaterTick,temp,3000,60000);
        }
        static private void UpdaterTick(object state)
        {
            //Ok, we're in the update check thread
            //First we check for any updates to the updater application
            Version UpdaterSuggested = GetUpdaterSuggestedVersion();
            Version UpdaterInstalled = GetUpdaterInstalledVersion();
            if (UpdaterInstalled < UpdaterSuggested)
            {
                //Update the updater
                //this lets us add features later like unpacking zips, if necessary
                try {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile("https://moddota.com/mdc/ModDotaUpdater" + UpdaterSuggested.ToString() + ".exe", "ModDotaUpdater.exe");
                    }
                } catch (Exception) {
                    //we can't do much, but since we couldn't update the updater, don't try to run it
                    return;
                }
            }
            // Ok, the updater is now up-to-date. Time to check if ModDotaHelper itself needs an update
            Version HelperSuggested = GetOwnSuggestedVersion();
            Version HelperInstalled = GetOwnVersion();
            if(HelperInstalled < HelperSuggested)
            {
                //Update the helper
                //This is really just a matter of killing our current process, and starting up the updater
                //in theory, the updater will then update the helper, and start it up again
                ModDotaHelper.closedown = true;
                // Wait for all workers to stop
                ModDotaHelper.workersactive.Wait();
                // Start the updater (it'll wait a few seconds after starting)
                Process.Start("ModDotaUpdater.exe");
                // So long and thanks for all the fish
                Process.GetCurrentProcess().Kill();
            }
        }
        /// <summary>
        /// Get our own version.
        /// </summary>
        /// <returns>The Version of this executable.</returns>
        static Version GetOwnVersion()
        {
            try {
                return typeof(ModDotaHelper).Assembly.GetName().Version;
            } catch (Exception)
            {
                return new Version(-1, -1, -1, -1);
            }
        }
        /// <summary>
        /// Get the Version of ModDotaHelper that ModDota wants us to be on
        /// </summary>
        /// <returns>The Version object extracted from the file</returns>
        static Version GetOwnSuggestedVersion()
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
                // can't connect to ModDota, so don't update
                return new Version(-1, -1, -1, -1);
            }
        }
        /// <summary>
        /// Get the version of the updater currently being offered at ModDota.
        /// </summary>
        /// <returns>A Version object extracted from the file</returns>
        static Version GetUpdaterSuggestedVersion()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string s = client.DownloadString("https://moddota.com/mdc/ModDotaUpdater.version");
                    return new Version(s);
                }
            } catch (Exception)
            {
                // can't connect to ModDota, so don't update
                return new Version(-1, -1, -1, -1);
            }
        }
        /// <summary>
        /// Get the version of the updater currently installed.
        /// </summary>
        /// <returns>A Version object extracted from the updater executable's assembly information.</returns>
        static Version GetUpdaterInstalledVersion()
        {
            try
            {
                return AssemblyName.GetAssemblyName("ModDotaUpdater.exe").Version;
            } catch (Exception)
            {
                // can't open it, so force update
                return new Version(-1, -1, -1, -1);
            }
        }
    }
}
