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
        /// <summary>
        /// The cryptographic signature of the file (base64-encoded)
        /// </summary>
        public string signature;
        /// <summary>
        /// Construct from a kv node, used when parsing .mod files.
        /// </summary>
        /// <param name="k"></param>
        public ModResource(KV.KeyValue k)
        {
            foreach (KV.KeyValue v in k.Children)
            {
                switch (v.Key)
                {
                    case "CRC":
                        CRC = UInt32.Parse(v.GetString());
                        break;
                    case "internalpath":
                        internalpath = v.GetString();
                        break;
                    case "downloadurl":
                        downloadurl = v.GetString();
                        break;
                    case "signature":
                        signature = v.GetString();
                        break;
                }
            }
        }
    }
}
