using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ModDotaHelper
{
    /// <summary>
    /// This is a VPK class, specifically designed to be used for
    /// the multi-part VPKv1 VPKs used for Dota 2's content files
    /// and content, for purposes of overriding them.
    /// Note that this is entirely disk-backed; all changes we're
    /// making to the vpk archive are immediately pushed to disk,
    /// so that we avoid issues related to low memory, dirty file
    /// correction on crash, and so on.
    /// 
    /// </summary>
    class VPK
    {
        /// <summary>
        /// The prefix for the vpk; for example, for pak01_*.vpk, we get "pak01"
        /// </summary>
        string fileprefix = null;

        /// <summary>
        /// Used to prevent multiple access to the vpk file; it's not yet thread-safe.
        /// </summary>
        Mutex vpkmutex = new Mutex();

        /// <summary>
        /// Maximum size of an archive to make
        /// </summary>
        public static long maxarchivesize = 134217728; // 2^27 - valve generally doesn't go much over this.

        /// <summary>
        /// Alignment of files inside archives
        /// </summary>
        public static long filealignment = 1024; // 1^10 - align to kilobyte boundaries

        /// <summary>
        /// Construct a VPK object with a specific filename
        /// </summary>
        /// <param name="filename">The filename (relative) where the vpk is</param>
        public VPK(string filename)
        {
            vpkmutex.WaitOne();
            if(filename.EndsWith(".vpk"))
            {
                filename = filename.Substring(0,filename.Length-4);
            }
            if(filename.EndsWith("_dir"))
            {
                filename = filename.Substring(0,filename.Length-4);
            }
            fileprefix = filename;
            if (!File.Exists(fileprefix + "_dir.vpk"))
            {
                //we've gotta make a new vpk from scratch
                using(FileStream headfile = File.Create(fileprefix + "_dir.vpk"))
                {
                    using(BinaryWriter writer = new BinaryWriter(headfile))
                    {
                        VPKHeader head = new VPKHeader(null);
                        writer.Write(StructTools.RawSerialize(head));
                    }
                }
            }
            vpkmutex.ReleaseMutex();
        }
        /// <summary>
        /// Adds a file (already in memory) to a vpk.
        /// </summary>
        /// <param name="contents">The file contents.</param>
        /// <param name="internalfilename">The file name, including path, internally.</param>
        public void AddFile(byte[] contents, string internalfilename)
        {
            Crc32.Crc32Algorithm checker = new Crc32.Crc32Algorithm();
            byte[] crc = checker.ComputeHash(contents);
            if(crc.Length != 4)
            {
                Console.WriteLine("wtf the crc isn't 32 bits long");
            }
            VPKFile asfile = new VPKFile(null);
            asfile.CRC32 = BitConverter.ToUInt32(crc, 0);
            asfile.entry_length = (uint)(contents.Length);
            
            // Now that we have the basics of the file, we need to figure out how to store it
            // This involves first loading the vpk directory information
            List<VPKExt> dir = GetDirectory();
            // Now we need to just add the file, and write new directory information
            // Get the location to which to write the file
            Tuple<int, uint> slot = GetNewestSpace(dir);
            int paddinglength = (int)filealignment - (int)((int)(slot.Item2) % (int)filealignment);
            int archivenum = slot.Item1;
            uint offset = slot.Item2;
            // Create a new archive file if necessary
            if(contents.Length + slot.Item2 + paddinglength > maxarchivesize) {
                paddinglength = 0;
                archivenum++;
                offset = 0;
            }
            // Write the file to the archive
            using(FileStream foo = File.OpenWrite(String.Format("{0}_{1:D3}.vpk",fileprefix,archivenum)))
            {
                using(BinaryWriter bw = new BinaryWriter(foo))
                {
                    bw.Seek((int)offset,SeekOrigin.Begin);
                    for(int i=0;i<paddinglength;i++)
                        bw.Write((byte)0);
                    bw.Write(contents);
                }
            }
            // Add the padding to the offset so it properly reflects the file
            offset += (uint)paddinglength;
            // Add the file information to the directory
            string ext = internalfilename.Substring(internalfilename.IndexOf('.')+1);
            internalfilename = internalfilename.Substring(0,internalfilename.IndexOf('.'));
            char[] lookfor = {'\\','/'};
            string path = internalfilename.Substring(0,internalfilename.LastIndexOfAny(lookfor));
            string name = internalfilename.Substring(internalfilename.LastIndexOfAny(lookfor)+1);
            // Find where to put it
            VPKExt writeext = null;
            foreach(VPKExt vext in dir)
            {
                if (vext.ext == ext)
                {
                    writeext = vext;
                }
            }
            if(writeext == null) {
                writeext = new VPKExt();
                writeext.ext = ext;
                writeext.directories = new List<VPKDirectory>();
                dir.Add(writeext);
            }
            VPKDirectory writedir = null;
            foreach (VPKDirectory vpkd in writeext.directories)
            {
                if (vpkd.path == path)
                {
                    writedir = vpkd;
                }
            }
            if (writedir == null)
            {
                writedir = new VPKDirectory();
                writedir.path = path;
                writedir.ext = ext;
                writedir.entries = new List<VPKFileEntry>();
                writeext.directories.Add(writedir);
            }
            VPKFileEntry writefile = null;
            foreach(VPKFileEntry vpkfe in writedir.entries)
            {
                if (vpkfe.name == name)
                {
                    writefile = vpkfe;
                    //ok, we gotta do a file replacement
                    //thankfully, this is just overwriting a few values
                    writefile.body.archive_index = (ushort)archivenum;
                    writefile.body.CRC32 = BitConverter.ToUInt32(crc, 0);
                    writefile.body.entry_length = (uint)(contents.Length);
                    writefile.body.entry_offset = offset;
                    writefile.body.preload_bytes = 0;
                }
            }
            if (writefile == null)
            {
                // Didn't find a file entry, so let's add one
                writefile = new VPKFileEntry();
                writefile.body.archive_index = (ushort)archivenum;
                writefile.body.CRC32 = BitConverter.ToUInt32(crc, 0);
                writefile.body.entry_length = (uint)(contents.Length);
                writefile.body.entry_offset = offset;
                writefile.body.preload_bytes = 0;
                writedir.entries.Add(writefile);
            }
            // Write the new directory to the directory file.
            WriteDirectory(dir);
        }
        /// <summary>
        /// Write a new _dir.vpk file, based on a vpk directory structure.
        /// </summary>
        /// <param name="directoryinfo">A big directory information structure as a List of VPKExts</param>
        private void WriteDirectory(List<VPKExt> directoryinfo)
        {
            using(FileStream output = File.OpenWrite(fileprefix+"_dir.vpk"))
            {
                using(BinaryWriter bw = new BinaryWriter(output))
                {
                    VPKHeader headerbin = new VPKHeader(null);
                    bw.Write(StructTools.RawSerialize(headerbin));
                    foreach(VPKExt ext in directoryinfo) 
                    {
                        bw.Write(Encoding.ASCII.GetBytes(ext.ext));
                        bw.Write((byte)0); //null terminator
                        foreach(VPKDirectory dir in ext.directories)
                        {
                            bw.Write(Encoding.ASCII.GetBytes(dir.path));
                            bw.Write((byte)0);
                            foreach(VPKFileEntry fe in dir.entries)
                            {
                                bw.Write(Encoding.ASCII.GetBytes(fe.name));
                                bw.Write(StructTools.RawSerialize(fe.body));
                            }
                            bw.Write((byte)0);
                        }
                        bw.Write((byte)0);
                    }
                    // Go back and fix up the length
                    long len = bw.BaseStream.Position;
                    bw.Seek(0, SeekOrigin.Begin);
                    headerbin.tree_length = (uint)(len - 18);
                    bw.Write(StructTools.RawSerialize(headerbin));
                }
            }
        }
        /// <summary>
        /// Get the location of the end of the last file in the archive.
        /// </summary>
        /// <param name="directory">A vpk directory structure, as returned by GetDirectory</param>
        /// <returns>A Tuple, containing a pair of (number of the last archive, position in the last arvhive)</returns>
        private static Tuple<int, uint> GetNewestSpace(List<VPKExt> directory)
        {
            int lastarchive = -1;
            uint lastendoffset = 0;
            foreach(VPKExt ext in directory) {
                foreach(VPKDirectory dir in ext.directories) {
                    foreach(VPKFileEntry fe in dir.entries)
                    {
                        if (fe.body.archive_index > lastarchive)
                        {
                            lastarchive = fe.body.archive_index;
                            lastendoffset = fe.body.entry_offset + fe.body.entry_length;
                        }
                        else if (fe.body.archive_index == lastarchive && lastendoffset < fe.body.entry_offset + fe.body.entry_length)
                        {
                            lastendoffset = fe.body.entry_offset + fe.body.entry_length;
                        }
                    }
                }
            }
            return new Tuple<int, uint>(lastarchive, lastendoffset);
        }
        /// <summary>
        /// Read the directory from the file. This is mostly ported code from my [pw]
        /// vpk.h from vpktool.
        /// </summary>
        /// <returns>The VPKDirectory for the file's contents</returns>
        private List<VPKExt> GetDirectory()
        {
            byte[] body = null;
            int length = 0;
            using(FileStream vpkfile = File.OpenRead(fileprefix+"_dir.vpk"))
            {
                using(BinaryReader reader = new BinaryReader(vpkfile))
                {
                    byte[] headerbytes = reader.ReadBytes(12);
                    VPKHeader head = StructTools.RawDeserialize<VPKHeader>(headerbytes,0);
                    if (head.version != 1)
                    {
                        return null;
                    }
                    body = reader.ReadBytes((int)head.tree_length);
                    length = (int)head.tree_length;
                }
            }
            if(body == null)
            {
                return null;
            }
            List<VPKExt> ret = new List<VPKExt>();
            int index = 0;
            int level = 0;
            VPKExt curext = new VPKExt();
            VPKDirectory curpath = new VPKDirectory();
            while (index < length && level >= 0)
            {
                if (body[index] == '\0')
                {
                    switch (level)
                    {
                        case 0:
                            break;
                        case 1:
                            ret.Add(curext);
                            break;
                        case 2:
                            curext.directories.Add(curpath);
                            break;
                    }
                    index++;
                    level--;
                    continue;
                }
                switch (level)
                {
                    case 0: //ext level
                        int extstartindex = index;
                        level++;
                        while(body[index]!=0) {
                            index++;
                        }
                        index++;
                        curext = new VPKExt();
                        curext.ext = Encoding.ASCII.GetString(body,extstartindex,index-extstartindex);
                        curext.directories = new List<VPKDirectory>();
                        break;
                    case 1:
                        int pathstartindex = index;
                        level++;
                        while (body[index] != 0)
                        {
                            index++;
                        }
                        index++;
                        curpath = new VPKDirectory();
                        curpath.path = Encoding.ASCII.GetString(body, pathstartindex, index - pathstartindex);
                        curpath.ext = curext.ext;
                        curpath.entries = new List<VPKFileEntry>();
                        break;
                    case 2:
                        int filestartindex = index;
                        while (body[index] != 0)
                        {
                            index++;
                        }
                        index++;
                        VPKFileEntry vpkfile = new VPKFileEntry();
                        vpkfile.name = Encoding.ASCII.GetString(body, filestartindex, index - filestartindex);
                        vpkfile.path = curpath.path;
                        vpkfile.ext = curext.ext;
                        vpkfile.body = StructTools.RawDeserialize<VPKFile>(body, index);
                        index += 18;
                        index += vpkfile.body.preload_bytes; // we ignore preload data
                        curpath.entries.Add(vpkfile);
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// Validate the data against a reference table of filename,CRC pairs.
        /// </summary>
        /// <param name="reference">A List of filename,CRC tuples.</param>
        /// <param name="calculateChecksums">Whether or not to checksum </param>
        /// <returns>Files that need to be re-acquired. Empty if checks out.</returns>
        public List<ModResource> Validate(List<ModSpecification> reference, bool calculateChecksums)
        {
            //TODO
            return new List<ModResource>();
        }
        /// <summary>
        /// Represents a file inside a VPK
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size=18, Pack=1)]
        private struct VPKFile
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 CRC32;
            [FieldOffset(4)]
            [MarshalAs(UnmanagedType.U2)]
            public UInt16 preload_bytes;
            [FieldOffset(6)]
            [MarshalAs(UnmanagedType.U2)]
            public UInt16 archive_index;
            [FieldOffset(8)]
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 entry_offset;
            [FieldOffset(12)]
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 entry_length;
            [FieldOffset(16)]
            [MarshalAs(UnmanagedType.U2)]
            public UInt16 terminator;
            /// <summary>
            /// default constructor; parameter is due to restrictions on parameterless struct constructors.
            /// </summary>
            /// <param name="ignored">ignored</param>
            public VPKFile(object ignored)
            {
                CRC32 = 0;
                preload_bytes = 0; // we don't use the preload_bytes feature
                archive_index = 0;
                entry_offset = 0;
                entry_length = 0;
                terminator = (UInt16)0xffff;//terminator is alwyas same value
            }
            public override string ToString()
            {
                return "CRC32: " + CRC32 + "\npreload_bytes: " + preload_bytes + "\narchive_index: " + archive_index + "\nentry_offset: " + entry_offset + "\nentry_length: " + entry_length+"\n";
            }
        }
        /// <summary>
        /// Just a VPKv1 header
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size=12, Pack=1)]
        private struct VPKHeader
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 signature;
            [FieldOffset(4)]
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 version;
            [FieldOffset(8)]
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 tree_length;
            /// <summary>
            /// default constructor; parameter is due to restrictions on parameterless struct constructors.
            /// </summary>
            /// <param name="ignored">ignored</param>
            public VPKHeader(object ignored)
            {
                signature = 0x55aa1234; //magic number
                version = 1; //vpkv1
                tree_length = 0; //default value
            }
        }
        /// <summary>
        /// A quick and dirty in-memory representation of the content structure
        /// </summary>
        private class VPKExt
        {
            public String ext = "";
            public List<VPKDirectory> directories = new List<VPKDirectory>();
            public override string ToString()
            {
                return "EXT: "+ext+"\n"+directories.ToString();
            }
        }
        private class VPKDirectory
        {
            public String ext = "";
            public String path = "";
            public List<VPKFileEntry> entries;
            public override string ToString()
            {
                return "DIR: " + path + "\n" + entries.ToString();
            }
        }
        private class VPKFileEntry
        {
            public String ext = "";
            public String path = "";
            public String name = "";
            public VPKFile body;
            public override string ToString()
            {
                return "FIL: " + name + "\n" + body.ToString();
            }
        }
        /// <summary>
        /// Used from JCH2k on stackoverflow
        /// </summary>
        private static class StructTools
        {
            /// <summary>
            /// converts byte[] to struct
            /// </summary>
            public static T RawDeserialize<T>(byte[] rawData, int position)
            {
                int rawsize = Marshal.SizeOf(typeof(T));
                if (rawsize > rawData.Length - position)
                    throw new ArgumentException("Not enough data to fill struct. Array length from position: " + (rawData.Length - position) + ", Struct length: " + rawsize);
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.Copy(rawData, position, buffer, rawsize);
                T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
                Marshal.FreeHGlobal(buffer);
                return retobj;
            }

            /// <summary>
            /// converts a struct to byte[]
            /// </summary>
            public static byte[] RawSerialize(object anything)
            {
                int rawSize = Marshal.SizeOf(anything);
                IntPtr buffer = Marshal.AllocHGlobal(rawSize);
                Marshal.StructureToPtr(anything, buffer, false);
                byte[] rawDatas = new byte[rawSize];
                Marshal.Copy(buffer, rawDatas, 0, rawSize);
                Marshal.FreeHGlobal(buffer);
                return rawDatas;
            }
        }
    }
}
