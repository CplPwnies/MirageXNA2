///////////////////////////////////////////////////
////// MIRAGE XNA - DEVELOPED BY MARK MORRIS //////
//////          WWW.RPGCREATION.COM          //////
///////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

using MirageXNA.Global;
using MirageXNA.Engine;

namespace MirageXNA.Network
{
    class ClientTCP
    {
        ///////////////////////////
        ////// TCP Functions //////
        ///////////////////////////

        // Check if player exists //
        public static bool IsPlaying(Int32 Index)
        {
            if (Types.Players[Index].Name == null) { return false; }
            if (Types.Players[Index].Name.Length > 0) { return true; }
            return false;
        }

        // Check if player is connected //
        public static bool IsConnected()
        {
            if (Mirage.client.ConnectionStatus == NetConnectionStatus.Connected) return true; else return false;
        }

        ///////////////////////
        ////// SEND DATA //////
        ///////////////////////
        public static void SendData(NetOutgoingMessage Data, NetDeliveryMethod deliveryMethod)
        {
            Mirage.client.SendMessage(Data, deliveryMethod);
        }

        /////////////////////
        ////// PACKETS //////
        /////////////////////

        // Send Request Ping //
        public static void SendGetPing()
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            Static.PingStart = Environment.TickCount;
            TempBuffer.Write((int)HandleTCP.ClientPackets.CPing);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Send Login //
        public static void SendLogin(string lName, string lPass)
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            TempBuffer.Write((int)HandleTCP.ClientPackets.CLogin);
            TempBuffer.Write(lName);
            TempBuffer.Write(lPass);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Send New Registration //
        public static void SendRegister(string rName, string rPass, int rSprite)
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            TempBuffer.Write((int)HandleTCP.ClientPackets.CRegister);
            TempBuffer.Write(rName);
            TempBuffer.Write(rPass);
            TempBuffer.Write(rSprite);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Send Player Movement //
        public static void SendPlayerMove()
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            TempBuffer.Write((int)HandleTCP.ClientPackets.CPlayerMove);
            TempBuffer.Write(Types.Players[Static.MyIndex].Dir);
            TempBuffer.Write(Types.Players[Static.MyIndex].Moving);
            TempBuffer.Write(Types.Players[Static.MyIndex].X);
            TempBuffer.Write(Types.Players[Static.MyIndex].Y);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Send Player Direction //
        public static void SendPlayerDir()
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            TempBuffer.Write((int)HandleTCP.ClientPackets.CPlayerDir);
            TempBuffer.Write(Types.Players[Static.MyIndex].Dir);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Say Message //
        public static void MapMsg(string Msg)
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            TempBuffer.Write((int)HandleTCP.ClientPackets.CMapMsg);
            TempBuffer.Write(Msg);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Request New Map //
        public static void SendPlayerRequestNewMap()
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            TempBuffer.Write((int)HandleTCP.ClientPackets.CRequestNewMap);
            TempBuffer.Write(Types.Players[Static.MyIndex].Dir);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Send Need Map //
        public static void SendNeedMap(bool needMap)
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            TempBuffer.Write((int)HandleTCP.ClientPackets.CNeedMap);
            TempBuffer.Write(needMap);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // Send Train Stat //
        public static void SendTrainStat(byte statNum)
        {
            NetOutgoingMessage TempBuffer = Mirage.client.CreateMessage();
            TempBuffer.Write((int)HandleTCP.ClientPackets.CTrainStat);
            TempBuffer.Write(statNum);
            SendData(TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

    }
}
