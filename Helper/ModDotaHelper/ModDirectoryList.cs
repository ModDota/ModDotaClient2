using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDotaHelper
{
    class ModDirectoryList
    {
        public List<Tuple<string, Uri, Uri>> directories = new List<Tuple<string, Uri, Uri>>();
        /// <summary>
        /// Construct a ModDirectoryList from a file listing installed ModDirectories.
        /// </summary>
        /// <param name="descriptor"></param>
        public ModDirectoryList(KV.KeyValue descriptor)
        {
            foreach(KV.KeyValue node in descriptor.Children)
            {
                try
                {
                    string name = node.Key;
                    Uri directoryuri = null;
                    Uri versionuri = null;
                    foreach(KV.KeyValue prop in node.Children)
                    {
                        switch (prop.Key)
                        {
                            case "version":
                                versionuri = new Uri(prop.GetString());
                                break;
                            case "directory":
                                directoryuri = new Uri(prop.GetString());
                                break;
                            default:
                                // ignore
                                break;
                        }
                    }
                    Uri uri = new Uri(node.GetString());
                    directories.Add(new Tuple<string, Uri, Uri>(name, directoryuri,versionuri));
                } catch (Exception)
                {
                    Console.WriteLine("Encountered a poor entry in the directory list, skipping...");
                }
            }
        }
    }
}
