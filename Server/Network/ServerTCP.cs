using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;	// Used to local machine info
using MirageXNA;
using MirageXNA.Network;
using MirageXNA.Core;
using MirageXNA.Globals;
using Lidgren.Network;

namespace MirageXNA.Network
{
    class ServerTCP
    {

        // Packets sent by server to client //
        public enum ServerPackets
        {
            None,
            SSendPing,
            SAlertMsg,
            SLoginOk,
            SPlayerMove,
            SPlayerDir,
            SPlayerData,
            SMapData,
            SChatMsg,
            SCheckForMap,

            // Keep below all
            totalServer
        }

        public static NetServer sSock;

        /////////////////////////////
        ////// INIT SERVER TCP //////
        /////////////////////////////
        public static void initServerTCP()
        {
            // Dummy Client - Start Indexed From 1 //
            IPAddress ip = System.Net.IPAddress.Parse("127.0.0.1");
            clientConnection client = null;
            Static.connectedClients.Add(client);

            // SERVER TCP CONFIG //
            NetPeerConfiguration config = new NetPeerConfiguration("MirageXNA");
            //config.LocalAddress = ip;
            config.Port = 7001;
            config.AcceptIncomingConnections = true;
            config.MaximumConnections = 100;
            config.ConnectionTimeout = 5;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            sSock = new NetServer(config);
            sSock.Start();
            sSock.Socket.Blocking = false;
        }

        // Method to send the message //
        public static void SendDataTo(int Index, NetOutgoingMessage Data, NetDeliveryMethod deliveryMethod)
        {
            if (Index > Static.connectedClients.Count - 1) return;
            NetOutgoingMessage data = ServerTCP.sSock.CreateMessage(); data.Write(Data);
            ServerTCP.sSock.SendMessage(data, (NetConnection)Static.connectedClients[Index], deliveryMethod);
        }

        // Method to send the message to all //
        public static void SendDataToAll(NetOutgoingMessage Data)
        {
            for (int LoopI = 1; LoopI <= Constant.MAX_PLAYERS; LoopI++)
            {
                if (clsGlobals.Players[LoopI].InGame)
                {
                    SendDataTo(LoopI, Data, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        // Method to send the message to all but index //
        public static void SendDataToAllBut(int Index, NetOutgoingMessage Data)
        {
            for (int LoopI = 1; LoopI <= Constant.MAX_PLAYERS; LoopI++)
            {
                if (clsGlobals.Players[LoopI].InGame)
                {
                    if (LoopI != Index)
                    {
                        SendDataTo(LoopI, Data, NetDeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }

        // Method to send the message to map //
        public static void SendDataToMap(int mapNum, NetOutgoingMessage Data)
        {
            for (int LoopI = 1; LoopI <= Constant.MAX_PLAYERS; LoopI++)
            {
                if (clsGlobals.Players[LoopI].InGame)
                {
                    if (Commands.getPlayerMap(LoopI) == mapNum)
                    {
                        SendDataTo(LoopI, Data, NetDeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }

        // Method to send the message to map but index //
        public static void SendDataToMapBut(int Index, int mapNum, NetOutgoingMessage Data)
        {
            for (int LoopI = 1; LoopI <= Constant.MAX_PLAYERS; LoopI++)
            {
                if (clsGlobals.Players[LoopI].InGame)
                {
                    if (Commands.getPlayerMap(LoopI) == mapNum)
                    {
                        if (LoopI != Index)
                        {
                            SendDataTo(LoopI, Data, NetDeliveryMethod.ReliableOrdered);
                        }
                    }
                }
            }
        }

        public static void SendAlertMessage(Int32 Index, String Message)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SAlertMsg);
            TempBuffer.Write(Message);
            SendDataTo(Index, TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        public static void SendPlayerMovement(Int32 Index, Int32 Movement)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SPlayerMove);
            TempBuffer.Write(Index);
            TempBuffer.Write(clsGlobals.Players[Index].X);
            TempBuffer.Write(clsGlobals.Players[Index].Y);
            TempBuffer.Write(clsGlobals.Players[Index].Dir);
            TempBuffer.Write(Movement);
            SendDataToAllBut(Index, TempBuffer);
        }

        public static void SendPlayerDirectionToAllBut(Int32 Index, Int32 Direction)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SPlayerDir);
            TempBuffer.Write(Index);
            TempBuffer.Write(Direction);
            SendDataToMapBut(Index, Commands.getPlayerMap(Index), TempBuffer);
        }

        public static void SendJoinMap(int Index)
        {
            // Send all players on current map to index
            for (int LoopI = 1; LoopI <= Constant.MAX_PLAYERS; LoopI++)
            {
                if (clsGlobals.IsPlaying(LoopI))
                {
                    if (LoopI != Index)
                    {
                        if (clsGlobals.Players[LoopI].Map == clsGlobals.Players[Index].Map)
                        {
                            SendDataTo(Index, PlayerData(LoopI), NetDeliveryMethod.ReliableOrdered);
                            Thread.Sleep(1);
                        }
                    }
                }
            }

            // Send index's player data to everyone on the map including himself
            SendDataToAll(PlayerData(Index));
        }

        public static void SendMap(Int32 Index, Int32 mapNum)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SMapData);
            TempBuffer.Write(clsTypes.mapCache[mapNum].Data);
            SendDataTo(Index, TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        public static void SendLeftGame(Int32 Index)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SPlayerData);
            TempBuffer.Write(Index);
            TempBuffer.Write("");
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            TempBuffer.Write(0);
            SendDataToAllBut(Index, TempBuffer);
        }

        // Send Message To Player Only //
        public static void SendPlayerMsg(Int32 Index, String Msg)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SChatMsg);
            TempBuffer.Write(Msg);
            SendDataTo(Index, TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Send Message To Map //
        public static void SendMapMsg(Int32 Index, String Msg)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SChatMsg);
            TempBuffer.Write(Msg);
            SendDataToMap(clsGlobals.Players[Index].Map, TempBuffer);
        }

        // Usable Byte Array Data //
        public static NetOutgoingMessage PlayerData(Int32 Index)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SPlayerData);
            TempBuffer.Write(Index);
            TempBuffer.Write(clsGlobals.Players[Index].Name);
            TempBuffer.Write(clsGlobals.Players[Index].Sprite);
            TempBuffer.Write(clsGlobals.Players[Index].Level);
            TempBuffer.Write(clsGlobals.Players[Index].Map);
            TempBuffer.Write(clsGlobals.Players[Index].X);
            TempBuffer.Write(clsGlobals.Players[Index].Y);
            TempBuffer.Write(clsGlobals.Players[Index].Dir);
            TempBuffer.Write(clsGlobals.Players[Index].Stat1);
            TempBuffer.Write(clsGlobals.Players[Index].Stat2);
            TempBuffer.Write(clsGlobals.Players[Index].Stat3);
            TempBuffer.Write(clsGlobals.Players[Index].Stat4);
            TempBuffer.Write(clsGlobals.Players[Index].Points);
            return TempBuffer;
        }

        // Check The Client To See if He Already Has The Map //
        public static void SendCheckMap(Int32 Index, Int32 mapNum, Int32 Revision)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerPackets.SCheckForMap);
            TempBuffer.Write(mapNum);
            TempBuffer.Write(Revision);
            SendDataTo(Index, TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

    }
}