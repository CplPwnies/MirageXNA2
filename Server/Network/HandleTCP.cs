using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Lidgren.Network;
using MySql.Data;
using MySql.Data.MySqlClient;

using MirageXNA;
using MirageXNA.Core;
using MirageXNA.Core.Player;
using MirageXNA.Globals;
using MirageXNA.Network;

namespace MirageXNA.Network
{
    class HandleTCP
    {

        // Packets sent by client to server //
        public enum ClientPackets
        {
            None,
            CPing,
            CLogin,
            CRegister,
            CPlayerMove,
            CPlayerDir,
            CAttack,
            CMapMsg,
            CRequestNewMap,
            CNeedMap,
            CTrainStat,

            // Keep below all
            totalClient
        }

        public static void ReceivedData()
        {
            NetIncomingMessage msg;

            while ((msg = ServerTCP.sSock.ReadMessage()) != null)
            {
                // Get Incoming Index //
                int Index = Static.connectedClients.IndexOf(msg.SenderConnection);

                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        ServerTCP.sSock.SendDiscoveryResponse(null, msg.SenderEndpoint);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage: Console.WriteLine(msg.ReadString()); break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        if (status == NetConnectionStatus.Connected)
                        {
                            // New Player Connected //
                            Static.connectedClients.Add(msg.SenderConnection);
                            Index = Static.connectedClients.IndexOf(msg.SenderConnection);
                            clsGlobals.Players[Index].InGame = false;
                            clsGlobals.Players[Index].Name = null;
                            Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " connected!");
                        }
                        if (status == NetConnectionStatus.Disconnected)
                        {
                            General.leftGame(Index);
                            General.clearPlayer(Index);
                            Static.connectedClients.RemoveAt(Index);
                        }
                        break;

                    // HANDLE DATA //
                    case NetIncomingMessageType.Data: HandleTCP.HandleData(Index, msg); break;

                    default:
                        Console.WriteLine("Unhandled type: " + msg.MessageType);
                        break;
                }
                ServerTCP.sSock.Recycle(msg);
            }
        }

        // HANDLE BYTE ARRAY FROM CLIENT //
        public static void HandleData(Int32 Index, NetIncomingMessage Data)
        {
            // Packet Header //
            Int32 MsgType = Data.ReadInt32();

            // Check Packet Number
            if (MsgType < 0 && MsgType >= (int)ClientPackets.totalClient) { Console.WriteLine("Packet Error"); return; }

            switch (MsgType)
            {
                case (int)ClientPackets.None: break;
                case (int)ClientPackets.CPing: HandleCheckPing(Index); break;
                case (int)ClientPackets.CLogin: HandleLogin(Index, Data); break;
                case (int)ClientPackets.CRegister: HandleRegister(Index, Data); break;
                case (int)ClientPackets.CPlayerMove: HandlePlayerMove(Index, Data); break;
                case (int)ClientPackets.CPlayerDir: HandlePlayerDir(Index, Data); break;
                case (int)ClientPackets.CAttack: HandleAttack(Index); break;
                case (int)ClientPackets.CMapMsg: HandleMapMsg(Index, Data); break;
                case (int)ClientPackets.CRequestNewMap: HandleRequestNewMap(Index, Data); break;
                case (int)ClientPackets.CNeedMap: HandleNeedMap(Index, Data); break;
                case (int)ClientPackets.CTrainStat: HandleTrainStat(Index, Data); break;
            }
        }

        // HANDLE PING //
        public static void HandleCheckPing(Int32 Index)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerTCP.ServerPackets.SSendPing);
            ServerTCP.SendDataTo(Index, TempBuffer, NetDeliveryMethod.ReliableOrdered);
        }

        // HANDLE REGISTER //
        public static void HandleRegister(Int32 Index, NetIncomingMessage Data)
        {
            string Name; string Pass; int Sprite;

            // Read Packet Information
            Name = Data.ReadString();
            Pass = Data.ReadString();
            Sprite = Data.ReadInt32();

            // Check Account Already Exists //
            DirectoryInfo dir = new DirectoryInfo("Data\\Accounts\\");
            string accountDir = dir.FullName + Name.ToLower() + ".bin";
            if (File.Exists(accountDir))
            {
                ServerTCP.SendAlertMessage(Index, "Account already exists.");
                return;
            }

            // Create Player //
            clsGlobals.Players[Index].Name = Name;
            clsGlobals.Players[Index].Password = Pass;
            clsGlobals.Players[Index].Sprite = Sprite;

            // Player Stating Position //
            clsGlobals.Players[Index].Map = 1;
            clsGlobals.Players[Index].X = 10;
            clsGlobals.Players[Index].Y = 10;
            clsGlobals.Players[Index].Dir = Constant.DIR_SOUTH;

            // Player Stating Statistics //
            clsGlobals.Players[Index].Stat1 = 1;
            clsGlobals.Players[Index].Stat2 = 1;
            clsGlobals.Players[Index].Stat3 = 0;
            clsGlobals.Players[Index].Stat4 = 0;
            clsGlobals.Players[Index].Level = 1;
            clsGlobals.Players[Index].Exp = 0;
            clsGlobals.Players[Index].Points = 10;

            // Save Player & Login //
            clsDatabase.savePlayer(Index);

            // Send Data Back To Client
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
            TempBuffer.Write((int)ServerTCP.ServerPackets.SLoginOk);
            TempBuffer.Write(Index);
            ServerTCP.SendDataTo(Index, TempBuffer, NetDeliveryMethod.ReliableOrdered);
            TempBuffer = null;

            // JOIN THE GAME //
            General.joinGame(Index);
        }

        // HANDLE LOGIN //
        public static void HandleLogin(Int32 Index, NetIncomingMessage Data)
        {
            string Login_Name = null;
            string Login_Pass = null;
            string tempPass = null;

            Login_Name = Data.ReadString();
            Login_Pass = Data.ReadString();

            DirectoryInfo dir = new DirectoryInfo("Data\\Accounts\\");
            string accountDir = dir.FullName + Login_Name.ToLower() + ".bin";
            if (File.Exists(accountDir))
            {
                using (FileStream stream = new FileStream(accountDir, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        tempPass = reader.ReadString();
                        tempPass = reader.ReadString();
                        reader.Close();
                    }
                }
            }
            else
            {
                ServerTCP.SendAlertMessage(Index, "Account does not exists.");
                return;
            }

            if (tempPass == Login_Pass)
            {

                if (clsDatabase.loadPlayer(Index, Login_Name))
                {
                    // Account Exist - Just log in //
                    ServerTCP.SendAlertMessage(Index, "Login Succesful. Joining Game...");

                    // Send Data Back To Client
                    NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();
                    TempBuffer.Write((int)ServerTCP.ServerPackets.SLoginOk);
                    TempBuffer.Write(Index);
                    ServerTCP.SendDataTo(Index, TempBuffer, NetDeliveryMethod.ReliableOrdered);
                    TempBuffer = null;

                    // JOIN THE GAME //
                    General.joinGame(Index);
                }
                else
                {
                    ServerTCP.SendAlertMessage(Index, "Account does not exists.");
                    return;
                }

            }
            else
            {
                ServerTCP.SendAlertMessage(Index, "Password Incorrect.");
                return;
            }

        }

        // HANDLE PLAYER MOVEMENT //
        public static void HandlePlayerMove(Int32 Index, NetIncomingMessage Data)
        {
            int X; int Y; int Dir; int Moving;

            // Read Packet Information
            Dir = Data.ReadInt32();
            Moving = Data.ReadInt32();
            X = Data.ReadInt32();
            Y = Data.ReadInt32();

            // Player Can Move - Update to all online //
            clsGlobals.Players[Index].Dir = Dir;
            clsGlobals.Players[Index].X = X;
            clsGlobals.Players[Index].Y = Y;

            clsGlobals.PlayerMove(Index, Dir, Moving);
        }

        // HANDLE PLAYER MOVEMENT //
        public static void HandlePlayerDir(Int32 Index, NetIncomingMessage Data)
        {
            // Read Packet Information
            int Dir = Data.ReadInt32();

            // Player Can Move - Update to all online //
            clsGlobals.Players[Index].Dir = Dir;

            // Send Message //
            ServerTCP.SendPlayerDirectionToAllBut(Index, Dir);
        }

        // HANDLE PLAYER ATTACK //
        public static void HandleAttack(Int32 Index)
        {
            int TempIndex = 0;

            // ######################################
            // ##      Player Attacking Player     ##
            // ######################################

            for (int LoopI = 1; LoopI <= 99; LoopI++)
            {
                TempIndex = LoopI;
                if (TempIndex != Index)
                {
                    Combat.Combat_PlayerAttackPlayer(Index, LoopI);
                }
            }

        }

        // HANDLE PLAYER MOVEMENT //
        public static void HandleMapMsg(Int32 Index, NetIncomingMessage Data)
        {
            // Read Packet Information
            string Msg = Data.ReadString();

            // Add Name to Message //
            Msg = clsGlobals.Players[Index].Name + ": " + Msg;

            // Send Message //
            ServerTCP.SendMapMsg(Index, Msg);
        }

        // Player request for a new map //
        public static void HandleRequestNewMap(Int32 Index, NetIncomingMessage Data)
        {
            int Dir = Data.ReadInt32();

            // Prevent hacking
            if (Dir < Constant.DIR_NORTH || Dir > Constant.DIR_EAST)
            {
                return;
            }

            clsGlobals.PlayerMove(Index, Dir, 1);
        }

        // HANDLE NEED MAP //
        public static void HandleNeedMap(Int32 Index, NetIncomingMessage Data)
        {
            bool needMap = Data.ReadBoolean();

            if (needMap)
            {
                ServerTCP.SendMap(Index, Commands.getPlayerMap(Index));
            }

            // Join Map //
            ServerTCP.SendJoinMap(Index);
        }

        // HANDLE TRAIN STAT //
        public static void HandleTrainStat(Int32 Index, NetIncomingMessage Data)
        {
            // Check if we have points //
            if (clsGlobals.Players[Index].Points == 0)
            {
                ServerTCP.SendPlayerMsg(Index, "ALERT: You don't have any points");
                return;
            }

            // Temp Stat //
            byte statNum = Data.ReadByte();

            switch (statNum)
            {
                case 1: clsGlobals.Players[Index].Stat1 = clsGlobals.Players[Index].Stat1 + 1;
                    ServerTCP.SendPlayerMsg(Index, "ALERT: You have trained your Strength!");
                    break;
                case 2: clsGlobals.Players[Index].Stat2 = clsGlobals.Players[Index].Stat2 + 1;
                    ServerTCP.SendPlayerMsg(Index, "ALERT: You have trained your Defence!");
                    break;
                case 3: clsGlobals.Players[Index].Stat3 = clsGlobals.Players[Index].Stat3 + 1;
                    ServerTCP.SendPlayerMsg(Index, "ALERT: You have trained your Magic!");
                    break;
                case 4: clsGlobals.Players[Index].Stat4 = clsGlobals.Players[Index].Stat4 + 1;
                    ServerTCP.SendPlayerMsg(Index, "ALERT: You have trained your Speed!");
                    break;
            }

            // Remove Point //
            clsGlobals.Players[Index].Points = clsGlobals.Players[Index].Points - 1;

            // Update Self //
            ServerTCP.SendDataToMap(clsGlobals.Players[Index].Map, ServerTCP.PlayerData(Index));
        }

    }
}
