using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDotaHelper
{
    public class ModDirectory
    {
        /// <summary>
        /// The version of the mod directory.
        /// </summary>
        public Version version = new Version();
        /// <summary>
        /// The entries (mods) in the mod directory.
        /// </summary>
        public List<ModDirectoryEntry> entries = new List<ModDirectoryEntry>();
        /// <summary>
        /// Construct a ModDirectory from the stored KV info
        /// </summary>
        /// <param name="des">The keyvalue object describing the ModDirectory.</param>
        public ModDirectory(KV.KeyValue des)
        {
            foreach(KV.KeyValue kv in des.Children)
            {
                switch(kv.Key)
                {
                    case "version":
                        version = new Version(kv.GetString());
                        break;
                    default:
                        entries.Add(new ModDirectoryEntry(kv));
                        break;
                }
            }
        }
    }
}
