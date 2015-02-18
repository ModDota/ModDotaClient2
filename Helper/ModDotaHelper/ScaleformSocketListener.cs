using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace ModDotaHelper
{
    /// <summary>
    /// Sets up a local socket, and listens for requests from the scaleform on it.
    /// </summary>
    class ScaleformSocketListener
    {
        private Socket communicationSocket;
        private object temp = null;
        public ScaleformSocketListener()
        {
            communicationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            communicationSocket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 25788));
            communicationSocket.BeginAccept(HandleMessage,temp);
        }
        private void HandleMessage(object state)
        {
            using(BinaryReader reader = new BinaryReader((Stream)state))
            {
                UInt32 messageid = reader.ReadUInt32();
                UInt32 length = reader.ReadUInt32();
                switch(messageid)
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
