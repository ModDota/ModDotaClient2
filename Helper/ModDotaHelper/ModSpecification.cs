using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ModDotaHelper
{
    /// <summary>
    /// The specification for an individual mod, as put in a .mod file.
    /// </summary>
    public class ModSpecification
    {
        /// <summary>
        /// The name of the mod; limit to lowercase letters, digits, and underscores
        /// </summary>
        public string name;
        /// <summary>
        /// A version number for the mod. Used to contol updates.
        /// </summary>
        public Version version;
        /// <summary>
        /// The files in the mod.
        /// </summary>
        public List<ModResource> files;
        /// <summary>
        /// The certificate used to sign the mod and the contained files.
        /// </summary>
        public X509Certificate2 cert;
        /// <summary>
        /// Construct a ModSpecification from the base data.
        /// </summary>
        /// <param name="sourcedata">The KeyValue from which this ModSpecification is to be constructed.</param>
        public ModSpecification(KV.KeyValue sourcedata)
        {
            if (!ModDotaHelper.modman.CCV.CheckSignature(sourcedata))
            {
                throw new CryptoChainValidator.SignatureException();
            }
            foreach(KV.KeyValue kv in sourcedata["body"].Children)
            {
                switch (kv.Key)
                {
                    case "modinfo":
                        break;
                    default:
                        files.Add(new ModResource(kv));
                        break;
                }
            }
        }
    }
}
