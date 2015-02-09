using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

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
                using(FileStream headfile = File.OpenWrite(fileprefix + "_dir.vpk"))
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
            
        }
        /// <summary>
        /// Validate the data against a reference table of filename,CRC pairs.
        /// </summary>
        /// <param name="reference">A List of filename,CRC tuples.</param>
        /// <param name="calculateChecksums">Whether or not to checksum </param>
        /// <returns>Files that need to be re-acquired. Empty if checks out.</returns>
        public List<string> Validate(List<Tuple<string,UInt32>> reference, bool calculateChecksums)
        {
            return new List<string>();
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
        private class VPKDirectory
        {
            VPKDirectory[] childDirectories;
            VPKFile[] files;
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
