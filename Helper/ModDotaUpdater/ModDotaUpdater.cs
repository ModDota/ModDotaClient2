using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            try
            {
                if (File.Exists("ModDotaHelper.exe.new"))
                {
                    File.Move("ModDotaHelper.exe.new", "ModDotaHelper.exe");
                }
            }
            finally
            {
                // Start the helper back up
                Process.Start("ModDotaHelper.exe");
            }
        }
    }
}
