using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpGameServer
{
    class ClientMetaInfo
    {
        public bool IsReadyForGame { get; set; } = false;
        public bool IsConnected { get; set; } = false;
        public int Number { get; set; } = 0;
        public Socket ClientSocket { get; set; } = null;
    }
}
