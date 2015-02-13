using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDotaHelper
{
    /// <summary>
    /// The specification for a file in a mod.
    /// </summary>
    public class ModResource
    {
        /// <summary>
        /// The CRC32 hash of the file, used for quick validation.
        /// </summary>
        public UInt32 CRC;
        /// <summary>
        /// The path (including file name and extension) internally.
        /// </summary>
        public string internalpath;
        /// <summary>
        /// The url at which the file can be acquired.
        /// </summary>
        public string downloadurl;
    }
}
