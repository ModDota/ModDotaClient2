using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModDotaUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            // All this does is check ModDotaHelper for updates, then exits
            // This is due to the inability to update your own executable cleanly in a cross-platform manner
            Version sug = GetSuggestedVersion();
            Version ins = GetInstalledVersion();
            if (sug.CompareTo(ins) > 0)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://moddota.com/mdc/ModDotaHelper"+sug.ToString()+".exe", "ModDotaHelper.exe");
                }
            }
        }
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
                return new Version(-1, -1, -1, -1);
            }
        }
        static Version GetInstalledVersion()
        {
            try
            {
                return AssemblyName.GetAssemblyName("ModDotaHelper.exe").Version;
            } catch (Exception)
            {
                // can't open it, so force update
                return new Version(-1, -1, -1, -1);
            }
        }
    }
}
