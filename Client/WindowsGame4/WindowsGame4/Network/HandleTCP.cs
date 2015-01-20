///////////////////////////////////////////////////
////// MIRAGE XNA - DEVELOPED BY MARK MORRIS //////
//////          WWW.RPGCREATION.COM          //////
///////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

using MirageXNA;
using MirageXNA.Global;
using MirageXNA.Engine;

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

        public static void HandleData(NetIncomingMessage Data)
        {
            // Packet Header //
            Int32 MsgType = Data.ReadInt32();

            // Check Packet Header Number //
            if (MsgType < 0 && MsgType >= (int)ServerPackets.totalServer) { Console.WriteLine("Packet Error"); return; }

            switch (MsgType)
            {
                case (int)ServerPackets.None: break;
                case (int)ServerPackets.SSendPing: HandleSendPing(); break;
                case (int)ServerPackets.SAlertMsg: HandleAlertMsg(Data); break;
                case (int)ServerPackets.SLoginOk: HandleLoginOk(Data); break;
                case (int)ServerPackets.SPlayerMove: HandlePlayerMovement(Data); break;
                case (int)ServerPackets.SPlayerDir: HandlePlayerDirection(Data); break;
                case (int)ServerPackets.SPlayerData: HandlePlayerData(Data); break;
                case (int)ServerPackets.SMapData: HandleMapData(Data); break;
                case (int)ServerPackets.SChatMsg: HandleMapMsg(Data); break;
                case (int)ServerPackets.SCheckForMap: HandleCheckForMap(Data); break;
            }

        }

        public static void HandleSendPing()
        {
            Static.PingEnd = Environment.TickCount;
            Static.Ping = Static.PingEnd - Static.PingStart;
        }

        public static void HandleAlertMsg(NetIncomingMessage Data)
        {
            string M;

            M = Data.ReadString();
            General.menuAlertMsg(M, 1);
        }

        public static void HandleLoginOk(NetIncomingMessage Data)
        {
            // Set Index
            Static.MyIndex = Data.ReadInt32();

            // Load InGame
            UI_Game.initGameGui();
            Static.ShowGameWindow = new bool[Constant.maxWindows];

            //
            Static.EnterTextBuffer = String.Empty;

            // Show Windows On Start Up Here //
            Static.ShowGameWindow[Constant.chatWindow] = true;

            Static.menuType = Constant.inGame;
        }

        // HANDLE PLAYER MOVEMENT //
        public static void HandlePlayerMovement(NetIncomingMessage Data)
        {
            // Read Packet Information
             Int32 PlayerIndex = Data.ReadInt32();

            // Player Can Move - Update to all online //
            Types.Players[PlayerIndex].X = Data.ReadInt32();
            Types.Players[PlayerIndex].Y = Data.ReadInt32();
            Types.Players[PlayerIndex].Dir = Data.ReadInt32();
            Types.Players[PlayerIndex].Moving = Data.ReadInt32();

            Types.Players[PlayerIndex].OffsetX = 0;
            Types.Players[PlayerIndex].OffsetY = 0;

            switch (Types.Players[PlayerIndex].Dir)
            {
                case Constant.DIR_NORTH:
                    Types.Players[PlayerIndex].OffsetY = 32;
                    break;
                case Constant.DIR_SOUTH:
                    Types.Players[PlayerIndex].OffsetY = 32 * -1;
                    break;
                case Constant.DIR_WEST:
                    Types.Players[PlayerIndex].OffsetX = 32;
                    break;
                case Constant.DIR_EAST:
                    Types.Players[PlayerIndex].OffsetX = 32 * -1;
                    break;
            }
        }

        // HANDLE PLAYER MOVEMENT //
        public static void HandlePlayerDirection(NetIncomingMessage Data)
        {
            // Read Packet Information
            Int32 PlayerIndex = Data.ReadInt32();

            // Player Can Move - Update to all online //
            Types.Players[PlayerIndex].Dir = Data.ReadInt32();
            Types.Players[PlayerIndex].Moving = 0;
            Types.Players[PlayerIndex].OffsetX = 0;
            Types.Players[PlayerIndex].OffsetY = 0;
        }

        // HANDLE JOIN GAME //
        public static void HandlePlayerData(NetIncomingMessage Data)
        {
            // Read Packet Information
            Int32 PlayerIndex = Data.ReadInt32();

            // Player Can Move - Update to all online //
            Types.Players[PlayerIndex].Name = Data.ReadString();
            Types.Players[PlayerIndex].Sprite = Data.ReadInt32();
            Types.Players[PlayerIndex].Level = Data.ReadInt32();
            Types.Players[PlayerIndex].Map = Data.ReadInt32();
            Types.Players[PlayerIndex].X = Data.ReadInt32();
            Types.Players[PlayerIndex].Y = Data.ReadInt32();
            Types.Players[PlayerIndex].Dir = Data.ReadInt32();
            Types.Players[PlayerIndex].Stat1 = Data.ReadInt32();
            Types.Players[PlayerIndex].Stat2 = Data.ReadInt32();
            Types.Players[PlayerIndex].Stat3 = Data.ReadInt32();
            Types.Players[PlayerIndex].Stat4 = Data.ReadInt32();
            Types.Players[PlayerIndex].Points = Data.ReadInt32();

            // Reset Offset //
            Types.Players[PlayerIndex].OffsetX = 0;
            Types.Players[PlayerIndex].OffsetY = 0;
        }

        // HANDLE MAP DATA //
        public static void HandleMapData(NetIncomingMessage Data)
        {

            // Clear Map //
            General.clearMap();

            // Map Number //
            Int32 mapNum = Data.ReadInt32();

            // Map Info //
            Types.Map.Name = Data.ReadString();
            Types.Map.Music = Data.ReadByte();
            Types.Map.Revision = Data.ReadInt32();
            Types.Map.Moral = Data.ReadByte();
            Types.Map.Weather = Data.ReadByte();
            Types.Map.Tileset = Data.ReadInt32();
            Types.Map.Up = Data.ReadInt16();
            Types.Map.Down = Data.ReadInt16();
            Types.Map.Left = Data.ReadInt16();
            Types.Map.Right = Data.ReadInt16();
            Types.Map.BootMap = Data.ReadInt16();
            Types.Map.BootX = Data.ReadByte();
            Types.Map.BootY = Data.ReadByte();
            Types.Map.MaxX = Data.ReadByte();
            Types.Map.MaxY = Data.ReadByte();

            // Get Max X & Y //
            byte MaxX = Types.Map.MaxX;
            byte MaxY = Types.Map.MaxY;

            // Clear Map //
            General.resizeMap(MaxX, MaxY);

            // Tiles //
            for (int X = 0; X <= MaxX - 1; X++)
            {
                for (int Y = 0; Y <= MaxY - 1; Y++)
                {
                    for (int I = 0; I <= 4; I++)
                    {
                        Types.Map.Tile[X, Y].Layer[I].X = Data.ReadByte();
                        Types.Map.Tile[X, Y].Layer[I].Y = Data.ReadByte();
                        Types.Map.Tile[X, Y].Layer[I].Tileset = Data.ReadByte();
                        Types.Map.Tile[X, Y].Autotile[I] = Data.ReadByte();
                    }
                    Types.Map.Tile[X, Y].Type = Data.ReadByte();
                    Types.Map.Tile[X, Y].Data1 = Data.ReadInt32();
                    Types.Map.Tile[X, Y].Data2 = Data.ReadInt32();
                    Types.Map.Tile[X, Y].Data3 = Data.ReadInt32();
                    Types.Map.Tile[X, Y].DirBlock = Data.ReadByte();
                }
            }

            // ****** Map Npc's ******
            for (int X = 0; X <= 19; X++)
            {
                Types.Map.Npc[X] = Data.ReadInt16();
            }

            // ****** Send Map SoundID ******
            for (int X = 0; X <= MaxX - 1; X++)
            {
                for (int Y = 0; Y <= MaxY - 1; Y++)
                {
                    Types.Map.SoundID[X, Y] = Data.ReadInt16();
                }
            }

            // Cache & Save //
            General.cacheTiles();
            Autotiles.cacheAutotiles();
            General.saveMap(mapNum);
        }

        // HANDLE MAP MSG //
        public static void HandleMapMsg(NetIncomingMessage Data)
        {
            // Read Packet Information
            string Msg = Data.ReadString();
            UI_Game.AddText(Msg, 1);
        }

        // HANDLE MAP MSG //
        public static void HandleCheckForMap(NetIncomingMessage Data)
        {

            // Erase all players except self //
            for (int i = 1; i <= Constant.MAX_PLAYERS - 1; i++)
            {
                if (i != Static.MyIndex)
                {
                    Types.Players[i].Map = 0;
                }
            }

            // Get Packet Info //
            int mapNum = Data.ReadInt32();
            int mapRev = Data.ReadInt32();

            // Check If Have Have Map //
            bool needMap = true;
            DirectoryInfo dir = new DirectoryInfo("Content\\Maps\\");
            string mapDir = dir.FullName + mapNum + ".bin";
            if (File.Exists(mapDir))
            {
                General.loadMap(mapNum);
                General.cacheTiles();         // Cache Map //
                Autotiles.cacheAutotiles();     // Cache Map //
                if (Types.Map.Revision == mapRev) needMap = false;
            }

            // Send Data To Server //
            ClientTCP.SendNeedMap(needMap);
        }

    }
}
