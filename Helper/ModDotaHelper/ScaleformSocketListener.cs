using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ModDotaHelper
{
    /// <summary>
    /// Sets up a local socket, and listens for requests from the scaleform on it.
    /// </summary>
    class ScaleformSocketListener
    {
        private Socket communicationSocket;
        private object temp = null;
        private ManualResetEvent startuplocker = new ManualResetEvent(false);
        public ScaleformSocketListener()
        {
            // We're a worker
            ModDotaHelper.workersactive.AddCount();
            Thread workerthread = new Thread(SocketThread);
            // Wait for the thread to start up
            startuplocker.WaitOne();
        }
        /// <summary>
        /// The type of a networked message
        /// </summary>
        enum MessageType : uint
        {
            Ping = 0,
            Pong = 1,
            SubscribeToMod = 2,
            UnsubscribeFromMod = 3,
            SubscriptionStatusChanged = 4,
            Error = 9999,
        }
        /// <summary>
        /// Describes the status of whether or not a particular mod is installed
        /// On change, download the .mod or delete it, and then quick-verify all
        /// installed files.
        /// </summary>
        enum SubscriptionStatus : uint
        {
            Uninstalled = 0,
            Installed = 1,
        }
        private void SocketThread(object state)
        {
            try
            {
                ModDotaHelper.workersactive.AddCount();
                // Make a socket
                communicationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                communicationSocket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 25788));
                communicationSocket.Listen(2);
                byte[] buffer = new byte[256];
                // Check if we're shutting down
                while (ModDotaHelper.closedown.WaitOne(0) == false)
                {
                    int len = communicationSocket.Receive(buffer);
                    // Message header is 8 bytes long
                    if (len < 8)
                    {
                        continue;
                    }
                    BinaryReader br = new BinaryReader(new MemoryStream(buffer));
                    UInt32 messagetype = br.ReadUInt32();
                    UInt32 length = br.ReadUInt32();
                    if (length != len - 8)
                        continue;
                    switch (messagetype)
                    {
                        case (uint)MessageType.Ping:
                            {
                                byte[] response = new byte[8];
                                BinaryWriter bw = new BinaryWriter(new MemoryStream(response));
                                bw.Write((UInt32)MessageType.Pong);
                                bw.Write((UInt32)0);
                                bw.Flush();
                                communicationSocket.Send(response);
                                break;
                            }
                        case (uint)MessageType.Pong:
                            // Don't really care
                            break;
                        case (uint)MessageType.SubscribeToMod:
                            {
                                // Gotta read this message's header
                                if (length < 6)
                                    continue;
                                SubscriptionStatus ss = (SubscriptionStatus)br.ReadUInt32();
                                UInt16 namelen = br.ReadUInt16();
                                if (2 * namelen > length - (6 + 8))
                                    continue;
                                string name = new string(br.ReadChars(namelen));
                                bool installed = ModDotaHelper.modman.GetInstallStatus(name);
                                if (installed)
                                {
                                    // it's already installed, don't do anything
                                }
                                else
                                {
                                    ModDotaHelper.modman.Install(name);
                                }
                                UInt32 responselength = (UInt32)(6 + 2 * namelen);
                                byte[] response = new byte[8 + responselength];
                                BinaryWriter bw = new BinaryWriter(new MemoryStream(response));
                                bw.Write((UInt32)MessageType.SubscriptionStatusChanged);
                                bw.Write(responselength);
                                bw.Write((UInt16)namelen);
                                bw.Write(name.ToCharArray());
                                communicationSocket.Send(response);
                                break;
                            }
                        case (uint)MessageType.SubscriptionStatusChanged:
                            // we're supposed to be the ones sending this...
                        default:
                            // Unknown message type, return error
                            {
                                string message = "Unknown Message Type "+messagetype.ToString();
                                UInt32 responselength = (UInt32)(2 + message.Length * 2);
                                byte[] response = new byte[8 + responselength];
                                BinaryWriter bw = new BinaryWriter(new MemoryStream(response));
                                bw.Write((UInt32)MessageType.Error);
                                bw.Write(responselength);
                                bw.Write((UInt16)message.Length);
                                bw.Write(message.ToCharArray());
                                communicationSocket.Send(response);
                            }
                    }
                }
            }
            finally
            {
                ModDotaHelper.workersactive.Signal();
            }
        }
        private void HandleMessage(object state)
        {
            using (BinaryReader reader = new BinaryReader((Stream)state))
            {
                UInt32 messageid = reader.ReadUInt32();
                UInt32 length = reader.ReadUInt32();
                switch (messageid)
                {
                    case 0:
                        // "are you there" test
                        break;
                    default:
                        // dunno
                        Console.WriteLine("Uknown message recieved with id " + messageid.ToString());
                        break;
                }
            }
        }
    }
}
