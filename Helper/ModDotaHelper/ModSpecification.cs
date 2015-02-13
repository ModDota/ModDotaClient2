using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDotaHelper
{
    /// <summary>
    /// The specification for an individual mod.
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
    }
}
