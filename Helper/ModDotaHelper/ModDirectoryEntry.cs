using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDotaHelper
{
    /// <summary>
    /// An entry for a mod in the mod directory.
    /// </summary>
    public class ModDirectoryEntry
    {
        /// <summary>
        /// The name of the mod; lowercase letters and underscores only
        /// </summary>
        public string name = null;
        /// <summary>
        /// The download url for the mod
        /// </summary>
        public string downloadurl = null;
        /// <summary>
        /// The version of the mod
        /// </summary>
        public Version version = new Version();
        /// <summary>
        /// The names of the mods on which this mod depends
        /// </summary>
        public List<string> dependencies = new List<string>();
        /// <summary>
        /// Whether or not a mod needs the workshop tools installed.
        /// </summary>
        public bool requiresource2 = false;
        /// <summary>
        /// Whether or not a mod is required to be installed.
        /// </summary>
        public bool core = false;
        /// <summary>
        /// Construct a ModDirectoryEntry from the stored KV info
        /// </summary>
        /// <param name="des">The keyvalue object describing the ModDirectoryEntry.</param>
        public ModDirectoryEntry(KV.KeyValue des)
        {
            name = des.Key;
            foreach (KV.KeyValue kv in des.Children)
            {
                switch (kv.Key)
                {
                    case "version":
                        version = new Version(kv.GetString());
                        break;
                    case "downloadurl":
                        downloadurl = kv.GetString();
                        break;
                    case "dependency":
                        dependencies.Add(kv.GetString());
                        break;
                    case "requiresource2":
                        requiresource2 = kv.GetBool();
                        break;
                    case "core":
                        core = kv.GetBool();
                        break;
                    default:
                        // no default rule here
                        break;
                }
            }

        }
    }
}
