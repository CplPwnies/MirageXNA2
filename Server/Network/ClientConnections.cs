using System;
using System.Net.Sockets;
using System.Threading;
using Lidgren.Network;

using MirageXNA.Globals;
using MirageXNA.Core;

namespace MirageXNA.Network
{    
    // ABOUT /////////////////////////////////////////////////////////////////////////
    // THIS WILL HOLD THE INFORMATION REGARDING THE CONNECTION INFO FROM THE CLIENT //
    //////////////////////////////////////////////////////////////////////////////////

    // UPDATES - 10/01/2012 ///////////////////////////////////////////
    // REMOVED THE WHOLE HANDLETCP CLASS FROM HERE. REORGANISED ALOT //
    ///////////////////////////////////////////////////////////////////

    public class clientConnection
    {
        private NetConnection tcpClient;
        private long Index;

        // Init The NetConnection //
        public clientConnection(NetConnection _TEMPSOCK)
        {
            tcpClient = _TEMPSOCK;
        }

        // NetConnection READ ONLY //
        public NetConnection netConnection
        {
            get { return tcpClient; }
        }

        // INDEX READ & WRITE //
        public long _INDEX
        {
            get { return Index; }
            set { Index = value; }
        }

    }
}
