using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
        /// Construct an empty ModDirectory.
        /// </summary>
        public ModDirectory()
        {
        }
        /// <summary>
        /// Construct a ModDirectory from the stored KV info
        /// </summary>
        /// <param name="des">The KV object containing the keyvalue structure as well as the signature for the moddirectory.</param>
        /// <param name="host">The host from which the ModDirectory was fetched, used for validation.</param>
        /// <exception cref="ModDotaHelper.ModDirectory.ModDirectorySignatureException">If the directory's signature is bad.</exception>
        public ModDirectory(KV.KeyValue des, string host)
        {
            bool passedvalidation = ModDotaHelper.modman.CCV.CheckSignature(des);
            if (!passedvalidation)
            {
                throw new CryptoChainValidator.SignatureException();
            }
            foreach(KV.KeyValue kv in des["body"].Children)
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
        /// <summary>
        /// Add all of the entries from the other ModDirectory to this one.
        /// </summary>
        /// <param name="other">The other ModDirectory.</param>
        public void add(ModDirectory other)
        {
            entries.AddRange(other.entries);
        }
    }
}
