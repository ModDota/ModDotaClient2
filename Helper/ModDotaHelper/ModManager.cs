using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDotaHelper
{
    /// <summary>
    /// One of the two big features of ModDotaHelper is the mod manager.
    /// This basically uses Pdub's gameinfo-based override method to set
    /// a new vpk as an override for your existing vpk.
    /// </summary>
    class ModManager
    {

        /// <summary>
        /// Checks gameinfo.txt (and later gameinfo.gi) to make sure that we are
        /// actually overriding the vpk, and our changes haven't been nuked by a
        /// "verify local game cache" use.
        /// </summary>
        void CheckGameInfo()
        {
        }
        /// <summary>
        /// Validate the installed content against the information from ModDota.
        /// This should fix a variety of issues related to bad downloads.
        /// </summary>
        /// <param name="deepverify">Whether or not we should CRC all mod files to verify them.</param>
        void ValidateInstalledMods(bool deepverify)
        {
            // Make sure that we're in gameinfo
            CheckGameInfo();
        }
    }
}
